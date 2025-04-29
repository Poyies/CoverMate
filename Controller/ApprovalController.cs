using CoverMate.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Data;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoverMate.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [AutoValidateAntiforgeryToken]
    [Authorize(Roles = "Admin, Approver")]
    public class ApprovalController : ControllerBase
    {
        private readonly SharedClass _sharedClass;
        private readonly IWebHostEnvironment _env;

        // Inject your shared class (assuming it's registered in DI)
        public ApprovalController(SharedClass sharedClass, IWebHostEnvironment env)
        {
            _sharedClass = sharedClass;
            _env = env;
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
                return BadRequest(new { message = "Invalid request data.", statuscode = 400 });
            }

            // Get assignee (the one assigning) from claims
            var assigneeId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(assigneeId, out long assigneeIdLong))
            {
                return BadRequest(new { message = "Invalid user session.", statuscode = 400 });
            }

            var parameters = new
            {
                request_id = model.RequestId,
                substitute_id = model.SubstituteId,
                assignee_id = assigneeIdLong
                // status defaults to 2 inside the SP
            };

            DataTable dt = await _sharedClass.GetTableAsync("UpdateRequestAssignment", true, parameters);

            if (dt != null && dt.Rows.Count > 0)
            {
                string message = dt.Rows[0]["message"].ToString();
                int statusCode = Convert.ToInt32(dt.Rows[0]["statusCode"]);

                if (statusCode == 200)
                {
                    // 🔍 Fetch request & substitute details
                    var detailsParams = new { request_id = model.RequestId };
                    DataTable detailsDt = await _sharedClass.GetTableAsync("GetRequestAssignmentDetails", true, detailsParams);

                    if (detailsDt != null && detailsDt.Rows.Count > 0)
                    {
                        var row = detailsDt.Rows[0];

                        string templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "SubAssignedEmail.html");

                        string htmlBody = await System.IO.File.ReadAllTextAsync(templatePath);

                        htmlBody = htmlBody
                            .Replace("@SubstituteName", row["substitute_name"].ToString())
                            .Replace("@TeacherName", row["teacher_name"].ToString())
                            .Replace("@RequestDate", Convert.ToDateTime(row["request_date"]).ToString("MMMM dd, yyyy"))
                            .Replace("@CourseName", row["course_name"].ToString())
                            .Replace("@Room", row["room"].ToString())
                            .Replace("@Time", $"{row["day"]}, {row["time"]}")
                            .Replace("@SubLink", row["subplanlink"].ToString())
                            .Replace("@Notes", string.IsNullOrEmpty(row["notes"].ToString()) ? "—" : row["notes"].ToString());

                        string subject = $"CoverMate: You've been assigned as a substitute for {row["teacher_name"]}";
                        string recipient = row["substitute_email"].ToString();

                        EmailSender.SendEmail(subject, htmlBody, recipient, "", "");
                    }

                    return Ok(new { message, statuscode = statusCode });
                }


                return StatusCode(500, new { message, statuscode = statusCode });
            }

            return StatusCode(500, new { message = "No result returned from stored procedure.", statuscode = 500 });
        }


        /// <summary>
        /// Get sub requests that have follow-up submitted and are pending approval
        /// </summary>
        [HttpGet("GetRequestsForApproval")]
        public async Task<IActionResult> GetRequestsForApproval()
        {
            try
            {
                DataTable dt = await _sharedClass.GetTableAsync("GetRequestsForApproval", true);

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

                    return Ok(new { message = "List of requests for approval", data = result });
                }
                else
                {
                    return NotFound(new { message = "No requests found", statuscode = 404 });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal error", error = ex.Message });
            }
        }


        //[HttpPost("ApproveRequest")]
        //[Authorize(Roles = "Approver")]
        //public async Task<IActionResult> ApproveRequest([FromBody] ApproveRequest model)
        //{
        //    if (!User.Identity.IsAuthenticated)
        //        return Unauthorized();

        //    var approverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (!int.TryParse(approverId, out int approverIdInt))
        //        return BadRequest(new { message = "Invalid session." });

        //    var parameters = new { request_id = model.RequestId, approver_id = approverIdInt };

        //    DataTable dt = await _sharedClass.GetTableAsync("ApproveRequest", true, parameters);

        //    if (dt != null && dt.Rows.Count > 0)
        //    {
        //        var row = dt.Rows[0];
        //        int code = Convert.ToInt32(row["statuscode"]);
        //        string msg = row["message"].ToString();

        //        return StatusCode(code, new { message = msg });
        //    }

        //    return StatusCode(500, new { message = "Unexpected error occurred." });
        //}

        [HttpPost("ApproveRequest")]
        [Authorize(Roles = "Approver")]
        public async Task<IActionResult> ApproveRequest([FromBody] ApproveRequest model)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var approverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var principalName = User.Identity.Name;

            if (!int.TryParse(approverId, out int approverIdInt))
                return BadRequest(new { message = "Invalid session." });

            var parameters = new { request_id = model.RequestId, approver_id = approverIdInt };

            // ✅ 1. Update the request status
            DataTable dt = await _sharedClass.GetTableAsync("ApproveRequest", true, parameters);

            if (dt != null && dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                int code = Convert.ToInt32(row["statuscode"]);
                string msg = row["message"].ToString();

                if (code == 200)
                {
                    try
                    {
                        // ✅ 2. Fetch sub email, course name, request date
                        var detailParams = new { request_id = model.RequestId };
                        DataTable detailDt = await _sharedClass.GetTableAsync("GetRequestAssignmentDetails", true, detailParams);

                        if (detailDt != null && detailDt.Rows.Count > 0)
                        {
                            var drow = detailDt.Rows[0];

                            string substituteName = drow["substitute_name"].ToString();
                            string substituteEmail = drow["substitute_email"].ToString();
                            string courseName = drow["course_name"].ToString();
                            DateTime requestDate = Convert.ToDateTime(drow["request_date"]);

                            // ✅ 3. Load the email template
                            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "PaymentApproval.html");
                            string htmlBody = await System.IO.File.ReadAllTextAsync(templatePath);

                            htmlBody = htmlBody
                                .Replace("@SubstituteName", substituteName)
                                .Replace("@RequestDate", requestDate.ToString("MMMM dd, yyyy"))
                                .Replace("@CourseName", courseName)
                                .Replace("@PrincipalName", principalName);

                            string subject = $"CoverMate: Payment Approved for {substituteName}";
                            string ccEmail = "covermateapp@gmail.com"; // for testing

                            // ✅ 4. Send Email
                            EmailSender.SendEmail(subject, htmlBody, substituteEmail, ccEmail, "");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Email sending failed: {ex.Message}");
                        // Email failure should NOT block the approval action
                    }

                    return Ok(new { message = msg, statuscode = code });
                }

                return StatusCode(code, new { message = msg });
            }

            return StatusCode(500, new { message = "Unexpected error occurred." });
        }

        [HttpPost("SendFollowupEmail")]
        public async Task<IActionResult> SendFollowupEmail([FromBody] ApproveRequest model)
        {
            if (model == null || model.RequestId <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            // Query to get substitute, teacher, etc.
            string query = @"
                SELECT 
                    r.request_id,
                    r.request_date,
                    s.name AS substitute_name,
                    s.email AS substitute_email,
                    t.name AS teacher_name,
                    c.course_name
                FROM Requests r
                INNER JOIN Substitutes s ON r.substitute_id = s.substitute_id
                INNER JOIN Teachers t ON r.teacher_id = t.teachers_id
                INNER JOIN Schedules sc ON r.schedule_id = sc.schedule_id
                INNER JOIN Courses c ON sc.course_id = c.course_id
                WHERE r.request_id = @requestId
            ";

            var parameters = new { requestId = model.RequestId };
            var dt = await _sharedClass.GetTableAsync(query, false, parameters);

            if (dt == null || dt.Rows.Count == 0)
                return NotFound(new { message = "Request not found." });

            var row = dt.Rows[0];

            // Load email template
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "FollowupReminderEmail.html");
            string emailBody = await System.IO.File.ReadAllTextAsync(templatePath);

            // Replace placeholders
            string followupLink = $"https://localhost:7134/Followup?requestId={model.RequestId}";
            emailBody = emailBody
                .Replace("@SubstituteName", row["substitute_name"].ToString())
                .Replace("@TeacherName", row["teacher_name"].ToString())
                .Replace("@RequestDate", Convert.ToDateTime(row["request_date"]).ToString("MMMM dd, yyyy"))
                .Replace("@CourseName", row["course_name"].ToString())
                .Replace("@FollowupLink", followupLink);

            // 🔥 Send Email
            string subject = $"CoverMate: Please complete your follow-up form";
            string recipient = row["substitute_email"].ToString();

            bool sent = EmailSender.SendEmail(subject, emailBody, recipient, "", "");

            if (sent)
                return Ok(new { message = "Follow-up email sent successfully." });
            else
                return StatusCode(500, new { message = "Failed to send email." });
        }

    }
}
