using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Literasi.Pages.Admin.Classes;

[Authorize(Roles = "Admin")]
public class PromotionModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public PromotionModel(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [BindProperty(SupportsGet = true)]
    public int? SourceClassId { get; set; }

    [BindProperty]
    public int? TargetClassId { get; set; }

    [BindProperty]
    public List<int> SelectedStudentIds { get; set; } = new();

    [BindProperty]
    public bool IsGraduation { get; set; }

    public List<StudentDto> Students { get; set; } = new();
    public List<ArchiveFileDto> ArchivedFiles { get; set; } = new();

    public SelectList SourceClassOptions { get; set; } = null!;
    public SelectList TargetClassOptions { get; set; } = null!;

    public async Task OnGetAsync()
    {
        await PopulateSelectListsAsync();

        if (SourceClassId.HasValue)
        {
            Students = await _context.Students
                .Include(s => s.User)
                .Where(s => s.ClassId == SourceClassId.Value && s.Status == "Aktif")
                .OrderBy(s => s.User.FullName)
                .Select(s => new StudentDto
                {
                    StudentId = s.StudentId,
                    FullName = s.User.FullName,
                    Nisn = s.Nisn,
                    Gender = s.User.Gender.ToString()
                })
                .ToListAsync();
        }

        // Fetch Archived Graduation Files
        string archiveDir = Path.Combine(_env.WebRootPath, "archives");
        if (Directory.Exists(archiveDir))
        {
            var dirInfo = new DirectoryInfo(archiveDir);
            ArchivedFiles = dirInfo.GetFiles("*.json")
                .OrderByDescending(f => f.CreationTime)
                .Select(f => new ArchiveFileDto
                {
                    FileName = f.Name,
                    FileSize = f.Length,
                    CreatedAt = f.CreationTime
                })
                .ToList();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (SelectedStudentIds == null || !SelectedStudentIds.Any())
        {
            TempData["Error"] = "Tidak ada siswa yang dipilih.";
            return RedirectToPage(new { SourceClassId });
        }

        if (IsGraduation)
        {
            // Process Graduation (Archive to Disk and delete from DB)
            var sourceClass = await _context.Classes
                .Include(c => c.AcademicYear)
                .FirstOrDefaultAsync(c => c.ClassId == SourceClassId);

            string className = sourceClass?.ClassName ?? "UnknownClass";
            string academicYear = sourceClass?.AcademicYear.YearName.Replace("/", "-") ?? "UnknownYear";

            var studentsToGraduate = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Submissions)
                    .ThenInclude(sub => sub.Assignment)
                .Include(s => s.Submissions)
                    .ThenInclude(sub => sub.Grade)
                .Where(s => SelectedStudentIds.Contains(s.StudentId))
                .ToListAsync();

            var archiveList = studentsToGraduate.Select(s => new
            {
                s.StudentId,
                s.Nisn,
                FullName = s.User.FullName,
                Username = s.User.Username,
                Email = s.User.Email,
                Gender = s.User.Gender.ToString(),
                Class = className,
                AcademicYear = academicYear,
                GraduatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Submissions = s.Submissions.Select(sub => new
                {
                    AssignmentTitle = sub.Assignment.Title,
                    sub.SubmissionType,
                    sub.SubmittedAt,
                    sub.Status,
                    Grade = sub.Grade != null ? new { sub.Grade.Score, sub.Grade.Feedback } : null
                }).ToList()
            }).ToList();

            // Save JSON to Disk
            string archiveDir = Path.Combine(_env.WebRootPath, "archives");
            if (!Directory.Exists(archiveDir))
            {
                Directory.CreateDirectory(archiveDir);
            }

            string fileName = $"lulusan_{academicYear}_{className}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string filePath = Path.Combine(archiveDir, fileName);
            string jsonString = JsonSerializer.Serialize(archiveList, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(filePath, jsonString);

            // Delete from Database
            foreach (var student in studentsToGraduate)
            {
                // Delete related grades first
                var submissionIds = student.Submissions.Select(sub => sub.SubmissionId).ToList();
                var grades = await _context.Grades.Where(g => submissionIds.Contains(g.SubmissionId)).ToListAsync();
                _context.Grades.RemoveRange(grades);

                // Delete submissions
                _context.Submissions.RemoveRange(student.Submissions);

                // Delete Student and User
                _context.Students.Remove(student);
                _context.Users.Remove(student.User);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"{studentsToGraduate.Count} siswa berhasil diluluskan dan diarsipkan ke file {fileName}.";
        }
        else
        {
            // Process Class Promotion (Kenaikan Kelas)
            if (!TargetClassId.HasValue)
            {
                TempData["Error"] = "Kelas tujuan wajib dipilih untuk proses kenaikan kelas.";
                return RedirectToPage(new { SourceClassId });
            }

            var targetClass = await _context.Classes.FindAsync(TargetClassId.Value);
            if (targetClass == null)
            {
                TempData["Error"] = "Kelas tujuan tidak ditemukan.";
                return RedirectToPage(new { SourceClassId });
            }

            var studentsToPromote = await _context.Students
                .Include(s => s.User)
                .Where(s => SelectedStudentIds.Contains(s.StudentId))
                .ToListAsync();

            foreach (var student in studentsToPromote)
            {
                student.ClassId = targetClass.ClassId;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"{studentsToPromote.Count} siswa berhasil dinaikkan ke kelas {targetClass.ClassName}.";
        }

        return RedirectToPage(new { SourceClassId });
    }

    private async Task PopulateSelectListsAsync()
    {
        var classes = await _context.Classes
            .Include(c => c.GradeLevel)
            .Include(c => c.AcademicYear)
            .OrderByDescending(c => c.AcademicYear.IsActive)
            .ThenBy(c => c.AcademicYear.YearName)
            .ThenBy(c => c.GradeLevel.LevelName)
            .ThenBy(c => c.ClassName)
            .Select(c => new
            {
                c.ClassId,
                DisplayName = $"{c.ClassName} ({c.AcademicYear.YearName})" + (c.AcademicYear.IsActive ? " - Aktif" : "")
            })
            .ToListAsync();

        SourceClassOptions = new SelectList(classes, "ClassId", "DisplayName");
        TargetClassOptions = new SelectList(classes, "ClassId", "DisplayName");
    }

    public class StudentDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Nisn { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
    }

    public class ArchiveFileDto
    {
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
