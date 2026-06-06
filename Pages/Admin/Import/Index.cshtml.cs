using Literasi.Data;
using Literasi.Models;
using StudentModel = Literasi.Models.Student;
using TeacherModel = Literasi.Models.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Literasi.Models.Enums;
using System.IO;
using System.Text;

namespace Literasi.Pages.Admin.Import;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public IFormFile? UploadedFile { get; set; }

    [BindProperty]
    public string ImportType { get; set; } = "Siswa";

    public List<string> SuccessLogs { get; set; } = new();
    public List<string> ErrorLogs { get; set; } = new();
    public List<ClassInfoDto> AvailableClasses { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadAvailableClassesAsync();
    }

    private async Task LoadAvailableClassesAsync()
    {
        AvailableClasses = await _context.Classes
            .Include(c => c.AcademicYear)
            .OrderBy(c => c.ClassName)
            .Select(c => new ClassInfoDto
            {
                ClassId = c.ClassId,
                ClassName = c.ClassName,
                AcademicYear = c.AcademicYear.YearName
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAvailableClassesAsync();

        if (UploadedFile == null || UploadedFile.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Silakan pilih file CSV terlebih dahulu.");
            return Page();
        }

        var fileExtension = Path.GetExtension(UploadedFile.FileName).ToLower();
        if (fileExtension != ".csv")
        {
            ModelState.AddModelError(string.Empty, "File harus berformat .csv.");
            return Page();
        }

        var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Siswa");
        var teacherRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Guru");

        if (studentRole == null || teacherRole == null)
        {
            ModelState.AddModelError(string.Empty, "Role 'Siswa' atau 'Guru' tidak ditemukan di database.");
            return Page();
        }

        using var reader = new StreamReader(UploadedFile.OpenReadStream(), Encoding.UTF8);
        string? headerLine = await reader.ReadLineAsync();

        if (string.IsNullOrEmpty(headerLine))
        {
            ModelState.AddModelError(string.Empty, "File CSV kosong.");
            return Page();
        }

        int rowNumber = 1;
        string? line;

        if (ImportType == "Siswa")
        {
            // Expected columns: FullName,Username,Email,Password,Gender,Nisn,ClassId
            while ((line = await reader.ReadLineAsync()) != null)
            {
                rowNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 7)
                {
                    ErrorLogs.Add($"Baris {rowNumber}: Format kolom tidak lengkap (harus 7 kolom).");
                    continue;
                }

                string fullName = parts[0].Trim();
                string username = parts[1].Trim();
                string email = parts[2].Trim();
                string password = parts[3].Trim();
                string genderStr = parts[4].Trim().ToUpper();
                string nisn = parts[5].Trim();
                string classIdStr = parts[6].Trim();

                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || 
                    string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || 
                    string.IsNullOrEmpty(genderStr) || string.IsNullOrEmpty(nisn) || 
                    string.IsNullOrEmpty(classIdStr))
                {
                    ErrorLogs.Add($"Baris {rowNumber}: Ada kolom wajib yang kosong.");
                    continue;
                }

                // Validate gender
                if (genderStr != "L" && genderStr != "P")
                {
                    ErrorLogs.Add($"Baris {rowNumber}: Jenis kelamin '{genderStr}' tidak valid (harus L atau P).");
                    continue;
                }

                // Validate ClassId
                if (!int.TryParse(classIdStr, out int classId))
                {
                    ErrorLogs.Add($"Baris {rowNumber}: ClassId '{classIdStr}' harus berupa angka.");
                    continue;
                }

                var classExists = await _context.Classes.AnyAsync(c => c.ClassId == classId);
                if (!classExists)
                {
                    ErrorLogs.Add($"Baris {rowNumber}: ClassId '{classId}' tidak terdaftar di database.");
                    continue;
                }

                // Check database duplicates
                var userExists = await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
                if (userExists)
                {
                    ErrorLogs.Add($"Baris {rowNumber}: Username '{username}' atau Email '{email}' sudah digunakan.");
                    continue;
                }

                var nisnExists = await _context.Students.AnyAsync(s => s.Nisn == nisn);
                if (nisnExists)
                {
                    ErrorLogs.Add($"Baris {rowNumber}: NISN '{nisn}' sudah terdaftar.");
                    continue;
                }

                // Create user and student
                try
                {
                    var user = new User
                    {
                        FullName = fullName,
                        Username = username,
                        Email = email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                        Gender = Enum.Parse<Gender>(genderStr),
                        IsActive = true,
                        RoleId = studentRole.RoleId
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    var student = new StudentModel
                    {
                        UserId = user.UserId,
                        Nisn = nisn,
                        ClassId = classId,
                        Status = "Aktif"
                    };

                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    SuccessLogs.Add($"Baris {rowNumber}: Siswa '{fullName}' berhasil didaftarkan.");
                }
                catch (Exception ex)
                {
                    ErrorLogs.Add($"Baris {rowNumber}: Error saat menyimpan database ({ex.Message}).");
                }
            }
        }
        else if (ImportType == "Guru")
        {
            // Expected columns: FullName,Username,Email,Password,Gender,Nip
            while ((line = await reader.ReadLineAsync()) != null)
            {
                rowNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 6)
                {
                    ErrorLogs.Add($"Baris {rowNumber}: Format kolom tidak lengkap (harus 6 kolom).");
                    continue;
                }

                string fullName = parts[0].Trim();
                string username = parts[1].Trim();
                string email = parts[2].Trim();
                string password = parts[3].Trim();
                string genderStr = parts[4].Trim().ToUpper();
                string nip = parts[5].Trim();

                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || 
                    string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || 
                    string.IsNullOrEmpty(genderStr) || string.IsNullOrEmpty(nip))
                {
                    ErrorLogs.Add($"Baris {rowNumber}: Ada kolom wajib yang kosong.");
                    continue;
                }

                // Validate gender
                if (genderStr != "L" && genderStr != "P")
                {
                    ErrorLogs.Add($"Baris {rowNumber}: Jenis kelamin '{genderStr}' tidak valid (harus L atau P).");
                    continue;
                }

                // Check database duplicates
                var userExists = await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
                if (userExists)
                {
                    ErrorLogs.Add($"Baris {rowNumber}: Username '{username}' atau Email '{email}' sudah digunakan.");
                    continue;
                }

                var nipExists = await _context.Teachers.AnyAsync(t => t.Nip == nip);
                if (nipExists)
                {
                    ErrorLogs.Add($"Baris {rowNumber}: NIP '{nip}' sudah terdaftar.");
                    continue;
                }

                // Create user and teacher
                try
                {
                    var user = new User
                    {
                        FullName = fullName,
                        Username = username,
                        Email = email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                        Gender = Enum.Parse<Gender>(genderStr),
                        IsActive = true,
                        RoleId = teacherRole.RoleId
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    var teacher = new TeacherModel
                    {
                        UserId = user.UserId,
                        Nip = nip
                    };

                    _context.Teachers.Add(teacher);
                    await _context.SaveChangesAsync();

                    SuccessLogs.Add($"Baris {rowNumber}: Guru '{fullName}' berhasil didaftarkan.");
                }
                catch (Exception ex)
                {
                    ErrorLogs.Add($"Baris {rowNumber}: Error saat menyimpan database ({ex.Message}).");
                }
            }
        }

        TempData["Success"] = $"Proses import selesai. Berhasil: {SuccessLogs.Count}, Gagal: {ErrorLogs.Count}.";
        return Page();
    }

    public class ClassInfoDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
    }
}
