using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Teacher.LearningMaterials;

[Authorize(Roles = "Guru")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public LearningMaterial LearningMaterial { get; set; }
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

        LearningMaterial.UploadedAt =
            DateTime.UtcNow;

        _context.LearningMaterials.Add(
            LearningMaterial);

        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    private async Task LoadAssignments()
    {
        var assignments =
            await _context.TeachingAssignments
                .Include(t => t.Subject)
                .Include(t => t.Class)
                .OrderBy(t => t.Subject.SubjectName)
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