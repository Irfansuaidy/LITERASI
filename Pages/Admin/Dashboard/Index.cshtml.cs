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
    public string ActiveAcademicYear { get; set; } = "-";

    public List<TeachingAssignmentDto> TeachingAssignments { get; set; }
        = new();

    public async Task OnGetAsync()
    {
        TotalStudents =
            await _context.Students.CountAsync();

        TotalTeachers =
            await _context.Teachers.CountAsync();

        TotalClasses =
            await _context.Classes.CountAsync();

        TotalSubjects =
            await _context.Subjects.CountAsync();

        var activeYear =
            await _context.AcademicYears
                .FirstOrDefaultAsync(a => a.IsActive);

        if (activeYear != null)
        {
            ActiveAcademicYear = activeYear.YearName;
        }

        TeachingAssignments =
            await _context.TeachingAssignments
                .Include(t => t.Teacher)
                    .ThenInclude(t => t.User)
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
    }

    public class TeachingAssignmentDto
    {
        public string TeacherName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}