using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;
using System;
using System.Data;
using System.Threading.Tasks;

namespace CoverMate.Pages
{
    public class FollowupModel : PageModel
    {
        private readonly SharedClass _sharedClass;

        public FollowupModel(SharedClass sharedClass)
        {
            _sharedClass = sharedClass;
        }

        [BindProperty(SupportsGet = true)]
        public int RequestId { get; set; }

        public bool IsAlreadySubmitted { get; set; }

        public async Task<IActionResult> OnGet()
        {
            if (RequestId <= 0)
            {
                return NotFound();
            }

            string query = "SELECT followup_notes FROM Requests WHERE request_id = @requestId";
            var parameters = new { requestId = RequestId };

            DataTable dt = await _sharedClass.GetTableAsync(query, false, parameters); // false = raw SQL

            if (dt == null || dt.Rows.Count == 0)
                return NotFound();

            var notes = dt.Rows[0]["followup_notes"]?.ToString();
            IsAlreadySubmitted = !string.IsNullOrWhiteSpace(notes);

            return Page();
        }

    }
}
