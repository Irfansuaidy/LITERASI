using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Admin.TeachingAssignment;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.TeachingAssignment Assignment { get; set; } = new();

    public SelectList TeacherOptions { get; set; } = null!;
    public SelectList SubjectOptions { get; set; } = null!;
    public SelectList ClassOptions { get; set; } = null!;

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

        // Enforce unique combination: Teacher + Subject + Class
        var duplicate = await _context.TeachingAssignments
            .AnyAsync(t =>
                t.TeacherId  == Assignment.TeacherId  &&
                t.SubjectId  == Assignment.SubjectId  &&
                t.ClassId    == Assignment.ClassId);

        if (duplicate)
        {
            ModelState.AddModelError(string.Empty,
                "Penugasan dengan kombinasi Guru, Mata Pelajaran, dan Kelas ini sudah ada.");
            await PopulateSelectListsAsync();
            return Page();
        }

        _context.TeachingAssignments.Add(Assignment);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Penugasan mengajar berhasil ditambahkan.";
        return RedirectToPage("/Admin/TeachingAssignment/Index");
    }

    private async Task PopulateSelectListsAsync()
    {
        var teachers = await _context.Teachers
            .Include(t => t.User)
            .OrderBy(t => t.User.FullName)
            .Select(t => new { t.TeacherId, t.User.FullName })
            .ToListAsync();

        var subjects = await _context.Subjects
            .OrderBy(s => s.SubjectName)
            .ToListAsync();

        var classes = await _context.Classes
            .Include(c => c.GradeLevel)
            .Include(c => c.AcademicYear)
            .OrderBy(c => c.AcademicYear.YearName)
            .ThenBy(c => c.GradeLevel.LevelName)
            .ThenBy(c => c.ClassName)
            .Select(c => new
            {
                c.ClassId,
                DisplayName = $"{c.ClassName} ({c.AcademicYear.YearName})"
            })
            .ToListAsync();

        TeacherOptions = new SelectList(teachers, "TeacherId", "FullName");
        SubjectOptions = new SelectList(subjects, "SubjectId", "SubjectName");
        ClassOptions   = new SelectList(classes, "ClassId", "DisplayName");
    }
}
