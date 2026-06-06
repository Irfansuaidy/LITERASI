using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Literasi.Pages.Teacher.LearningMaterials;

[Authorize(Roles = "Guru")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<LearningMaterial> LearningMaterials { get; set; } = new List<LearningMaterial>();

    public async Task OnGetAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);

        if (teacher == null) return;

        LearningMaterials = await _context.LearningMaterials
            .Include(m => m.TeachingAssignment)
                .ThenInclude(t => t.Subject)
            .Include(m => m.TeachingAssignment)
                .ThenInclude(t => t.Class)
            .Where(m => m.TeachingAssignment.TeacherId == teacher.TeacherId)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();
    }
}