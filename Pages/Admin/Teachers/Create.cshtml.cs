using System.ComponentModel.DataAnnotations;
using Literasi.Data;
using Literasi.Models;
using TeacherModel = Literasi.Models.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Literasi.Models.Enums;

namespace Literasi.Pages.Admin.Teachers;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public CreateTeacherInput Input { get; set; } = new();

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // Get the Guru role
        var teacherRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == "Guru");

        if (teacherRole == null)
        {
            ModelState.AddModelError(string.Empty, "Role 'Guru' tidak ditemukan.");
            return Page();
        }

        // Check for duplicate username or email
        var exists = await _context.Users
            .AnyAsync(u => u.Username == Input.Username || u.Email == Input.Email);

        if (exists)
        {
            ModelState.AddModelError(string.Empty, "Username atau email sudah digunakan.");
            return Page();
        }

        // Check for duplicate NIP
        var nipExists = await _context.Teachers
            .AnyAsync(t => t.Nip == Input.Nip);

        if (nipExists)
        {
            ModelState.AddModelError(string.Empty, "NIP sudah terdaftar.");
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
            RoleId = teacherRole.RoleId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var teacherModels = new TeacherModel
        {
            UserId = user.UserId,
            Nip = Input.Nip
        };

        _context.Teachers.Add(teacherModels);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Guru {user.FullName} berhasil ditambahkan.";
        return RedirectToPage("/Admin/Teachers/Index");
    }

    public class CreateTeacherInput
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

        [Required(ErrorMessage = "NIP wajib diisi.")]
        public string Nip { get; set; } = string.Empty;
    }
}
