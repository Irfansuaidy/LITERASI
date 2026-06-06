using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Literasi.Pages.Teacher.Assignments;

[Authorize(Roles = "Guru")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Assignment Assignment { get; set; } = new();

    public SelectList TeachingAssignments { get; set; } = null!;

    public async Task OnGetAsync()
    {
        await LoadAssignments();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        RemoveNavigationValidation();
        ValidateRequiredSelections();

        if (!ModelState.IsValid)
        {
            await LoadAssignments();
            return Page();
        }

        // Verify ownership
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
        if (teacher == null) return Forbid();

        var ownsTA = await _context.TeachingAssignments
            .AnyAsync(ta => ta.TeachingAssignmentId == Assignment.TeachingAssignmentId
                         && ta.TeacherId == teacher.TeacherId);
        if (!ownsTA)
        {
            ModelState.AddModelError(string.Empty, "Anda tidak memiliki hak atas Teaching Assignment ini.");
            await LoadAssignments();
            return Page();
        }

        Assignment.CreatedAt = DateTime.UtcNow;
        _context.Assignments.Add(Assignment);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Tugas \"{Assignment.Title}\" berhasil dibuat.";
        return RedirectToPage("./Index");
    }

    private async Task LoadAssignments()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);

        var assignments = await _context.TeachingAssignments
            .Include(t => t.Subject)
            .Include(t => t.Class)
            .Where(t => t.TeacherId == (teacher != null ? teacher.TeacherId : 0))
            .OrderBy(t => t.Subject.SubjectName)
            .ToListAsync();

        TeachingAssignments = new SelectList(
            assignments.Select(t => new
            {
                t.TeachingAssignmentId,
                Display = $"{t.Subject.SubjectName} - {t.Class.ClassName}"
            }),
            "TeachingAssignmentId",
            "Display");
    }

    private void RemoveNavigationValidation()
    {
        ModelState.Remove("Assignment.TeachingAssignment");
        ModelState.Remove("Assignment.Submissions");
    }

    private void ValidateRequiredSelections()
    {
        if (Assignment.TeachingAssignmentId <= 0)
            ModelState.AddModelError("Assignment.TeachingAssignmentId", "Teaching assignment wajib dipilih.");

        if (Assignment.Deadline == default)
            ModelState.AddModelError("Assignment.Deadline", "Deadline wajib diisi.");
    }
}
