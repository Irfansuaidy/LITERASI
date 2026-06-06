using Literasi.Data;
using Literasi.Models;
using Literasi.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Literasi.Pages.Teacher.Assignments;

[Authorize(Roles = "Guru")]
public class SubmissionsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public SubmissionsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public List<SubmissionRow> Submissions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var teacher = await GetCurrentTeacherAsync();
        if (teacher == null) return Forbid();

        var assignment = await LoadAssignmentAsync(id, teacher.TeacherId);
        if (assignment == null) return NotFound();

        LoadPageHeader(assignment);
        Submissions = assignment.Submissions
            .OrderByDescending(s => s.SubmittedAt)
            .Select(MapSubmission)
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int assignmentId, int submissionId, decimal score, string? feedback)
    {
        var teacher = await GetCurrentTeacherAsync();
        if (teacher == null) return Forbid();

        var submission = await _context.Submissions
            .Include(s => s.Assignment)
                .ThenInclude(a => a.TeachingAssignment)
            .Include(s => s.Grade)
            .FirstOrDefaultAsync(s =>
                s.SubmissionId == submissionId &&
                s.AssignmentId == assignmentId &&
                s.Assignment.TeachingAssignment.TeacherId == teacher.TeacherId);

        if (submission == null) return NotFound();

        if (score < 0 || score > submission.Assignment.MaxScore)
        {
            ModelState.AddModelError(string.Empty, $"Nilai harus berada di antara 0 dan {submission.Assignment.MaxScore}.");
            return await OnGetAsync(assignmentId);
        }

        if (submission.Grade == null)
        {
            submission.Grade = new Grade
            {
                SubmissionId = submission.SubmissionId,
                GradedByTeacherId = teacher.TeacherId
            };
            _context.Grades.Add(submission.Grade);
        }

        submission.Grade.Score = score;
        submission.Grade.Feedback = feedback;
        submission.Grade.GradedAt = DateTime.UtcNow;
        submission.Grade.GradedByTeacherId = teacher.TeacherId;
        submission.Status = SubmissionStatus.Reviewed;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Nilai dan feedback berhasil disimpan.";
        return RedirectToPage("./Submissions", new { id = assignmentId });
    }

    private async Task<global::Literasi.Models.Teacher?> GetCurrentTeacherAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
    }

    private async Task<Assignment?> LoadAssignmentAsync(int assignmentId, int teacherId)
    {
        return await _context.Assignments
            .Include(a => a.TeachingAssignment)
                .ThenInclude(ta => ta.Subject)
            .Include(a => a.TeachingAssignment)
                .ThenInclude(ta => ta.Class)
            .Include(a => a.Submissions)
                .ThenInclude(s => s.Student)
                    .ThenInclude(st => st.User)
            .Include(a => a.Submissions)
                .ThenInclude(s => s.Grade)
            .FirstOrDefaultAsync(a =>
                a.AssignmentId == assignmentId &&
                a.TeachingAssignment.TeacherId == teacherId);
    }

    private void LoadPageHeader(Assignment assignment)
    {
        AssignmentId = assignment.AssignmentId;
        AssignmentTitle = assignment.Title;
        SubjectName = assignment.TeachingAssignment.Subject.SubjectName;
        ClassName = assignment.TeachingAssignment.Class.ClassName;
        MaxScore = assignment.MaxScore;
    }

    private static SubmissionRow MapSubmission(Submission submission)
    {
        return new SubmissionRow
        {
            SubmissionId = submission.SubmissionId,
            StudentName = submission.Student.User.FullName,
            Nisn = submission.Student.Nisn,
            SubmittedAt = submission.SubmittedAt,
            Status = submission.Status.ToString(),
            FilePath = submission.FilePath,
            FileUrl = string.IsNullOrWhiteSpace(submission.FilePath)
                ? null
                : $"/uploads/submissions/{submission.FilePath}",
            ExternalUrl = submission.ExternalUrl,
            Score = submission.Grade?.Score,
            Feedback = submission.Grade?.Feedback
        };
    }

    public class SubmissionRow
    {
        public int SubmissionId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Nisn { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public string? FileUrl { get; set; }
        public string? ExternalUrl { get; set; }
        public decimal? Score { get; set; }
        public string? Feedback { get; set; }
    }
}
