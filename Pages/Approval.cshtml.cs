using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoverMate.Pages
{
    [Authorize(Roles = "Approver")]
    public class ApprovalModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
