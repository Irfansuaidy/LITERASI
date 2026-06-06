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
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public EditTeacherInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var teacher = await _context.Teachers
            .Include(t => t.User)
            .FirstOrDefaultAsync(m => m.TeacherId == id);

        if (teacher == null)
        {
            return NotFound();
        }

        Input = new EditTeacherInput
        {
            TeacherId = teacher.TeacherId,
            UserId = teacher.UserId,
            FullName = teacher.User.FullName,
            Username = teacher.User.Username,
            Email = teacher.User.Email ?? string.Empty,
            Gender = teacher.User.Gender.ToString(),
            Nip = teacher.Nip,
            IsActive = teacher.User.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var teacher = await _context.Teachers
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TeacherId == Input.TeacherId);

        if (teacher == null)
        {
            return NotFound();
        }

        // Validate duplicates for username or email (excluding current user)
        var duplicateUser = await _context.Users
            .AnyAsync(u => u.UserId != teacher.UserId && (u.Username == Input.Username || u.Email == Input.Email));

        if (duplicateUser)
        {
            ModelState.AddModelError(string.Empty, "Username atau email sudah digunakan oleh pengguna lain.");
            return Page();
        }

        // Validate duplicate NIP (excluding current teacher)
        var duplicateNip = await _context.Teachers
            .AnyAsync(t => t.TeacherId != teacher.TeacherId && t.Nip == Input.Nip);

        if (duplicateNip)
        {
            ModelState.AddModelError(string.Empty, "NIP sudah terdaftar pada guru lain.");
            return Page();
        }

        // Update User info
        teacher.User.FullName = Input.FullName;
        teacher.User.Username = Input.Username;
        teacher.User.Email = Input.Email;
        teacher.User.Gender = Enum.Parse<Gender>(Input.Gender);
        teacher.User.IsActive = Input.IsActive;

        // Reset password if provided
        if (!string.IsNullOrEmpty(Input.NewPassword))
        {
            if (Input.NewPassword.Length < 6)
            {
                ModelState.AddModelError("Input.NewPassword", "Password minimal 6 karakter.");
                return Page();
            }
            teacher.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.NewPassword);
        }

        // Update Teacher info
        teacher.Nip = Input.Nip;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TeacherExists(teacher.TeacherId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        TempData["Success"] = $"Data guru {teacher.User.FullName} berhasil diperbarui.";
        return RedirectToPage("./Index");
    }

    private bool TeacherExists(int id)
    {
        return _context.Teachers.Any(e => e.TeacherId == id);
    }

    public class EditTeacherInput
    {
        public int TeacherId { get; set; }
        public int UserId { get; set; }

        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username wajib diisi.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Password Baru (Kosongkan jika tidak ingin diubah)")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Jenis kelamin wajib dipilih.")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "NIP wajib diisi.")]
        public string Nip { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
