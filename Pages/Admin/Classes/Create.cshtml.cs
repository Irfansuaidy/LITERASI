using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Admin.Classes;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Class Class { get; set; } = new();

    public SelectList GradeLevelOptions { get; set; } = null!;
    public SelectList AcademicYearOptions { get; set; } = null!;
    public SelectList TeacherOptions { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        await PopulateSelectListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync();
            return Page();
        }

        _context.Classes.Add(Class);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Kelas berhasil ditambahkan.";
        return RedirectToPage("/Admin/Classes/Index");
    }

    private async Task PopulateSelectListsAsync()
    {
        var gradeLevels = await _context.GradeLevels
            .OrderBy(g => g.LevelName)
            .ToListAsync();

        var academicYears = await _context.AcademicYears
            .OrderByDescending(a => a.IsActive)
            .ThenByDescending(a => a.StartDate)
            .ToListAsync();

        var teachers = await _context.Teachers
            .Include(t => t.User)
            .OrderBy(t => t.User.FullName)
            .ToListAsync();

        GradeLevelOptions = new SelectList(gradeLevels, "GradeLevelId", "LevelName");
        AcademicYearOptions = new SelectList(academicYears, "AcademicYearId", "YearName");
        TeacherOptions = new SelectList(teachers, "TeacherId", "User.FullName");
    }
}
