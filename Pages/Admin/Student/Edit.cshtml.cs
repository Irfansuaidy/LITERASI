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
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public EditStudentInput Input { get; set; } = new();

    public SelectList ClassOptions { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(m => m.StudentId == id);

        if (student == null)
        {
            return NotFound();
        }

        Input = new EditStudentInput
        {
            StudentId = student.StudentId,
            UserId = student.UserId,
            FullName = student.User.FullName,
            Username = student.User.Username,
            Email = student.User.Email ?? string.Empty,
            Gender = student.User.Gender.ToString(),
            Nisn = student.Nisn,
            ClassId = student.ClassId,
            Status = student.Status,
            IsActive = student.User.IsActive
        };

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

        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentId == Input.StudentId);

        if (student == null)
        {
            return NotFound();
        }

        // Validate duplicates for username or email (excluding current user)
        var duplicateUser = await _context.Users
            .AnyAsync(u => u.UserId != student.UserId && (u.Username == Input.Username || u.Email == Input.Email));

        if (duplicateUser)
        {
            ModelState.AddModelError(string.Empty, "Username atau email sudah digunakan oleh pengguna lain.");
            await PopulateSelectListsAsync();
            return Page();
        }

        // Validate duplicate NISN (excluding current student)
        var duplicateNisn = await _context.Students
            .AnyAsync(s => s.StudentId != student.StudentId && s.Nisn == Input.Nisn);

        if (duplicateNisn)
        {
            ModelState.AddModelError(string.Empty, "NISN sudah terdaftar pada siswa lain.");
            await PopulateSelectListsAsync();
            return Page();
        }

        // Update User info
        student.User.FullName = Input.FullName;
        student.User.Username = Input.Username;
        student.User.Email = Input.Email;
        student.User.Gender = Enum.Parse<Gender>(Input.Gender);
        student.User.IsActive = Input.IsActive;

        // Reset password if provided
        if (!string.IsNullOrEmpty(Input.NewPassword))
        {
            if (Input.NewPassword.Length < 6)
            {
                ModelState.AddModelError("Input.NewPassword", "Password minimal 6 karakter.");
                await PopulateSelectListsAsync();
                return Page();
            }
            student.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.NewPassword);
        }

        // Update Student info
        student.Nisn = Input.Nisn;
        student.ClassId = Input.ClassId;
        student.Status = Input.Status;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!StudentExists(student.StudentId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        TempData["Success"] = $"Data siswa {student.User.FullName} berhasil diperbarui.";
        return RedirectToPage("./Index");
    }

    private bool StudentExists(int id)
    {
        return _context.Students.Any(e => e.StudentId == id);
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

    public class EditStudentInput
    {
        public int StudentId { get; set; }
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

        [Required(ErrorMessage = "NISN wajib diisi.")]
        public string Nisn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kelas wajib dipilih.")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Status wajib diisi.")]
        public string Status { get; set; } = "Aktif";

        public bool IsActive { get; set; } = true;
    }
}
