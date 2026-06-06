using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Admin.AcademicYears;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public AcademicYear AcademicYear { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var year = await _context.AcademicYears.FindAsync(id);

        if (year == null)
            return NotFound();

        AcademicYear = year;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("AcademicYear.Classes");

        if (string.IsNullOrWhiteSpace(AcademicYear.YearName))
        {
            ModelState.AddModelError("AcademicYear.YearName", "Nama tahun ajaran wajib diisi.");
        }

        if (!AcademicYear.IsValidPeriod())
        {
            ModelState.AddModelError(string.Empty, "Tanggal selesai harus lebih besar dari tanggal mulai.");
        }

        if (!ModelState.IsValid)
            return Page();

        // If this year is being set as active, deactivate all others
        if (AcademicYear.IsActive)
        {
            var others = await _context.AcademicYears
                .Where(a => a.IsActive && a.AcademicYearId != AcademicYear.AcademicYearId)
                .ToListAsync();

            foreach (var year in others)
                year.IsActive = false;
        }

        _context.AcademicYears.Update(AcademicYear);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.AcademicYears.AnyAsync(a => a.AcademicYearId == AcademicYear.AcademicYearId))
                return NotFound();

            throw;
        }

        TempData["Success"] = "Tahun ajaran berhasil diperbarui.";
        return RedirectToPage("/Admin/AcademicYears/Index");
    }
}
