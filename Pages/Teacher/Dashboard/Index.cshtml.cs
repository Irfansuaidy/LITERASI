using Literasi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Literasi.Pages.Teacher.Dashboard;

[Authorize(Roles = "Guru")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalTeachingAssignments { get; set; }
    public int TotalLearningMaterials { get; set; }
    public int TotalAssignments { get; set; }

    public List<RecentMaterialDto> RecentMaterials { get; set; }
        = new();

    public async Task OnGetAsync()
    {
        var userId =
            int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);

        var teacher =
            await _context.Teachers
                .FirstOrDefaultAsync(t =>
                    t.UserId == userId);

        if (teacher == null)
        {
            return;
        }

        TotalTeachingAssignments =
            await _context.TeachingAssignments
                .CountAsync(t =>
                    t.TeacherId == teacher.TeacherId);

        TotalLearningMaterials =
            await _context.LearningMaterials
                .CountAsync(m =>
                    m.TeachingAssignment.TeacherId
                        == teacher.TeacherId);

        TotalAssignments =
            await _context.Assignments
                .CountAsync(a =>
                    a.TeachingAssignment.TeacherId
                        == teacher.TeacherId);

        RecentMaterials =
            await _context.LearningMaterials
                .Include(m => m.TeachingAssignment)
                    .ThenInclude(t => t.Subject)
                .Include(m => m.TeachingAssignment)
                    .ThenInclude(t => t.Class)
                .Where(m =>
                    m.TeachingAssignment.TeacherId
                        == teacher.TeacherId)
                .OrderByDescending(m =>
                    m.UploadedAt)
                .Take(5)
                .Select(m =>
                    new RecentMaterialDto
                    {
                        Title = m.Title,
                        SubjectName =
                            m.TeachingAssignment
                                .Subject.SubjectName,
                        ClassName =
                            m.TeachingAssignment
                                .Class.ClassName
                    })
                .ToListAsync();
    }

    public class RecentMaterialDto
    {
        public string Title { get; set; }
            = string.Empty;

        public string SubjectName { get; set; }
            = string.Empty;

        public string ClassName { get; set; }
            = string.Empty;
    }
}