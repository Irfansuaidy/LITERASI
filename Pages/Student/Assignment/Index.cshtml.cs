using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Literasi.Pages.Student.Assignments
{
    [Authorize(Roles = "Siswa")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool IsProfileComplete { get; set; } = true;
        public List<AssignmentListDto> ActiveAssignments { get; set; } = new();
        public List<AssignmentListDto> PastAssignments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var studentData = await _context.Set<global::Literasi.Models.Student>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (studentData == null)
            {
                IsProfileComplete = false;
                return Page();
            }

            int currentClassId = studentData.ClassId;
            int currentStudentId = studentData.StudentId;
            var currentTime = DateTime.Now;

            // Tarik tugas dan gabungkan status submission-nya jika ada
            var assignmentsRaw = await _context.Set<global::Literasi.Models.Assignment>()
                .Where(a => a.TeachingAssignment.ClassId == currentClassId)
                .Select(a => new AssignmentListDto
                {
                    AssignmentId = a.AssignmentId,
                    Title = a.Title,
                    SubjectName = a.TeachingAssignment.Subject.SubjectName,
                    Deadline = a.Deadline,
                    MaxScore = a.MaxScore,
                    Score = a.Submissions
                        .Where(s => s.StudentId == currentStudentId)
                        .Select(s => s.Grade != null ? s.Grade.Score : (decimal?)null)
                        .FirstOrDefault(),
                    // Cek status pengumpulan tugas oleh siswa ini untuk MVP Flow
                    SubmissionStatus = a.Submissions
                        .Where(s => s.StudentId == currentStudentId)
                        .Select(s => s.Status.ToString())
                        .FirstOrDefault() ?? "Belum Mengumpulkan"
                })
                .AsNoTracking()
                .ToListAsync();

            // Pemisahan Visual (Segregation) berdasarkan Tenggat Waktu (Deadline)
            ActiveAssignments = assignmentsRaw.Where(a => a.Deadline > currentTime).OrderBy(a => a.Deadline).ToList();
            PastAssignments = assignmentsRaw.Where(a => a.Deadline <= currentTime).OrderByDescending(a => a.Deadline).ToList();

            return Page();
        }
    }

    public class AssignmentListDto
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public decimal MaxScore { get; set; }
        public string SubmissionStatus { get; set; } = string.Empty;
        public decimal? Score { get; set; }
    }
}
