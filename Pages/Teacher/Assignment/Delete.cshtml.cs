using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Literasi.Pages.Teacher.Assignments;

[Authorize(Roles = "Guru")]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Assignment? Assignment { get; set; } = null;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
        if (teacher == null) return Forbid();

        Assignment = await _context.Assignments
            .Include(a => a.TeachingAssignment)
                .ThenInclude(t => t.Subject)
            .Include(a => a.TeachingAssignment)
                .ThenInclude(t => t.Class)
            .FirstOrDefaultAsync(a =>
                a.AssignmentId == id && a.TeachingAssignment.TeacherId == teacher.TeacherId);

        if (Assignment == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
        if (teacher == null) return Forbid();

        var assignment = await _context.Assignments
            .Include(a => a.TeachingAssignment)
            .FirstOrDefaultAsync(a =>
                a.AssignmentId == id && a.TeachingAssignment.TeacherId == teacher.TeacherId);

        if (assignment != null)
        {
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Tugas berhasil dihapus.";
        }

        return RedirectToPage("./Index");
    }
}