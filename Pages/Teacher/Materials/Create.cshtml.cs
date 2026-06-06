using Literasi.Data;
using Literasi.Models;
using Literasi.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Literasi.Pages.Teacher.LearningMaterials;

[Authorize(Roles = "Guru")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public CreateModel(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [BindProperty]
    public LearningMaterial LearningMaterial { get; set; } = new();

    [BindProperty]
    public IFormFile? UploadedFile { get; set; }

    public SelectList TeachingAssignments { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAssignments();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        RemoveNavigationValidation();
        ValidateRequiredSelections();

        // Remove file-related validations from ModelState if type is LINK
        if (LearningMaterial.MaterialType == MaterialType.LINK)
        {
            ModelState.Remove("UploadedFile");
        }
        else
        {
            ModelState.Remove("LearningMaterial.ExternalUrl");
        }

        if (!ModelState.IsValid)
        {
            await LoadAssignments();
            return Page();
        }

        // Verify ownership
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
        if (teacher == null) return Forbid();

        var ownsTA = await _context.TeachingAssignments
            .AnyAsync(ta => ta.TeachingAssignmentId == LearningMaterial.TeachingAssignmentId
                         && ta.TeacherId == teacher.TeacherId);
        if (!ownsTA)
        {
            ModelState.AddModelError(string.Empty, "Anda tidak memiliki hak atas Teaching Assignment ini.");
            await LoadAssignments();
            return Page();
        }

        // Handle file upload for PDF/PPT
        if (LearningMaterial.MaterialType != MaterialType.LINK)
        {
            if (UploadedFile == null || UploadedFile.Length == 0)
            {
                ModelState.AddModelError("UploadedFile", "File wajib diunggah untuk tipe PDF atau PPT.");
                await LoadAssignments();
                return Page();
            }

            var extension = Path.GetExtension(UploadedFile.FileName).ToLowerInvariant();
            var validExtension = LearningMaterial.MaterialType == MaterialType.PDF
                ? extension == ".pdf"
                : extension == ".ppt" || extension == ".pptx";

            if (!validExtension)
            {
                ModelState.AddModelError("UploadedFile", "Tipe file tidak sesuai dengan pilihan materi.");
                await LoadAssignments();
                return Page();
            }

            // Validate file size per type
            long maxSize = LearningMaterial.MaterialType == MaterialType.PDF
                ? 50L * 1024 * 1024   // 50 MB
                : 25L * 1024 * 1024;  // 25 MB

            if (UploadedFile.Length > maxSize)
            {
                string maxLabel = LearningMaterial.MaterialType == MaterialType.PDF ? "50MB" : "25MB";
                ModelState.AddModelError("UploadedFile", $"Ukuran file melebihi batas maksimal ({maxLabel}).");
                await LoadAssignments();
                return Page();
            }

            // Save file to disk
            string uploadDir = Path.Combine(_env.WebRootPath, "uploads", "materials");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            string safeFileName = Path.GetFileName(UploadedFile.FileName);
            string uniqueName = $"{Guid.NewGuid()}_{safeFileName}";
            string filePath = Path.Combine(uploadDir, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await UploadedFile.CopyToAsync(stream);
            }

            LearningMaterial.FilePath = uniqueName;
            LearningMaterial.ExternalUrl = null;
        }
        else
        {
            LearningMaterial.FilePath = null;
        }

        LearningMaterial.UploadedAt = DateTime.UtcNow;
        _context.LearningMaterials.Add(LearningMaterial);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Materi \"{LearningMaterial.Title}\" berhasil ditambahkan.";
        return RedirectToPage("./Index");
    }

    private async Task LoadAssignments()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);

        var assignments = await _context.TeachingAssignments
            .Include(t => t.Subject)
            .Include(t => t.Class)
            .Where(t => t.TeacherId == (teacher != null ? teacher.TeacherId : 0))
            .OrderBy(t => t.Subject.SubjectName)
            .ToListAsync();

        TeachingAssignments = new SelectList(
            assignments.Select(t => new
            {
                t.TeachingAssignmentId,
                Display = $"{t.Subject.SubjectName} - {t.Class.ClassName}"
            }),
            "TeachingAssignmentId",
            "Display");
    }

    private void RemoveNavigationValidation()
    {
        ModelState.Remove("LearningMaterial.TeachingAssignment");
    }

    private void ValidateRequiredSelections()
    {
        if (LearningMaterial.TeachingAssignmentId <= 0)
            ModelState.AddModelError("LearningMaterial.TeachingAssignmentId", "Teaching assignment wajib dipilih.");
    }
}
