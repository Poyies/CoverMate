using CoverMate.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace CoverMate.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowupController : ControllerBase
    {
        private readonly SharedClass _sharedClass;

        public FollowupController(SharedClass sharedClass)
        {
            _sharedClass = sharedClass;
        }

        [HttpPost("Submit")]
        [AllowAnonymous] // optional depending on your use case
        public async Task<IActionResult> SubmitFollowup([FromBody] FollowupSubmitRequest model)
        {
            if (model == null || model.RequestId <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            var parameters = new
            {
                request_id = model.RequestId,
                notes = model.Notes ?? ""
            };

            string sql = "UPDATE Requests SET followup_notes = @notes WHERE request_id = @request_id";

            try
            {
                await _sharedClass.ExecuteQueryAsync(sql, false, parameters);

                // ✅ Assume success if no exception was thrown
                return Ok(new { message = "Follow-up submitted successfully." });
            }
            catch (Exception ex)
            {
                // ✅ Catch any failure
                return StatusCode(500, new { message = "Follow-up submission failed.", error = ex.Message });
            }
        }
    }
}
