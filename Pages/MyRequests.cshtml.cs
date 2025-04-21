using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoverMate.Pages
{
    [Authorize(Roles = "Teacher")]
    public class MyRequestsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
