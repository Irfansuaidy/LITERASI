using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Literasi.Pages.Student.Class
{
    [Authorize(Roles = "Siswa")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string ClassName { get; set; } = string.Empty;
        public bool IsProfileComplete { get; set; } = true;
        public List<SubjectGroupDto> SubjectGroups { get; set; } = new();

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

            ClassName = studentData.Class?.ClassName ?? "Kelas";
            int currentClassId = studentData.ClassId;

            // Tarik data relasional multi-level Teaching Assignments sesuai ERD MVP
            SubjectGroups = await _context.Set<global::Literasi.Models.TeachingAssignment>()
                .Where(ta => ta.ClassId == currentClassId)
                .Select(ta => new SubjectGroupDto
                {
                    SubjectCode = ta.Subject.SubjectCode,
                    SubjectName = ta.Subject.SubjectName,
                    TeacherName = ta.Teacher.User.FullName,
                    Materials = ta.LearningMaterials.Select(m => new MaterialItemDto
                    {
                        MaterialId = m.MaterialId,
                        Title = m.Title,
                        Description = m.Description,
                        MaterialType = m.MaterialType.ToString(),
                        FilePath = m.FilePath,
                        ExternalUrl = m.ExternalUrl
                    }).ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            return Page();
        }
    }

    public class SubjectGroupDto
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public List<MaterialItemDto> Materials { get; set; } = new();
    }

    public class MaterialItemDto
    {
        public int MaterialId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string MaterialType { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public string? ExternalUrl { get; set; }
    }
}
