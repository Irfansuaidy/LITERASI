using BCrypt.Net;
using Literasi.Data;
using Microsoft.EntityFrameworkCore;
using Literasi.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        var identity =
            new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (user.Role.RoleName == "Admin")
        {
            return RedirectToPage("/Admin/Dashboard/Index");
        }

        if (user.Role.RoleName == "Guru")
        {
            return RedirectToPage("/Teacher/Dashboard/Index");
        }

        if (user.Role.RoleName == "Siswa")
        {
            return RedirectToPage("/Student/Dashboard/Index");
        }

        return RedirectToPage("/Index");
    }
}