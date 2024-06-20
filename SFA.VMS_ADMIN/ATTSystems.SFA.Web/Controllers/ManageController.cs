using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.Model.ViewModel;
using ATTSystems.SFA.Web.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATTSystems.SFA.Web.Controllers
{
    [NoDirectAccess]
    [Authorize]
    [StateManager]
    public class ManageController : Controller
    {
        private readonly ILogger<ManageController> _logger;

        private IConfiguration config;

        public ManageController(IConfiguration _configuration, ILogger<ManageController> logger)
        {
            config = _configuration;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region rakesh
        public async Task<ActionResult> ChangePassword()
        {
            _logger.LogInformation("Change password");
            ResetViewModel model = new ResetViewModel
            {
                Username = User?.Identity?.Name ?? string.Empty
            };
            PasswordSettingViewModel model1 = new PasswordSettingViewModel();
            APIRequest request = new APIRequest()
            {

            };
            var res = await WebAPIHelper.AppRequestAsync("/Auth/Getpasswordlength/", request);
            if (res != null)
            {
                if (res.Passwordsetting != null)
                {
                    model1.Passwordsetting = res.Passwordsetting;
                    ViewBag.pswdlenght = res.Passwordsetting[0].MinPwdLength;
                    ViewBag.minupper = res.Passwordsetting[0].MinUpperCase;
                    ViewBag.minlower = res.Passwordsetting[0].MinLowerCase;
                    ViewBag.minspch = res.Passwordsetting[0].MinSpecialCharacter;
                    ViewBag.minnum = res.Passwordsetting[0].MinNumeric;
                    _logger.LogInformation("Getting password setting count");
                }
            }
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> UpdateChangePassword(ResetViewModel model)
        {
            _logger.LogInformation("Update change password");
            APIRequest request = new APIRequest
            {
                RequestType = "UserChangePassword",
                UserName = User?.Identity?.Name ?? string.Empty,
                Model = model
            };
            dynamic showMessageString = string.Empty;
            try
            {
                string errmsg = string.Empty;
                if (model != null)
                {
                    if (model.NewPassword == model.ConfirmPassword)

                    {
                        model.Username = User?.Identity?.Name ?? string.Empty;

                        string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
                        var result = await WebAPIHelper.ChangePasswordAsync(baseuri, "/Auth/UserChangePassword/", request);
                        if (result != null)
                        {
                            if (result != null && result.Succeeded)
                            {
                                ViewBag.StatusMessage = "Password Change have been successfully";
                                showMessageString = new
                                {
                                    Code = 200,
                                    Message = "Success.",
                                };
                                _logger.LogInformation("Password Change have been successfully");
                            }
                            else if (result != null && !result.Succeeded && result.Code == -2)
                            {
                                ViewBag.StatusMessage = "User's old password does not match";
                                showMessageString = new
                                {
                                    Code = -2,
                                    Message = "User's old password does not match",
                                };
                                _logger.LogInformation("User's old password does not match");
                            }
                            else if (result != null && !result.Succeeded && result.Code == -3)
                            {
                                ViewBag.StatusMessage = "";
                                showMessageString = new
                                {
                                    Code = -3,
                                    Message = result.Message,
                                };
                            }
                            else
                            {
                                string msg = result != null ? result.Message : "Internal error.";
                                ModelState.AddModelError("", msg);
                                _logger.LogInformation("Internal error.");
                            }
                        }
                    }
                    else
                    {
                        showMessageString = new
                        {
                            Code = 202,
                            Message = "New and Confirmed Password are different.",
                        };
                        _logger.LogInformation("New and Confirmed Password are different.");
                    }
                }

                else
                {
                    foreach (string key in ViewData.ModelState.Keys)
                    {
                        string modelStateError = string.Empty;
                        var modelState = ViewData.ModelState[key];
                        if (modelState != null && modelState.Errors != null && modelState.Errors.Count > 0)
                        {
                            string? msg = modelState.Errors.First().ErrorMessage;
                            string tmp = key.Replace("SecureCode", "Password");
                            errmsg = string.Format(@"{0}<br />{1}:{2}", errmsg, tmp, msg);
                        }
                    }
                    showMessageString = new
                    {
                        Code = 201,
                        Message = !string.IsNullOrEmpty(errmsg) ? errmsg.Substring(6) : errmsg,
                        ModalType = "Add",
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update change password is getting error");
                // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Controller: Manage, Action: ChangePassword, Message: {0} ", ex.Message));
                // return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error in processing.");
            }
            return Json(showMessageString);
        }



        #endregion
    }
}
