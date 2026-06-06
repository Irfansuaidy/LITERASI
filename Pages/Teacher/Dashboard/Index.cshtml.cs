using Literasi.Data;
using Literasi.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Literasi.Pages.Teacher.Dashboard;

[Authorize(Roles = "Guru")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public string TeacherName { get; set; } = "Guru";
    public string ActiveAcademicYear { get; set; } = "-";
    public int TotalTeachingAssignments { get; set; }
    public int TotalClasses { get; set; }
    public int TotalLearningMaterials { get; set; }
    public int TotalAssignments { get; set; }
    public int TotalSubmissions { get; set; }
    public int PendingGrades { get; set; }
    public int ReviewedSubmissions { get; set; }

    public List<TeachingAssignmentDto> TeachingAssignments { get; set; } = new();
    public List<RecentMaterialDto> RecentMaterials { get; set; } = new();
    public List<RecentSubmissionDto> RecentSubmissions { get; set; } = new();
    public List<PendingAssignmentGradeDto> PendingAssignmentGrades { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var teacher = await _context.Teachers
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (teacher == null)
        {
            return;
        }

        TeacherName = teacher.User.FullName;

        var activeYear = await _context.AcademicYears
            .FirstOrDefaultAsync(a => a.IsActive);

        if (activeYear != null)
        {
            ActiveAcademicYear = activeYear.YearName;
        }

        TotalTeachingAssignments = await _context.TeachingAssignments
            .CountAsync(t => t.TeacherId == teacher.TeacherId);

        TotalClasses = await _context.TeachingAssignments
            .Where(t => t.TeacherId == teacher.TeacherId)
            .Select(t => t.ClassId)
            .Distinct()
            .CountAsync();

        TotalLearningMaterials = await _context.LearningMaterials
            .CountAsync(m => m.TeachingAssignment.TeacherId == teacher.TeacherId);

        TotalAssignments = await _context.Assignments
            .CountAsync(a => a.TeachingAssignment.TeacherId == teacher.TeacherId);

        TotalSubmissions = await _context.Submissions
            .CountAsync(s => s.Assignment.TeachingAssignment.TeacherId == teacher.TeacherId);

        PendingGrades = await _context.Submissions
            .CountAsync(s =>
                s.Assignment.TeachingAssignment.TeacherId == teacher.TeacherId &&
                s.Grade == null);

        ReviewedSubmissions = await _context.Submissions
            .CountAsync(s =>
                s.Assignment.TeachingAssignment.TeacherId == teacher.TeacherId &&
                s.Status == SubmissionStatus.Reviewed);

        TeachingAssignments = await _context.TeachingAssignments
            .Where(t => t.TeacherId == teacher.TeacherId)
            .OrderBy(t => t.Subject.SubjectName)
            .ThenBy(t => t.Class.ClassName)
            .Select(t => new TeachingAssignmentDto
            {
                SubjectName = t.Subject.SubjectName,
                ClassName = t.Class.ClassName,
                MaterialCount = t.LearningMaterials.Count,
                AssignmentCount = t.Assignments.Count,
                StudentCount = t.Class.Students.Count
            })
            .ToListAsync();

        PendingAssignmentGrades = await _context.Assignments
            .Where(a =>
                a.TeachingAssignment.TeacherId == teacher.TeacherId &&
                a.Submissions.Any(s => s.Grade == null))
            .OrderByDescending(a => a.Submissions.Count(s => s.Grade == null))
            .ThenBy(a => a.Deadline)
            .Take(5)
            .Select(a => new PendingAssignmentGradeDto
            {
                AssignmentId = a.AssignmentId,
                Title = a.Title,
                SubjectName = a.TeachingAssignment.Subject.SubjectName,
                ClassName = a.TeachingAssignment.Class.ClassName,
                PendingCount = a.Submissions.Count(s => s.Grade == null)
            })
            .ToListAsync();

        RecentMaterials = await _context.LearningMaterials
            .Where(m => m.TeachingAssignment.TeacherId == teacher.TeacherId)
            .OrderByDescending(m => m.UploadedAt)
            .Take(5)
            .Select(m => new RecentMaterialDto
            {
                Title = m.Title,
                MaterialType = m.MaterialType.ToString(),
                SubjectName = m.TeachingAssignment.Subject.SubjectName,
                ClassName = m.TeachingAssignment.Class.ClassName,
                UploadedAt = m.UploadedAt
            })
            .ToListAsync();

        RecentSubmissions = await _context.Submissions
            .Where(s => s.Assignment.TeachingAssignment.TeacherId == teacher.TeacherId)
            .OrderByDescending(s => s.SubmittedAt)
            .Take(5)
            .Select(s => new RecentSubmissionDto
            {
                StudentName = s.Student.User.FullName,
                ClassName = s.Student.Class.ClassName,
                AssignmentTitle = s.Assignment.Title,
                SubmittedAt = s.SubmittedAt,
                Status = s.Status.ToString()
            })
            .ToListAsync();
    }

    public class TeachingAssignmentDto
    {
        public string SubjectName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int MaterialCount { get; set; }
        public int AssignmentCount { get; set; }
        public int StudentCount { get; set; }
    }

    public class PendingAssignmentGradeDto
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int PendingCount { get; set; }
    }

    public class RecentMaterialDto
    {
        public string Title { get; set; } = string.Empty;
        public string MaterialType { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class RecentSubmissionDto
    {
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
