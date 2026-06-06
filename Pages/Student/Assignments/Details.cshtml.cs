using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Literasi.Pages.Student.Assignments;

public class DetailsRedirectModel : PageModel
{
    public void OnGet(int id)
    {
        // Server-side redirect is performed in the .cshtml; keep code-behind minimal
    }
}
