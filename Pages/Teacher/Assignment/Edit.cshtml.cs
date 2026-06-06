using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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
        Assignment = await _context.Assignments
            .FirstOrDefaultAsync(a =>
                a.AssignmentId == id);

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

        if (Assignment == null)
        {
            return BadRequest();
        }

        _context.Attach(Assignment)
            .State = EntityState.Modified;

        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    private async Task LoadAssignments()
    {
        var assignments =
            await _context.TeachingAssignments
                .Include(t => t.Subject)
                .Include(t => t.Class)
                .ToListAsync();

        TeachingAssignments =
            new SelectList(
                assignments.Select(t => new
                {
                    t.TeachingAssignmentId,
                    Display =
                        $"{t.Subject.SubjectName} - {t.Class.ClassName}"
                }),
                "TeachingAssignmentId",
                "Display");
    }
}