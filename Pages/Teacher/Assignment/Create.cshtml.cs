using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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
    public Assignment Assignment { get; set; }
        = new();

    public SelectList TeachingAssignments { get; set; }
        = null!;

    public async Task OnGetAsync()
    {
        await LoadAssignments();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAssignments();
            return Page();
        }

        Assignment.CreatedAt =
            DateTime.UtcNow;

        _context.Assignments.Add(
            Assignment);

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