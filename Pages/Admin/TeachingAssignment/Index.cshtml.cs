using Literasi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Admin.TeachingAssignment;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<AssignmentDto> Assignments { get; set; } = new();

    public async Task OnGetAsync()
    {
        Assignments = await _context.TeachingAssignments
            .Include(t => t.Teacher).ThenInclude(t => t.User)
            .Include(t => t.Subject)
            .Include(t => t.Class).ThenInclude(c => c.AcademicYear)
            .OrderBy(t => t.Class.AcademicYear.YearName)
            .ThenBy(t => t.Class.ClassName)
            .ThenBy(t => t.Subject.SubjectName)
            .Select(t => new AssignmentDto
            {
                TeachingAssignmentId = t.TeachingAssignmentId,
                TeacherName = t.Teacher.User.FullName,
                SubjectName = t.Subject.SubjectName,
                ClassName = t.Class.ClassName,
                AcademicYear = t.Class.AcademicYear.YearName
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var assignment = await _context.TeachingAssignments.FindAsync(id);

        if (assignment == null)
        {
            TempData["Error"] = "Penugasan tidak ditemukan.";
            return RedirectToPage();
        }

        _context.TeachingAssignments.Remove(assignment);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Penugasan berhasil dihapus.";
        return RedirectToPage();
    }

    public class AssignmentDto
    {
        public int TeachingAssignmentId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
    }
}
