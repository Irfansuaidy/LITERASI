using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Literasi.Pages.Student
{
    [Authorize(Roles = "Siswa")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public bool IsProfileComplete { get; set; } = true;

        public List<MaterialDashboardDto> RecentMaterials { get; set; } = new();
        public List<AssignmentDashboardDto> UpcomingAssignments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var studentData = await _context.Set<global::Literasi.Models.Student>()
                .Include(s => s.Class)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (studentData == null)
            {
                IsProfileComplete = false;
                return Page();
            }

            StudentName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Siswa";
            ClassName = studentData.Class?.ClassName ?? "Tidak Terikat Kelas";
            int currentClassId = studentData.ClassId;

            RecentMaterials = await _context.Set<global::Literasi.Models.LearningMaterial>()
                .Where(m => m.TeachingAssignment.ClassId == currentClassId)
                .OrderByDescending(m => m.MaterialId)
                .Take(5)
                .Select(m => new MaterialDashboardDto
                {
                    MaterialId = m.MaterialId,
                    Title = m.Title,
                    SubjectName = m.TeachingAssignment.Subject.SubjectName,
                    TeacherName = m.TeachingAssignment.Teacher.User.FullName,
                    FilePath = m.FilePath,
                    ExternalUrl = m.ExternalUrl,
                    MaterialType = m.MaterialType.ToString()
                })
                .AsNoTracking()
                .ToListAsync();
            UpcomingAssignments = await _context.Set<global::Literasi.Models.Assignment>()
                .Where(a => a.TeachingAssignment.ClassId == currentClassId && a.Deadline > DateTime.Now)
                .OrderBy(a => a.Deadline)
                .Take(5)
                .Select(a => new AssignmentDashboardDto
                {
                    AssignmentId = a.AssignmentId,
                    Title = a.Title,
                    SubjectName = a.TeachingAssignment.Subject.SubjectName,
                    Deadline = a.Deadline
                })
                .AsNoTracking()
                .ToListAsync();

            return Page();
        }
    }

    // DTO Efisiensi Proyeksi LINQ
    public class MaterialDashboardDto
    {
        public int MaterialId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public string? ExternalUrl { get; set; }
        public string MaterialType { get; set; } = string.Empty;
    }

    public class AssignmentDashboardDto
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
    }
}
