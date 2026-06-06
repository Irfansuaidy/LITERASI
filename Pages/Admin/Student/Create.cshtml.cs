using System.ComponentModel.DataAnnotations;
using Literasi.Data;
using Literasi.Models;
using StudentModels = Literasi.Models.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Literasi.Models.Enums;

namespace Literasi.Pages.Admin.Student;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public CreateStudentInput Input { get; set; } = new();

    public SelectList ClassOptions { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        await PopulateSelectListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync();
            return Page();
        }

        // Get the Student role
        var studentRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == "Siswa");

        if (studentRole == null)
        {
            ModelState.AddModelError(string.Empty, "Role 'Siswa' tidak ditemukan.");
            await PopulateSelectListsAsync();
            return Page();
        }

        // Check for duplicate username or email
        var exists = await _context.Users
            .AnyAsync(u => u.Username == Input.Username || u.Email == Input.Email);

        if (exists)
        {
            ModelState.AddModelError(string.Empty, "Username atau email sudah digunakan.");
            await PopulateSelectListsAsync();
            return Page();
        }

        // Check for duplicate NISN
        var nisnExists = await _context.Students
            .AnyAsync(s => s.Nisn == Input.Nisn);

        if (nisnExists)
        {
            ModelState.AddModelError(string.Empty, "NISN sudah terdaftar.");
            await PopulateSelectListsAsync();
            return Page();
        }

        var user = new User
        {
            FullName = Input.FullName,
            Username = Input.Username,
            Email = Input.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password),
            Gender = Enum.Parse<Gender>(Input.Gender),
            IsActive = true,
            RoleId = studentRole.RoleId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var studentModels = new StudentModels
        {
            UserId = user.UserId,
            Nisn = Input.Nisn,
            ClassId = Input.ClassId
        };

        _context.Students.Add(studentModels);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Siswa {user.FullName} berhasil ditambahkan.";
        return RedirectToPage("/Admin/Student/Index");
    }

    private async Task PopulateSelectListsAsync()
    {
        var classes = await _context.Classes
            .Include(c => c.GradeLevel)
            .Include(c => c.AcademicYear)
            .OrderBy(c => c.GradeLevel.LevelName)
            .ThenBy(c => c.ClassName)
            .Select(c => new
            {
                c.ClassId,
                DisplayName = $"{c.ClassName} ({c.AcademicYear.YearName})"
            })
            .ToListAsync();

        ClassOptions = new SelectList(classes, "ClassId", "DisplayName");
    }

    public class CreateStudentInput
    {
        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username wajib diisi.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi.")]
        [MinLength(6, ErrorMessage = "Password minimal 6 karakter.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jenis kelamin wajib dipilih.")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "NISN wajib diisi.")]
        public string Nisn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kelas wajib dipilih.")]
        public int ClassId { get; set; }
    }
}
