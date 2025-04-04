using CoverMate.Helpers;
using CoverMate.Model;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Data;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoverMate.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyRequestController : ControllerBase
    {
        private readonly SharedClass _sharedClass;

        // Inject your shared class (assuming it's registered in DI)
        public MyRequestController(SharedClass sharedClass)
        {
            _sharedClass = sharedClass;
        }

        /// <summary>
        ///  Fetch blocklist of teacher based on teacher_id  =  User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        /// </summary>
        /// <returns></returns>
        [HttpGet("Getblocklist")]
        public async Task<IActionResult> Getblocklist()
        {
            // Check if the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "You dont have access to this route.", statuscode = 401 });
            }

            // Retrieve the teacher's ID from the current user's claims
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var parameters = new { teacher_id = teacherId };

            // Fetch data asynchronously using your shared class
            DataTable dt = await _sharedClass.GetTableAsync("GetBlocklist", true, parameters);

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
                return Ok(new { message = "List of blocks", data = result });
            }
            else
            {
                return NotFound(new { message = "No data found", statuscode = "404" });
            }
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

            // Retrieve the teacher's ID from the current user's claims
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var parameters = new { teacher_id = teacherId };

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



        /// <summary>
        /// Inserts a new substitute request for the authenticated teacher.
        /// </summary>
        /// <param name="request">Request details</param>
        /// <returns>A success message with status code 200 or an error message with status code 400.</returns>
        [HttpPost("SetNewRequest")]
        public async Task<IActionResult> SetNewRequest([FromBody] NewRequest request)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "Unauthorized access.", statuscode = 401 });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Retrieve teacher ID from user claims.
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(teacherId, out long teacherIdLong))
            {
                return BadRequest(new { message = "Invalid teacher id.", statuscode = 400 });
            }

            // Sanitize the input fields.
            var sanitizedSubplanlink = HtmlSanitizerHelper.SanitizeHtml(request.Subplanlink);
            var sanitizedReason = HtmlSanitizerHelper.SanitizeHtml(request.Reason);
            var sanitizedNotes = HtmlSanitizerHelper.SanitizeHtml(request.Notes);

            // Build parameters for the stored procedure.
            var parameters = new
            {
                teacher_id = teacherIdLong,
                schedule_id = request.ScheduleId,
                subplanlink = sanitizedSubplanlink,
                reason = sanitizedReason,
                notes = sanitizedNotes
            };

            // Execute the stored procedure asynchronously.
            DataTable dt = await _sharedClass.GetTableAsync("SetNewRequest", true, parameters);

            if (dt != null && dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                string message = row["Message"].ToString();
                int statusCode = Convert.ToInt32(row["StatusCode"]);

                return statusCode == 200
                    ? Ok(new { message, statuscode = statusCode })
                    : BadRequest(new { message, statuscode = statusCode });
            }
            else
            {
                return BadRequest(new { message = "Unexpected error occurred.", statuscode = 400 });
            }
        }



    }

}
