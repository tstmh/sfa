
namespace ATTSystems.SFA.Web.Controllers
{
    using ATTSystems.NetCore.Model.HttpModel;
    using ATTSystems.NetCore.Model.ViewModel;
    using ATTSystems.SFA.Model.ViewModel;
    using ATTSystems.SFA.Web.Helper;
    using DocumentFormat.OpenXml.Office2016.Excel;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;


    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            _logger.LogInformation("User login");
            LoginViewModel model = new();
            return View(model);
        }



        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            _logger.LogInformation("UserChangePassword");
            APIRequest request = new()
            {
                RequestType = "UserChangePassword",
                UserName = User?.Identity?.Name ?? string.Empty,
                Model = model
            };

            var result = await Helper.WebAPIHelper.UserAuthAsync("/Auth/Authentication1/", request);

            if (result != null && result.IsAuth && result.AuthResult)
            {
                _logger.LogInformation("Getting user details");
                //string key = model.Username + model.Password;

                SetSession(result);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                if (result != null && result.ErrorMessage == "")
                {
                    ModelState.AddModelError("", "User does not exist in the system..! ");
                    _logger.LogInformation("User does not exist in the system..!");
                    return View(model);
                }
                if (result != null && result.AuthResult == false)
                {
                    ModelState.AddModelError("", "This account is locked out. Please contact system administrator..! ");
                    _logger.LogInformation("This account is locked out. Please contact system administrator..!");
                    return View(model);
                }
                else if (result != null && result.IsAuth == false)
                {
                    ModelState.AddModelError("", "Your failed attempt count is: " + result.ErrorMessage + " " + "[Maximum Attempt( " + result.UserId + " times)].");
                    _logger.LogInformation("Your failed attempt");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                    _logger.LogInformation("Invalid login attempt.");
                    return View(model);
                }
            }
        }

        //[HttpPost]
        //[AllowAnonymous]
        public async Task<ActionResult> Logoff()
        {
            try
            {
                if (!string.IsNullOrEmpty(User?.Identity?.Name ?? string.Empty))
                {
                    APIRequest request = new()
                    {
                        RequestType = "Save logout Details",
                        UserName = User?.Identity?.Name ?? string.Empty,
                    };

                    var logoutdetails = await Helper.WebAPIHelper.UserAuthAsync("/Auth/LogoutSave/", request);

                    //HttpContext.SignOutAsync();
                    HttpContext.Session.Clear();
                    _logger.LogInformation("getting session out");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "logoff method is getting error");
            }
            return RedirectToAction("Login", "Auth");
        }
        private async void SetSession(AuthResponse result)
        {
            _logger.LogInformation("Set session");
            var claims = new[] { new Claim(ClaimTypes.Name, result.UserId),
                new Claim(ClaimTypes.Role, "User") };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            if (result.DepartmentList != null && result.DepartmentList.Count > 0)
            {
                _logger.LogInformation("getting department list");
                string idList = string.Empty;
                foreach (var item in result.DepartmentList)
                {
                    idList = string.Format("{0},{1}", idList, item.Department_ID);
                }

                if (!string.IsNullOrEmpty(idList))
                {
                    HttpContext.Session.SetString("DEPARTMENT", idList[1..]);
                }
            }
            else
            {
                HttpContext.Session.SetString("Department", "");
            }

            if (result.ModuleList != null && result.ModuleList.Count > 0)
            {
                string idList = string.Empty;
                foreach (var item in result.ModuleList)
                {
                    idList = string.Format("{0},{1}", idList, item.ModuleName);
                }

                if (!string.IsNullOrEmpty(idList))
                {
                    HttpContext.Session.SetString("MODULE", idList[1..]);
                }
            }

            HttpContext.Session.SetString("USERKEY", result.UserId);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            _logger.LogInformation("redirect to home page");
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        #region rakesh
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword()
        {
            _logger.LogInformation("Forgot password method");
            ResetCodeViewModel model = new()
            {
                //Email = string.Empty,
                //Captcha = string.Empty,
                //CaptchaImage = SetupCaptchaBase64Image(),
            };
            PasswordSettingViewModel model1 = new();
            APIRequest request = new()
            {

            };
            var res = await WebAPIHelper.AppRequestAsync("/Auth/Getpasswordlength/", request);
            if (res != null)
            {
                if (res.Passwordsetting != null)
                {
                    _logger.LogInformation("Getting password setting count");
                    model1.Passwordsetting = res.Passwordsetting;
                    ViewBag.pswdlenght = res.Passwordsetting[0].MinPwdLength;
                    ViewBag.minupper = res.Passwordsetting[0].MinUpperCase;
                    ViewBag.minlower = res.Passwordsetting[0].MinLowerCase;
                    ViewBag.minspch = res.Passwordsetting[0].MinSpecialCharacter;
                    ViewBag.minnum = res.Passwordsetting[0].MinNumeric;
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ValidateUsername(string username)
        {
            _logger.LogInformation("Validate the user name");
            dynamic showMessageString;
            APIRequest request = new()
            {
                UserName = username
            };
            var res = await WebAPIHelper.AppRequestAsync("/Auth/ValidateUsername/", request);
            if (res != null)
            {
                if (res.Succeeded)
                {
                    showMessageString = new
                    {
                        Code = 200,
                        Message = "Success"

                    };
                    _logger.LogInformation("Validate the user name success");
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 202,
                        Message = "Failure"
                    };
                    _logger.LogInformation("Validate the user name is failure");
                }
            }
            else
            {
                showMessageString = new
                {
                    Code = 300,
                    Message = "failure"
                };
                _logger.LogInformation("user name is failure");
            }
            return Json(showMessageString);
        }
        [HttpPost]
        public async Task<ActionResult> ResetForgotPassword(ResetCodeViewModel model)
        {
            _logger.LogInformation("Reset forgot password");
            dynamic showMessageString=string.Empty;
            APIRequest request = new()
            {
                Model = model
            };
            var resp = await WebAPIHelper.AppRequestAsync("/Auth/ResetForgotPassword/", request);
            if (resp != null)
            {

                if (resp != null && resp.Succeeded)
                {
                    showMessageString = new
                    {
                        Code = 200,
                        Message = "Success",
                    };
                    _logger.LogInformation("Reset forgot password success");
                }
                else if (resp!=null && resp.Code == -3)
                {
                    ViewBag.StatusMessage = "";
                    showMessageString = new
                    {
                        Code = -3,
                        Message = resp.Message,
                    };
                }
                else
                {
                    string msg = "Failed to reset password ";
                    if (resp != null && !string.IsNullOrEmpty(resp.Message))
                    {
                        msg += "- " + resp.Message;
                    }
                    _logger.LogInformation("Reset forgot password failed");
                    showMessageString = new
                    {
                        Code = 202,
                        Message = msg,
                    };
                }
            }
            return Json(showMessageString);
        }
        #endregion
    }
}
