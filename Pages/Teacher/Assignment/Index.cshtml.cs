using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Literasi.Pages.Teacher.Assignments;

[Authorize(Roles = "Guru")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Assignment> Assignments { get; set; } = new List<Assignment>();

    public async Task OnGetAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);

        if (teacher == null) return;

        Assignments = await _context.Assignments
            .Include(a => a.TeachingAssignment)
                .ThenInclude(t => t.Subject)
            .Include(a => a.TeachingAssignment)
                .ThenInclude(t => t.Class)
            .Include(a => a.Submissions)
            .Where(a => a.TeachingAssignment.TeacherId == teacher.TeacherId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}
