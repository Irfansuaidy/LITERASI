using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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
        Assignment = await _context.Assignments
            .Include(a => a.TeachingAssignment)
                .ThenInclude(t => t.Subject)
            .Include(a => a.TeachingAssignment)
                .ThenInclude(t => t.Class)
            .FirstOrDefaultAsync(a =>
                a.AssignmentId == id);

        if (Assignment == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var assignment =
            await _context.Assignments
                .FindAsync(id);

        if (assignment != null)
        {
            _context.Assignments
                .Remove(assignment);

            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}