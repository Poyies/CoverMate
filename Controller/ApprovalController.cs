using CoverMate.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoverMate.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [AutoValidateAntiforgeryToken]
    [Authorize(Roles = "Admin")]
    public class ApprovalController : ControllerBase
    {
        private readonly SharedClass _sharedClass;

        // Inject your shared class (assuming it's registered in DI)
        public ApprovalController(SharedClass sharedClass)
        {
            _sharedClass = sharedClass;
        }


        /// <summary>
        ///  Fetch List of subsitute request  based on teacher_id  =  User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetListofRequest")]
        public async Task<IActionResult> GetListofRequest()
        {
            // Check if the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var parameters = new { teacher_id = "", filterByTeacher = 0 };

            // Fetch data asynchronously using your shared class
            DataTable dt = await _sharedClass.GetTableAsync("GetListofRequest", true, parameters);

            // If data is found, convert DataTable rows to a list of dictionaries for JSON serialization
            if (dt != null && dt.Rows.Count > 0)
            {
                var result = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var rowDict = new Dictionary<string, object>();
                    foreach (DataColumn column in dt.Columns)
                    {
                        rowDict[column.ColumnName] = row[column];
                    }
                    result.Add(rowDict);
                }
                return Ok(new { message = "List of request", data = result });
            }
            else
            {
                return NotFound(new { message = "No data found", statuscode = "404" });
            }
        }


        [HttpGet("GetListofSubs")]
        public async Task<IActionResult> GetListofSubs()
        {
            // Check if the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            // Fetch data asynchronously using your shared class
            DataTable dt = await _sharedClass.GetTableAsync("GetActiveSubstitutesWithCourses", true, null);

            // If data is found, convert DataTable rows to a list of dictionaries for JSON serialization
            if (dt != null && dt.Rows.Count > 0)
            {
                var result = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var rowDict = new Dictionary<string, object>();
                    foreach (DataColumn column in dt.Columns)
                    {
                        rowDict[column.ColumnName] = row[column];
                    }
                    result.Add(rowDict);
                }
                return Ok(new { message = "List of active subs", data = result });
            }
            else
            {
                return NotFound(new { message = "No data found", statuscode = "404" });
            }
        }


        [HttpGet("GetRequestSummary")]
        public async Task<IActionResult> GetRequestSummary()
        {
            // Check if the user is authenticated (optional)
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            // Call the stored procedure using your shared utility
            DataTable dt = await _sharedClass.GetTableAsync("GetRequestSummary", true, null);

            if (dt != null && dt.Rows.Count > 0)
            {
                var summary = new
                {
                    total_requests = Convert.ToInt32(dt.Rows[0]["total_requests"]),
                    pending_requests = Convert.ToInt32(dt.Rows[0]["pending_requests"]),
                    approved_requests = Convert.ToInt32(dt.Rows[0]["approved_requests"])
                };

                return Ok(new { message = "Summary loaded", data = summary });
            }
            else
            {
                return NotFound(new { message = "No summary data found", statuscode = "404" });
            }
        }


        [HttpPost("AssignSubstitute")]
        public async Task<IActionResult> AssignSubstitute([FromBody] AssignRequest model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "Unauthorized access.", statuscode = 401 });
            }

            if (model == null || model.RequestId <= 0 || model.SubstituteId <= 0)
            {
                return BadRequest(new { message = "Invalid request payload.", statuscode = 400 });
            }

            var parameters = new
            {
                request_id = model.RequestId,
                substitute_id = model.SubstituteId,
                assigned_date = DateTime.Now
            };

            try
            {
                // replace sp here...
                DataTable dt = await _sharedClass.GetTableAsync("AssignSubstituteToRequest", true, parameters);

                if (dt != null && dt.Rows.Count > 0)
                {
                    string message = dt.Rows[0]["Message"].ToString();
                    int statusCode = Convert.ToInt32(dt.Rows[0]["StatusCode"]);

                    if (statusCode == 200)
                        return Ok(new { message, statuscode = statusCode });
                    else
                        return BadRequest(new { message, statuscode = statusCode });
                }
                else
                {
                    return StatusCode(500, new { message = "Unexpected error during assignment.", statuscode = 500 });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Exception occurred.", error = ex.Message });
            }
        }

    }
}
