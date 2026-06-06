using Literasi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Admin.Subjects;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<SubjectDto> Subjects { get; set; } = new();

    public async Task OnGetAsync()
    {
        Subjects = await _context.Subjects
            .Include(s => s.TeachingAssignments)
            .OrderBy(s => s.SubjectName)
            .Select(s => new SubjectDto
            {
                SubjectId       = s.SubjectId,
                SubjectName     = s.SubjectName,
                AssignmentCount = s.TeachingAssignments.Count
            })
            .ToListAsync();
    }

    public class SubjectDto
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int AssignmentCount { get; set; }
    }
}
