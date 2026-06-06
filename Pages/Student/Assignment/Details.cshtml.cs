using Literasi.Data;
using Literasi.Models;
using Literasi.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Literasi.Pages.Student.Assignments;

[Authorize(Roles = "Siswa")]
public class DetailsModel : PageModel
{
    private const long MaxSubmissionFileSize = 10L * 1024 * 1024;

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public DetailsModel(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [BindProperty]
    public SubmissionType SubmissionType { get; set; } = SubmissionType.PDF;

    [BindProperty]
    public IFormFile? UploadedFile { get; set; }

    [BindProperty]
    public string? ExternalUrl { get; set; }

    public int AssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public decimal MaxScore { get; set; }
    public bool IsPastDeadline { get; set; }
    public bool HasSubmission { get; set; }
    public bool IsReviewed { get; set; }
    public string SubmissionStatus { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public string? SubmittedFileUrl { get; set; }
    public string? SubmittedExternalUrl { get; set; }
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var student = await GetCurrentStudentAsync();
        if (student == null) return RedirectToPage("/Auth/Login");

        var assignment = await LoadAssignmentForStudentAsync(id, student.StudentId, student.ClassId);
        if (assignment == null) return NotFound();

        LoadPageData(assignment, student.StudentId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var student = await GetCurrentStudentAsync();
        if (student == null) return RedirectToPage("/Auth/Login");

        var assignment = await LoadAssignmentForStudentAsync(id, student.StudentId, student.ClassId);
        if (assignment == null) return NotFound();

        var existingSubmission = assignment.Submissions.FirstOrDefault(s => s.StudentId == student.StudentId);
        if (existingSubmission?.Status == Literasi.Models.Enums.SubmissionStatus.Reviewed)
        {
            ModelState.AddModelError(string.Empty, "Tugas sudah dinilai dan tidak dapat diubah.");
            LoadPageData(assignment, student.StudentId);
            return Page();
        }

        ValidateSubmissionInput();

        if (!ModelState.IsValid)
        {
            LoadPageData(assignment, student.StudentId);
            return Page();
        }

        var submission = existingSubmission ?? new Submission
        {
            AssignmentId = assignment.AssignmentId,
            StudentId = student.StudentId
        };

        submission.SubmissionType = SubmissionType;
        submission.SubmittedAt = DateTime.UtcNow;
        submission.Status = DateTime.Now > assignment.Deadline
            ? Literasi.Models.Enums.SubmissionStatus.Late
            : Literasi.Models.Enums.SubmissionStatus.Submitted;

        if (SubmissionType == SubmissionType.PDF)
        {
            submission.FilePath = await SaveUploadedFileAsync();
            submission.ExternalUrl = null;
        }
        else
        {
            submission.FilePath = null;
            submission.ExternalUrl = ExternalUrl?.Trim();
        }

        if (existingSubmission == null)
        {
            _context.Submissions.Add(submission);
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Pengumpulan tugas berhasil disimpan.";
        return RedirectToPage("./Details", new { id = assignment.AssignmentId });
    }

    private async Task<global::Literasi.Models.Student?> GetCurrentStudentAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return null;

        return await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
    }

    private async Task<Assignment?> LoadAssignmentForStudentAsync(int assignmentId, int studentId, int classId)
    {
        return await _context.Assignments
            .Include(a => a.TeachingAssignment)
                .ThenInclude(ta => ta.Subject)
            .Include(a => a.TeachingAssignment)
                .ThenInclude(ta => ta.Class)
            .Include(a => a.Submissions.Where(s => s.StudentId == studentId))
                .ThenInclude(s => s.Grade)
            .FirstOrDefaultAsync(a =>
                a.AssignmentId == assignmentId &&
                a.TeachingAssignment.ClassId == classId);
    }

    private void LoadPageData(Assignment assignment, int studentId)
    {
        var submission = assignment.Submissions.FirstOrDefault(s => s.StudentId == studentId);

        AssignmentId = assignment.AssignmentId;
        Title = assignment.Title;
        Description = assignment.Description;
        SubjectName = assignment.TeachingAssignment.Subject.SubjectName;
        ClassName = assignment.TeachingAssignment.Class.ClassName;
        Deadline = assignment.Deadline;
        MaxScore = assignment.MaxScore;
        IsPastDeadline = DateTime.Now > assignment.Deadline;

        HasSubmission = submission != null;
        if (submission == null) return;

        SubmissionType = submission.SubmissionType;
        ExternalUrl = submission.ExternalUrl;
        SubmissionStatus = submission.Status.ToString();
        SubmittedAt = submission.SubmittedAt;
        SubmittedFileUrl = string.IsNullOrWhiteSpace(submission.FilePath)
            ? null
            : $"/uploads/submissions/{submission.FilePath}";
        SubmittedExternalUrl = submission.ExternalUrl;
        IsReviewed = submission.Status == Literasi.Models.Enums.SubmissionStatus.Reviewed;
        Score = submission.Grade?.Score;
        Feedback = submission.Grade?.Feedback;
    }

    private void ValidateSubmissionInput()
    {
        if (SubmissionType == SubmissionType.PDF)
        {
            if (UploadedFile == null || UploadedFile.Length == 0)
            {
                ModelState.AddModelError("UploadedFile", "File jawaban wajib diunggah.");
                return;
            }

            if (UploadedFile.Length > MaxSubmissionFileSize)
            {
                ModelState.AddModelError("UploadedFile", "Ukuran file maksimal 10MB.");
            }

            var extension = Path.GetExtension(UploadedFile.FileName).ToLowerInvariant();
            if (extension != ".pdf")
            {
                ModelState.AddModelError("UploadedFile", "File jawaban harus berformat PDF.");
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(ExternalUrl))
            {
                ModelState.AddModelError("ExternalUrl", "Link jawaban wajib diisi.");
                return;
            }

            if (!Uri.TryCreate(ExternalUrl.Trim(), UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                ModelState.AddModelError("ExternalUrl", "Link harus berupa URL http/https yang valid.");
            }
        }
    }

    private async Task<string> SaveUploadedFileAsync()
    {
        string uploadDir = Path.Combine(_env.WebRootPath, "uploads", "submissions");
        if (!Directory.Exists(uploadDir))
        {
            Directory.CreateDirectory(uploadDir);
        }

        string safeFileName = Path.GetFileName(UploadedFile!.FileName);
        string uniqueName = $"{Guid.NewGuid()}_{safeFileName}";
        string filePath = Path.Combine(uploadDir, uniqueName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await UploadedFile.CopyToAsync(stream);

        return uniqueName;
    }
}
