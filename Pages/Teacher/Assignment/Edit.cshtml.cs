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
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Assignment? Assignment { get; set; } = null;

    public SelectList TeachingAssignments { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
        if (teacher == null) return Forbid();

        Assignment = await _context.Assignments
            .Include(a => a.TeachingAssignment)
            .FirstOrDefaultAsync(a =>
                a.AssignmentId == id && a.TeachingAssignment.TeacherId == teacher.TeacherId);

        if (Assignment == null)
            return NotFound();

        await LoadAssignments();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAssignments();
            return Page();
        }

        if (Assignment == null) return BadRequest();

        // Verify ownership
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
        if (teacher == null) return Forbid();

        var ownsTA = await _context.TeachingAssignments
            .AnyAsync(ta => ta.TeachingAssignmentId == Assignment.TeachingAssignmentId
                         && ta.TeacherId == teacher.TeacherId);
        if (!ownsTA) return Forbid();

        _context.Attach(Assignment).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Tugas \"{Assignment.Title}\" berhasil diperbarui.";
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
}