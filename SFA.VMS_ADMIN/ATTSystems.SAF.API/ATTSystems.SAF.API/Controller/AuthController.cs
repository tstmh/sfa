namespace ATTSystems.SAF.API.Controller
{
    using ATTSystems.NetCore;
    using ATTSystems.NetCore.Logger;
    using ATTSystems.NetCore.Model.DBModel;
    using ATTSystems.NetCore.Model.HttpModel;
    using ATTSystems.NetCore.Model.ViewModel;
    using ATTSystems.SFA.DAL;
    using ATTSystems.SFA.DAL.Interface;
    using ATTSystems.SFA.Model.DBModel;
    using ATTSystems.SFA.Model.HttpModel;
    using ATTSystems.SFA.Model.ViewModel;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System.Net.Mail;

    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IAuthentication _authentication;
        private IBaseRepository _ibaseReposit;
        private readonly ILogger<AuthController> _logger;
        private ISetting _set;
        public AuthController(IAuthentication authentication, IBaseRepository baseRepository, ISetting setting, ILogger<AuthController> logger)
        {
            _authentication = authentication;
            _ibaseReposit = baseRepository;
            _set = setting;
            _logger = logger;
        }
        [HttpPost]
        [Route("RequestAccountReset")]
        public APIResponse RequestAccountReset(APIRequest request)
        {
            APIResponse result = new APIResponse();
            _logger.LogInformation("Request Account Reset");
            try
            {
                result.Succeeded = _authentication.RequestAccountReset(request);
                if (result.Succeeded)
                {
                    string key = _ibaseReposit.GetErrorMsg();
                    string url = string.Empty;
                    List<Setting> urlSetting = _authentication.GetSettingByType("URL");
                    if (urlSetting != null && urlSetting != null)
                    {
                        url = urlSetting.FirstOrDefault().Value ?? string.Empty;
                    }

                    if (!string.IsNullOrEmpty(url))
                    {
                        url += url.EndsWith("/") ? "Auth/SecureReset/" : "/Auth/SecureReset/";
                        url = string.Format("{0}{1}", url, key);

                        List<NetCore.Model.DBModel.Setting> SMTPList = _authentication.GetSettingByType("SMTP");
                        if (SMTPList != null && SMTPList.Count > 0)
                        {
                            string msg = ResetPasswordSendMail(url, request.RequestString, SMTPList);
                            if (string.IsNullOrEmpty(msg))
                            {
                                result.Succeeded = true;
                            }
                            else
                            {
                                result.Succeeded = false;
                                result.Message = msg;
                            }
                        }
                        else
                        {
                            result.Succeeded = false;
                            result.Message = "Email Details not available.";
                            _logger.LogInformation("Email Details not available.");
                        }
                    }
                    else
                    {
                        result.Succeeded = false;
                        result.Message = "Invalid based URL";
                        _logger.LogInformation("Invalid based URL");
                    }
                }
                else
                {
                    result.Message = _ibaseReposit.GetErrorMsg();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request account reset is getting error");
                //  LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: AuthController, Action: GetModuleList, Message: {0} ", ex.Message));
            }

            return result;
        }

        [HttpPost]
        [Route("GetResetJobRequest")]
        public APIResponse GetResetJobRequest(APIRequest request)
        {
            APIResponse result = new APIResponse();
            try
            {
                //  LoggerHelper.Instance.TraceLog(string.Format("Incoming - GetResetJobRequest"));
                _logger.LogInformation("Get reset job request");
                result.Code = _authentication.GetUserByRowId(request.RequestString);
                if (result.Code > 0)
                {
                    result.Succeeded = true;
                }
                else
                {
                    result.Message = _ibaseReposit.GetErrorMsg();
                    if (string.IsNullOrEmpty(result.Message))
                    {
                        result.Message = "Invalid request";
                        _logger.LogInformation("Invalid request");
                        //   LoggerHelper.Instance.TraceLog(string.Format("Failed - GetResetJobRequest - Invalid request"));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset job request is getting error");
                // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Auth, Action: GetResetJobRequest, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
            return result;
        }

        [HttpPost]
        [Route("Authentication")]
        public AuthResponse Authentication(LoginViewModel loginViewModel)
        {
            _logger.LogInformation("Authentication method");
            string userName = loginViewModel != null && !string.IsNullOrEmpty(loginViewModel.Username) ? loginViewModel.Username : string.Empty;

            AuthResponse resp = new AuthResponse
            {
                IsAuth = false,
                AuthResult = false,
                DepartmentList = null,
                RoleList = null,
                ErrorMessage = "Internal error!",
                UserId = userName,
            };

            try
            {
                var user = _authentication.GetUser(userName);
                resp.IsAuth = true;
                if (user != null)
                {
                    if (CryptographyHash.VerifyHashedPassword(user.PasswordHash, loginViewModel.Password))
                    {
                        resp.RoleList = ConsolidateRoles(user.Roles);
                        resp.DepartmentList = ConsolidateDepartment(user.Departments);
                        resp.ModuleList = ConsolidateModule(_authentication.GetUserModuleByRole(user.Roles));  //user.ModuleList != null ? user.ModuleList : null;
                        resp.AuthResult = true;
                        resp.ErrorMessage = "OK";
                    }
                    else
                    {
                        resp.ErrorMessage = "Invalid user password!";

                        //Add Password invalid

                    }
                }
                else
                {
                    // Invalid User Name Insertion



                    if (!string.IsNullOrEmpty(_ibaseReposit.GetErrorMsg()))
                    {
                        resp.ErrorMessage = "Invalid user name!";
                    }
                    else
                    {
                        resp.ErrorMessage = _ibaseReposit.GetErrorMsg();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User authentication is getting error");
                //  LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Auth, Action: Authentication, Message: {0} ", ex.Message));
            }
            return resp;
        }

        [HttpPost]
        [Route("ResetSecureCode")]
        public APIResponse ResetSecureCode(APIRequest request)
        {
            APIResponse result = new APIResponse();
            try
            {
                _logger.LogInformation("Incoming reset secure code");
                int rowId = 0;
                if (int.TryParse(request.RequestString, out rowId))
                {

                    result.Succeeded = _authentication.ResetSecureCode(rowId, request.Message);
                    if (!result.Succeeded)
                    {
                        result.Message = _ibaseReposit.GetErrorMsg();
                    }
                }
                else
                {

                    _logger.LogInformation("Failed - ResetSecureCode Invalid User");
                    result.Message = "Invalid User.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResetSecureCode is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Auth, Action: ResetSecureCode, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
            return result;
        }



        private List<RoleViewModel> ConsolidateRoles(ICollection<NetCore.Model.DBModel.Role> roleList)
        {
            List<RoleViewModel> result = null;
            //user.Roles != null ? user.Roles.ToList() : null;
            if (roleList != null)
            {
                result = new List<RoleViewModel>();
                foreach (var item in roleList)
                {
                    result.Add(new RoleViewModel
                    {
                        Id = item.Id,
                        AllowPermission = item.AllowPermission,
                        Description = item.Description,
                        Name = item.Name,
                    });
                }
            }

            return result;
        }


        private List<DepartmentViewModel> ConsolidateDepartment(ICollection<NetCore.Model.DBModel.Department> departList)
        {
            List<DepartmentViewModel> result = null;

            if (departList != null)
            {
                result = new List<DepartmentViewModel>();
                foreach (var item in departList)
                {
                    result.Add(new DepartmentViewModel
                    {
                        Department_ID = item.DepartmentId,
                        Department_Name = item.DepartmentName,
                        Description = item.Description
                    });
                }
            }

            return result;
        }

        private List<ModuleViewModel> ConsolidateModule(List<NetCore.Model.DBModel.Module> moduleList)
        {
            List<ModuleViewModel> result = null;
            if (moduleList != null)
            {
                result = new List<ModuleViewModel>();
                foreach (var item in moduleList)
                {
                    result.Add(new ModuleViewModel
                    {
                        Id = item.Id,
                        ModuleId = item.ModuleId,
                        ModuleName = item.ModuleName
                    });
                }
            }

            return result;
        }

        private string ResetPasswordSendMail(string url, string receipentEmail, List<NetCore.Model.DBModel.Setting> SMTPList)
        {
            string result = string.Empty;
            try
            {

                _logger.LogInformation("ResetPasswordSendMail 1 ------ Started");
                int port = 25;
                MailMessage msg = new MailMessage();
                msg.To.Add(new MailAddress(receipentEmail));
                msg.Subject = "Reset your Account Password";
                msg.Body = "Dear User,<br><br>You have requested to reset your password, please click <a href='" + url + "'>HERE</a> or copy the following link to your browser.<br><br>";
                msg.Body += url;
                msg.Body += "<br>This link will expired in next 15 minutes. If you're still having trouble resetting your password, please contact oour staff.";
                msg.Body += "<br><br>Sincerely,<br>VMS Team<br><br>Please do not reply to this message.";
                msg.IsBodyHtml = true;
                msg.From = new MailAddress(SMTPList.FirstOrDefault(x => x.Field == "SenderEmail").Value);
                string strPort = SMTPList.FirstOrDefault(x => x.Field == "SMTPPort").Value;
                if (!int.TryParse(strPort, out port))
                {
                    port = 25;
                }

                string replyAddr = SMTPList.FirstOrDefault(x => x.Field == "ReplyEmail").Value;
                if (!string.IsNullOrEmpty(replyAddr))
                {
                    msg.ReplyToList.Add(replyAddr);
                }
                else
                {
                    msg.ReplyToList.Add(msg.From);
                }


                SmtpClient smtp = new SmtpClient();

                _logger.LogInformation("ResetPasswordSendMail 2.");
                smtp.Host = SMTPList.FirstOrDefault(x => x.Field == "SMTPServer").Value;
                smtp.Port = port;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential(SMTPList.FirstOrDefault(x => x.Field == "SMTPUserId").Value, SMTPList.FirstOrDefault(x => x.Field == "SMTPPassword").Value);
                smtp.Send(msg);
                msg.Dispose();
                smtp.Dispose();
                _logger.LogInformation("ResetPasswordSendMail completed.");


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset password send mail getting ");
                result = ex.Message;

            }

            return result;
        }
        #region rakesh
        [HttpPost]
        [Route("Getpasswordlength")]
        public async Task<IActionResult> Getpasswordlength(APIRequest request)
        {
            PasswordSettingViewModel model = new PasswordSettingViewModel();
            AppResponse result = new AppResponse();


            try
            {
                _logger.LogInformation("GET password length");
                var res = await _authentication.GetpasswordlengthAsync();
                if (res != null)
                {
                    result.Passwordsetting = res.Passwordsetting;
                    result.Code = 1;
                    result.Message = "Password loaded";
                    result.Succeeded = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get password length is getting error");
                // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: AuthController, Action: Getpasswordlength, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }


            return Ok(result);
        }
        [HttpPost]
        [Route("UserChangePassword")]
        public async Task<IActionResult> UserChangePassword(APIRequest request)
        {

            AppResponse result = new AppResponse();
            try
            {
                ResetViewModel result1 = JsonConvert.DeserializeObject<ResetViewModel>(request.Model.ToString());

                _logger.LogInformation("User change password");
                var user = await _authentication.GetUserAsync(request);
                if (user != null)
                {
                    if (CryptographyHash.VerifyHashedPassword(user.PasswordHash, result1.OldPassword))
                    {


                        int pswdid = 1;
                        List<PasswordSetting> pwdsetting = await _set.GetPasswordSettingAsync(pswdid);
                        if (pwdsetting != null)
                        {
                            int countedLetters = 0;
                            int upper_count = 0;
                            int lower_count = 0;
                            int number_count = 0;
                            int special_count = 0;
                            if (result1.NewPassword != null)
                            {
                                foreach (char c in result1.NewPassword)
                                {
                                    if (Char.IsUpper(c))
                                        upper_count++;

                                    else if (Char.IsLower(c))
                                        lower_count++;

                                    else if (Char.IsDigit(c))
                                        number_count++;

                                    else if (Char.IsPunctuation(c))
                                        special_count++;

                                }
                            }

                            countedLetters = upper_count + lower_count + number_count + special_count;

                            if (upper_count < pwdsetting[0].MinUpperCase || lower_count < pwdsetting[0].MinLowerCase || number_count < pwdsetting[0].MinNumeric || special_count < pwdsetting[0].MinSpecialCharacter || countedLetters < pwdsetting[0].MinPwdLength)
                            {
                                result.Succeeded = false;
                                result.Message = "Passwords must be at least " + pwdsetting[0].MinPwdLength + " characters and contain 4 of the following: at least " + pwdsetting[0].MinUpperCase + " upper case (A-Z), at least " + pwdsetting[0].MinUpperCase + " lower case (a-z), at least " + pwdsetting[0].MinNumeric + " number (0-9) and at least " + pwdsetting[0].MinSpecialCharacter + " special character (e.g. !@#$%^&*)";
                                //result.Message = "New Password should contains at least " + pwdsetting[0].MinUpperCase +  " uppercase letter.";
                                result.Code = -3;
                            }

                            else
                            {
                                string msg = await _authentication.ChangePasswordAsync(CryptographyHash.HashPassword(result1.NewPassword), request);

                                if (string.IsNullOrEmpty(msg))
                                {
                                    result.Succeeded = true;
                                    result.Message = "Password successfully changed.";
                                    result.Code = 1;
                                    _logger.LogInformation("Password successfully changed.");
                                }
                                else
                                {
                                    result.Succeeded = false;
                                    result.Message = msg;
                                    result.Code = -3;
                                }
                            }
                        }
                    }
                    else
                    {
                        result.Succeeded = false;
                        result.Message = "User's old password does not match";
                        result.Code = -2;
                        _logger.LogInformation("User's old password does not match");
                    }
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = "User is not found in the system";
                    result.Code = -1;
                    _logger.LogInformation("User is not found in the system");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserChangePassword is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Auth, Action: ChangePassword, Message: {0} ", ex.Message));
            }

            return Ok(result);
        }


        [HttpPost]
        [Route("ValidateUsername")]
        public async Task<IActionResult> ValidateUsername(APIRequest request)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("ValidateUsername");

            try
            {

                var res = await _authentication.ValidateUsernameAsync(request);
                if (res > 0)
                {
                    result.Code = 1;
                    result.Message = "Valid User";
                    result.Succeeded = true;
                }
                // response.Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValidateUsername is getting error");
                // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: AuthController, Action: ValidateUsername, Message: {0} ", ex.Message));

            }
            return Ok(result);
        }

        [HttpPost]
        [Route("ResetForgotPassword")]
        public async Task<IActionResult> ResetForgotPassword(APIRequest request)
        {
            APIResponse result = new APIResponse();

            ResetCodeViewModel model = JsonConvert.DeserializeObject<ResetCodeViewModel>(request.Model.ToString());

            try
            {

                _logger.LogInformation("Reset forgot password");
                var user = await _authentication.GetUserAsync(request);


                if (user != null)
                {

                    string msg = await _authentication.ResetForgotPasswordAsync(CryptographyHash.HashPassword(model.NewPassword), model);

                    if (string.IsNullOrEmpty(msg))
                    {
                        result.Succeeded = true;
                        result.Message = "Password successfully changed.";
                        result.Code = 1;
                        _logger.LogInformation("Password successfully changed.");
                    }
                    else
                    {
                        result.Succeeded = false;
                        result.Message = msg;
                        result.Code = -3;
                    }

                }
                else
                {

                    result.Succeeded = false;
                    result.Message = "User is not found in the system";
                    result.Code = -1;
                    _logger.LogInformation("User is not found in the system");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResetForgotPassword is getting error");
                // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Auth, Action: ChangePassword, Message: {0} ", ex.Message));
            }



            return Ok(result);
        }

        [HttpPost]
        [Route("Authentication1")]
        public async Task<IActionResult> Authentication1(/*LoginViewModel loginViewModel,*/ APIRequest request)
        {
            LoginViewModel result1 = JsonConvert.DeserializeObject<LoginViewModel>(request.Model.ToString());

            AuthResponse resp = new AuthResponse
            {
                IsAuth = false,
                AuthResult = false,
                DepartmentList = null,
                RoleList = null,
                UserId = result1.Username,
            };

            try
            {
                _logger.LogInformation("Get user authentication");
                var user = await _authentication.GetUserAsync(request);
                resp.IsAuth = true;
                if (user != null)
                {
                    if (CryptographyHash.VerifyHashedPassword(user.PasswordHash, result1.Password))
                    {
                        var resetlockout = await _authentication.ResetLockOutAsync(result1.Username);
                        if (resetlockout == -1)
                        {
                            resp.AuthResult = false;
                            resp.ErrorMessage = "Account lockout Started!";
                            _logger.LogInformation("Account lockout Started!");
                        }
                        else
                        {
                            resp.RoleList = ConsolidateRoles(user.Roles);
                            resp.DepartmentList = ConsolidateDepartment(user.Departments);
                            resp.AuthResult = true;
                            resp.ModuleList = ConsolidateModule(user.ModuleList);
                            var usersession = await _authentication.SaveUserSessionAsync(result1.Username);
                            resp.ErrorMessage = usersession.ToString();


                        }
                    }
                    else
                    {
                        //Invalid Password audit

                        var res = await _authentication.WrongPasswordAsync(result1.Username);

                        //Invalid Password audit

                        var dblockcount = await _authentication.GetLockOutCount();
                        //int dblockcount = Convert.ToInt32(ConfigurationManager.AppSettings["maxfailcount"]);
                        var lockoutcount = await _authentication.CheckLockOutAsync(result1.Username);
                        if (lockoutcount == dblockcount)
                        {
                            resp.AuthResult = false;
                            resp.ErrorMessage = "Account lockout Started!";
                            _logger.LogInformation("Account lockout Started!");
                        }
                        else if (lockoutcount <= dblockcount)
                        {
                            resp.IsAuth = false;
                            resp.AuthResult = true;
                            resp.ErrorMessage = lockoutcount.ToString();
                            resp.UserId = dblockcount.ToString();
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(_authentication.GetErrorMsg()))
                    {

                        resp.ErrorMessage = "Invalid login attempt!";
                        _logger.LogInformation("Invalid login attempt!");
                    }
                    else
                    {
                        resp.ErrorMessage = _authentication.GetErrorMsg();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User authentication is getting error");

            }
            return Ok(resp);
        }


        //logout Save details
        [HttpPost]
        [Route("LogoutSave")]
        public async Task<ActionResult> LogoutSave(APIRequest request)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Saving Logged Out User Details");

            try
            {

                var res = await _authentication.LogoutSaveAsync(request.UserName);
                if (res > 0)
                {
                    result.Code = 1;
                    result.Message = "User Logout details saved successfully";
                    result.Succeeded = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saving Logout Details getting error");
            }
            return Ok(result);
        }

        #endregion
    }
}
