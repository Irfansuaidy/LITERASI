using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Literasi.Pages.Auth;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnPost()
    {
        await HttpContext.SignOutAsync("AdminAuth");
        await HttpContext.SignOutAsync("TeacherAuth");
        await HttpContext.SignOutAsync("StudentAuth");

        return RedirectToPage("/Auth/Login");
    }
}
