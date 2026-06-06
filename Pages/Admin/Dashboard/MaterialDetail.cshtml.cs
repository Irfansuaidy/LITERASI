using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Admin.Dashboard;

[Authorize(Roles = "Admin")]
public class MaterialDetailModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public MaterialDetailModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public LearningMaterial Material { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var material = await _context.LearningMaterials
            .Include(m => m.TeachingAssignment.Teacher.User)
            .Include(m => m.TeachingAssignment.Subject)
            .Include(m => m.TeachingAssignment.Class)
            .FirstOrDefaultAsync(m => m.MaterialId == id);

        if (material == null)
        {
            return NotFound();
        }

        Material = material;
        return Page();
    }
}
