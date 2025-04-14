using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;
using System.Data;
using System.Security.Claims;

namespace CoverMate.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SharedClass _sharedClass;

        public LoginModel(SharedClass sharedClass)
        {
            _sharedClass = sharedClass;

        }
        public async Task<IActionResult> OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                string url = await GoogleResponse();
                return Redirect(url);
            }
            else
            {
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostGoogle()
        {
            var properties = new GoogleChallengeProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<string> GoogleResponse()
        {
            string redirectPage = "/";
            var role = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).SingleOrDefault();

            if (string.IsNullOrEmpty(role))
            {
                var username = "";
                var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                var googleClaims = authResult.Principal.Identities.FirstOrDefault().Claims;
                var googleClaims_email = googleClaims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
                var googleClaims_name = googleClaims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault().Value;
                //username = googleClaims_email.Split("@")[0];

                //googleClaims_email = "rmendoza@school.edu.ph";
                googleClaims_email = "asantos@school.edu.ph";
                var parameters = new
                {
                    Email = googleClaims_email
                };

                DataTable dt = await _sharedClass.GetTableAsync("GetUserLoginInfo", true, parameters);

                if (dt.Rows.Count > 0)
                {
                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.Name, dt.Rows[0]["name"].ToString()),
                        new Claim(ClaimTypes.Email, dt.Rows[0]["email"].ToString()),
                        new Claim(ClaimTypes.NameIdentifier, dt.Rows[0]["users_id"].ToString())
                    };

                    // Add role claims
                    string roleType = dt.Rows[0]["role_type"]?.ToString();

                    if (!string.IsNullOrEmpty(roleType))
                    {
                        switch (roleType.ToLower())
                        {
                            case "admin":
                                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                                redirectPage = "/AllRequests";
                                break;

                            case "teacher":
                                claims.Add(new Claim(ClaimTypes.Role, "Teacher"));
                                redirectPage = "/MyRequests";
                                break;

                            default:
                                claims.Add(new Claim(ClaimTypes.Role, "Guest"));
                                redirectPage = "/Unauthorized";
                                break;
                        }
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, GoogleDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal, new AuthenticationProperties
                    {
                        IsPersistent = true,
                    });
                }
                else
                {
                    return "/Account/Logout";
                }
            }
            else
            {
                if (role == "Admin")
                {
                    redirectPage = "/AllRequests";
                }
                else
                {
                    redirectPage = "/MyRequests";
                }
            }

            return redirectPage;
        }
    }
}
