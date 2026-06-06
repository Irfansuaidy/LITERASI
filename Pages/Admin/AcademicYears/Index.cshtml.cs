using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Admin.AcademicYears;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<AcademicYear> AcademicYears { get; set; } = new();

    public async Task OnGetAsync()
    {
        AcademicYears = await _context.AcademicYears
            .OrderByDescending(a => a.IsActive)
            .ThenByDescending(a => a.StartDate)
            .ToListAsync();
    }
}
