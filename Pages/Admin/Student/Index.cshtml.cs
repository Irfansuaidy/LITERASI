using Literasi.Data;
using Literasi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Pages.Admin.Student;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<StudentDto> Students { get; set; } = new();

    public async Task OnGetAsync()
    {
        Students = await _context.Students
            .Include(s => s.User)
            .Include(s => s.Class)
            .OrderBy(s => s.Class.ClassName)
            .ThenBy(s => s.User.FullName)
            .Select(s => new StudentDto
            {
                StudentId  = s.StudentId,
                FullName   = s.User.FullName,
                Username   = s.User.Username,
                Nisn       = s.Nisn,
                ClassName  = s.Class.ClassName,
                Gender     = s.User.Gender.ToString(),
                IsActive   = s.User.IsActive
            })
            .ToListAsync();
    }

    public class StudentDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Nisn { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
