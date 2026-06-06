using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Admin.Reports;

[Authorize(Roles = "Admin")]
public class FinalGradesModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public FinalGradesModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public int? ClassId { get; set; }

    public SelectList ClassOptions { get; set; } = null!;

    public List<SubjectGradeColumn> SubjectColumns { get; set; } = new();
    public List<StudentGradeRow> GradeMatrix { get; set; } = new();

    public string SelectedClassName { get; set; } = string.Empty;
    public string SelectedAcademicYear { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        await PopulateClassesAsync();

        if (ClassId.HasValue)
        {
            var selectedClass = await _context.Classes
                .Include(c => c.AcademicYear)
                .FirstOrDefaultAsync(c => c.ClassId == ClassId.Value);

            if (selectedClass != null)
            {
                SelectedClassName = selectedClass.ClassName;
                SelectedAcademicYear = selectedClass.AcademicYear.YearName;
            }

            SubjectColumns = await _context.TeachingAssignments
                .Where(t => t.ClassId == ClassId.Value)
                .OrderBy(t => t.Subject.SubjectName)
                .Select(t => new SubjectGradeColumn
                {
                    SubjectId = t.SubjectId,
                    SubjectName = t.Subject.SubjectName
                })
                .Distinct()
                .ToListAsync();

            // Fetch students with their submissions & grades
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Submissions)
                    .ThenInclude(sub => sub.Assignment)
                        .ThenInclude(a => a.TeachingAssignment)
                            .ThenInclude(t => t.Subject)
                .Include(s => s.Submissions)
                    .ThenInclude(sub => sub.Grade)
                .Where(s => s.ClassId == ClassId.Value)
                .OrderBy(s => s.User.FullName)
                .ToListAsync();

            // Build matrix
            foreach (var student in students)
            {
                var row = new StudentGradeRow
                {
                    StudentName = student.User.FullName,
                    Nisn = student.Nisn
                };

                decimal totalScore = 0;
                int gradedCount = 0;

                foreach (var subject in SubjectColumns)
                {
                    var subjectScores = student.Submissions
                        .Where(sub =>
                            sub.Assignment.TeachingAssignment.SubjectId == subject.SubjectId &&
                            sub.Grade != null)
                        .Select(sub => sub.Grade!.Score)
                        .ToList();

                    decimal? score = subjectScores.Any()
                        ? Math.Round(subjectScores.Average(), 2)
                        : null;

                    row.SubjectScores[subject.SubjectId] = score;

                    if (score.HasValue)
                    {
                        totalScore += score.Value;
                        gradedCount++;
                    }
                }

                row.AverageScore = gradedCount > 0 ? Math.Round(totalScore / gradedCount, 2) : 0;
                GradeMatrix.Add(row);
            }
        }
    }

    private async Task PopulateClassesAsync()
    {
        var classes = await _context.Classes
            .Include(c => c.GradeLevel)
            .Include(c => c.AcademicYear)
            .OrderByDescending(c => c.AcademicYear.IsActive)
            .ThenBy(c => c.AcademicYear.YearName)
            .ThenBy(c => c.ClassName)
            .Select(c => new
            {
                c.ClassId,
                DisplayName = $"{c.ClassName} ({c.AcademicYear.YearName})" + (c.AcademicYear.IsActive ? " - Aktif" : "")
            })
            .ToListAsync();

        ClassOptions = new SelectList(classes, "ClassId", "DisplayName");
    }

    public class SubjectGradeColumn
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
    }

    public class StudentGradeRow
    {
        public string StudentName { get; set; } = string.Empty;
        public string Nisn { get; set; } = string.Empty;
        public Dictionary<int, decimal?> SubjectScores { get; set; } = new();
        public decimal AverageScore { get; set; }
    }
}
