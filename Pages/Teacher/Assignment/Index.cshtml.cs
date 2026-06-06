using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Teacher.Assignments;

[Authorize(Roles = "Guru")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Assignment> Assignments { get; set; }
        = new List<Assignment>();

    public async Task OnGetAsync()
    {
        Assignments = await _context.Assignments
            .Include(a => a.TeachingAssignment)
                .ThenInclude(t => t.Subject)
            .Include(a => a.TeachingAssignment)
                .ThenInclude(t => t.Class)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}