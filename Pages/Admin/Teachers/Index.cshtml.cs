using Literasi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Admin.Teachers;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<TeacherDto> Teachers { get; set; } = new();

    public async Task OnGetAsync()
    {
        Teachers = await _context.Teachers
            .Include(t => t.User)
            .OrderBy(t => t.User.FullName)
            .Select(t => new TeacherDto
            {
                TeacherId = t.TeacherId,
                FullName  = t.User != null ? (t.User.FullName ?? string.Empty) : string.Empty,
                Username  = t.User != null ? (t.User.Username ?? string.Empty) : string.Empty,
                Email     = t.User != null ? (t.User.Email ?? string.Empty) : string.Empty,
                Nip       = t.Nip ?? string.Empty,
                Gender    = t.User != null ? t.User.Gender.ToString() : string.Empty,
                IsActive  = t.User != null && t.User.IsActive
            })
            .ToListAsync();
    }

    public class TeacherDto
    {
        public int TeacherId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Nip { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
