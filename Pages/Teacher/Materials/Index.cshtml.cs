using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Teacher.LearningMaterials;

[Authorize(Roles = "Guru")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<LearningMaterial> LearningMaterials
    { get; set; }
        = new List<LearningMaterial>();

    public async Task OnGetAsync()
    {
        LearningMaterials =
            await _context.LearningMaterials
                .Include(m => m.TeachingAssignment)
                    .ThenInclude(t => t.Subject)
                .Include(m => m.TeachingAssignment)
                    .ThenInclude(t => t.Class)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();
    }
}