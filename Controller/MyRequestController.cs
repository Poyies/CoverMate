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
                return Unauthorized();
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
                return Ok(result);
            }
            else
            {
                return NotFound("No data found");
            }
        }

    }

}
