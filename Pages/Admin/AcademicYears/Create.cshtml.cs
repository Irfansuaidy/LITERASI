using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Admin.AcademicYears;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public AcademicYear AcademicYear { get; set; } = new();

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // If this year is set as active, deactivate all others
        if (AcademicYear.IsActive)
        {
            var activeYears = await _context.AcademicYears
                .Where(a => a.IsActive)
                .ToListAsync();

            foreach (var year in activeYears)
                year.IsActive = false;
        }

        _context.AcademicYears.Add(AcademicYear);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Tahun ajaran berhasil ditambahkan.";
        return RedirectToPage("/Admin/AcademicYears/Index");
    }
}
