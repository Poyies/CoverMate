using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoverMate.Pages
{
    [Authorize(Roles = "Admin")]
    public class AllRequestsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
