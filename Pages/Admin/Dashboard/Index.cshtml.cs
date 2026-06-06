using Literasi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Admin.Dashboard;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalClasses { get; set; }
    public int TotalSubjects { get; set; }
    public int TotalMaterials { get; set; }
    public int TotalAssignments { get; set; }
    public int TotalSubmissions { get; set; }
    public string ActiveAcademicYear { get; set; } = "-";

    public List<TeachingAssignmentDto> TeachingAssignments { get; set; } = new();
    public List<RecentMaterialDto> RecentMaterials { get; set; } = new();
    public List<RecentSubmissionDto> RecentSubmissions { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalStudents = await _context.Students.CountAsync();
        TotalTeachers = await _context.Teachers.CountAsync();
        TotalClasses = await _context.Classes.CountAsync();
        TotalSubjects = await _context.Subjects.CountAsync();
        
        TotalMaterials = await _context.LearningMaterials.CountAsync();
        TotalAssignments = await _context.Assignments.CountAsync();
        TotalSubmissions = await _context.Submissions.CountAsync();

        var activeYear = await _context.AcademicYears
            .FirstOrDefaultAsync(a => a.IsActive);

        if (activeYear != null)
        {
            ActiveAcademicYear = activeYear.YearName;
        }

        TeachingAssignments = await _context.TeachingAssignments
            .Include(t => t.Teacher.User)
            .Include(t => t.Subject)
            .Include(t => t.Class)
            .Take(5)
            .Select(t => new TeachingAssignmentDto
            {
                TeacherName = t.Teacher.User.FullName,
                SubjectName = t.Subject.SubjectName,
                ClassName = t.Class.ClassName,
                IsActive = true
            })
            .ToListAsync();

        RecentMaterials = await _context.LearningMaterials
            .Include(m => m.TeachingAssignment.Teacher.User)
            .Include(m => m.TeachingAssignment.Subject)
            .Include(m => m.TeachingAssignment.Class)
            .OrderByDescending(m => m.UploadedAt)
            .Take(5)
            .Select(m => new RecentMaterialDto
            {
                MaterialId = m.MaterialId,
                Title = m.Title,
                MaterialType = m.MaterialType.ToString(),
                TeacherName = m.TeachingAssignment.Teacher.User.FullName,
                SubjectName = m.TeachingAssignment.Subject.SubjectName,
                ClassName = m.TeachingAssignment.Class.ClassName,
                UploadedAt = m.UploadedAt
            })
            .ToListAsync();

        RecentSubmissions = await _context.Submissions
            .Include(s => s.Student.User)
            .Include(s => s.Student.Class)
            .Include(s => s.Assignment)
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
        public string TeacherName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class RecentMaterialDto
    {
        public int MaterialId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string MaterialType { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
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