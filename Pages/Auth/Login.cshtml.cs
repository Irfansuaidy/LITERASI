using BCrypt.Net;
using Literasi.Data;
using Microsoft.EntityFrameworkCore;
using Literasi.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Literasi.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public LoginModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public LoginViewModel Login { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u =>
                u.Username == Login.Username &&
                u.IsActive);

        if (user == null)
        {
            ModelState.AddModelError("", "Username atau password salah");
            return Page();
        }

        bool validPassword =
            BCrypt.Net.BCrypt.Verify(
                Login.Password,
                user.PasswordHash);

        if (!validPassword)
        {
            ModelState.AddModelError("", "Username atau password salah");
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.FullName),

            new Claim(
                ClaimTypes.Role,
                user.Role.RoleName)
        };

        if (user.Role.RoleName == "Admin")
        {
            await SignInRoleAsync("AdminAuth", claims);
            return RedirectToPage("/Admin/Dashboard/Index");
        }

        if (user.Role.RoleName == "Guru")
        {
            await SignInRoleAsync("TeacherAuth", claims);
            return RedirectToPage("/Teacher/Dashboard/Index");
        }

        if (user.Role.RoleName == "Siswa")
        {
            await SignInRoleAsync("StudentAuth", claims);
            return RedirectToPage("/Student/Dashboard/Index");
        }

        return RedirectToPage("/Index");
    }

    private async Task SignInRoleAsync(string scheme, List<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, scheme);
        await HttpContext.SignInAsync(scheme, new ClaimsPrincipal(identity));
    }
}
