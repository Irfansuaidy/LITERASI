using Literasi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Admin.Classes;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<ClassDto> Classes { get; set; } = new();

    public async Task OnGetAsync()
    {
        Classes = await _context.Classes
            .Include(c => c.GradeLevel)
            .Include(c => c.AcademicYear)
            .Include(c => c.HomeroomTeacher)
                .ThenInclude(t => t!.User)
            .Include(c => c.Students)
            .OrderBy(c => c.AcademicYear.YearName)
            .ThenBy(c => c.GradeLevel.LevelName)
            .ThenBy(c => c.ClassName)
            .Select(c => new ClassDto
            {
                ClassId        = c.ClassId,
                ClassName      = c.ClassName,
                GradeLevel     = c.GradeLevel.LevelName,
                AcademicYear   = c.AcademicYear.YearName,
                HomeroomTeacher = c.HomeroomTeacher != null
                    ? c.HomeroomTeacher.User.FullName
                    : null,
                StudentCount   = c.Students.Count
            })
            .ToListAsync();
    }

    public class ClassDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string? HomeroomTeacher { get; set; }
        public int StudentCount { get; set; }
    }
}
