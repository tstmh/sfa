namespace ATTSystems.SFA.DAL.Implementation
{
    using ATTSystems.NetCore;
    using ATTSystems.NetCore.Model.DBModel;
    using ATTSystems.NetCore.Model.HttpModel;
    using ATTSystems.NetCore.Utilities;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ATTSystems.SFA.DAL.Interface;
    using ATTSystems.SFA.Model.ViewModel;
    using Newtonsoft.Json;
    using ATTSystems.SFA.Model.DBModel;
    using Microsoft.Extensions.Logging;

    public class AuthenticationRepository : IBaseRepository, IDisposable, IAuthentication
    {

        #region Standard Practice to create a Data Repo
        private string ErrorMsg = string.Empty;
        private DataContext entity;
        private readonly ILogger<AuthenticationRepository> _logger;
        public AuthenticationRepository(ILogger<AuthenticationRepository> logger)
        {
            entity = new DataContext();
            _logger = logger;
        }

        ~AuthenticationRepository()
        {
            Dispose(false);
        }

        public string GetErrorMsg()
        {
            return ErrorMsg;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

            if (disposing && entity != null)
            {
                entity.Dispose();
            }
        }
        #endregion

        public User? GetUser(string userName)
        {
            User? result = null;

            try
            {
                _logger.LogInformation("Get user");
                result = entity.Users.Include(x => x.Departments).Include(x => x.Roles).FirstOrDefault(x => x.UserName.ToUpper() == userName.ToUpper() && x.IsDeleted == false);
                if (result != null && result.Departments != null && result.Departments.Count() > 0)
                {
                    var deptList = result.Departments.Where(x => x.IsDeleted == false);
                    if (deptList != null && deptList.Count() > 0)
                    {
                        result.Departments = deptList.ToList();
                    }
                    else
                    {
                        result.Departments = null;
                    }

                    // sort Role List
                    var roleList = result.Roles.Where(x => x.IsDeleted == false);
                    if (roleList != null)
                    {
                        result.Roles = roleList.ToList();
                    }
                    else
                    {
                        result.Roles = null;
                    }
                }
                else
                {
                    UsersSessionsTracking ust = new UsersSessionsTracking();
                    ust.UserName= userName;
                    ust.Status = "Login Failed";
                    ust.AttemptedDateTime = DateTime.Now;
                    ust.Remarks = "Invalid UserName";
                    entity.UsersSessionsTrackings.Add(ust);
                    entity.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ExceptionTools.GetInnerExceptionMessage(ex);
                _logger.LogError(ex, "not getting user");
            }

            return result;
        }

        public List<Module> GetUserModuleByRole(ICollection<Role> roleList)
        {
            List<Module> result = new List<Module>();
            try
            {
                _logger.LogInformation("Get user module by role");
                if (roleList != null && roleList.Count > 0)
                {
                    List<int> roleIds = roleList.Select(x => x.Id).ToList();
                    var roleModule = entity.Roles.Include(x => x.Modules).Where(x => roleIds.Contains(x.Id));
                    if (roleModule != null)
                    {
                        foreach (var rm in roleModule)
                        {
                            if (rm.Modules != null && rm.Modules.Count > 0)
                            {
                                foreach (var module in rm.Modules)
                                {
                                    if (module.IsDeleted == false)
                                    {
                                        Module? tmpModule = result.FirstOrDefault(x => x.Id == module.Id);
                                        if (tmpModule == null)
                                            result.Add(module);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "not getting user module");
                ErrorMsg = ExceptionTools.GetInnerExceptionMessage(ex);
            }

            return result;
        }

        public bool ResetSecureCode(int rowId, string request)
        {
            ErrorMsg = string.Empty;
            bool result = false;

            try
            {
                _logger.LogInformation("Reset secure code");
                JobRequest? job = entity.JobRequests.FirstOrDefault(x => x.Id == rowId);

                if (job != null)
                {
                    int userId = 0;
                    int.TryParse(job.UserKey, out userId);
                    User? user = entity.Users.FirstOrDefault(x => x.Id == userId);
                    if (user != null)
                    {
                        user.PasswordHash = CryptographyHash.HashPassword(request);
                        entity.SaveChanges();

                        job.JobStatus = 1;
                        job.UpdateBy = user.UserName;
                        job.UpdateDateTime = DateTime.Now;
                        entity.SaveChanges();
                        result = true;
                    }
                }
                else
                {
                    ErrorMsg = "Invalid user";
                    _logger.LogInformation("Invalid user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to reset secure code");
                ErrorMsg = ExceptionTools.GetInnerExceptionMessage(ex);
            }


            return result;
        }

        public bool RequestAccountReset(APIRequest request)
        {
            ErrorMsg = string.Empty;
            bool result = false;

            try
            {
                _logger.LogInformation("Request account reset");
                User? user = entity.Users.FirstOrDefault(x => x.Email.ToUpper() == request.RequestString.ToUpper() && x.IsDeleted == false);
                if (user != null)
                {
                    string[] tok = request.RequestString.Split('@');
                    string reqUser = tok[0].Length > 50 ? tok[0].Substring(0, 50) : tok[0];
                    DateTime now = DateTime.Now;
                    string key = string.Format("{0}{1}{2}", now.ToString("HHmmss"), user.UserName, now.ToString("yyyyMMdd"));
                    key = CryptographyHash.ConvertToSHA256(key);
                    entity.JobRequests.Add(new JobRequest
                    {
                        UserId = 0,
                        UserKey = user.Id.ToString(),
                        JobType = "RESETPWD",
                        JobKey = key,
                        JobStatus = 0,
                        JobMessage = "NA",
                        ExpiryDateTime = now.AddMinutes(15),
                        CreateDateTime = now,
                        CreateBy = reqUser,
                    });

                    entity.SaveChanges();
                    ErrorMsg = key;
                    result = true;
                }
                else
                {
                    ErrorMsg = "Invalid User Account.";
                    _logger.LogInformation("Invalid User Account.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed account reset");
                ErrorMsg = ExceptionTools.GetInnerExceptionMessage(ex);
            }


            return result;
        }

        public List<Setting> GetSettingByType(string type)
        {
            ErrorMsg = string.Empty;
            List<Setting> result = new List<Setting>();

            try
            {
                _logger.LogInformation("Get setting by type");
                var list = entity.Settings.Where(x => x.Type == type);
                if (list != null)
                {
                    result = list.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to getting by type");
                ErrorMsg = ExceptionTools.GetInnerExceptionMessage(ex);
            }


            return result;
        }

        public int GetUserByRowId(string key)
        {
            ErrorMsg = string.Empty;
            int result = 0;

            try
            {
                _logger.LogInformation("Get user by row id");
                DateTime now = DateTime.Now;
                JobRequest? job = entity.JobRequests.FirstOrDefault(x => x.JobKey == key && x.JobType == "RESETPWD");
                if (job != null)
                {
                    if (job.JobStatus == 0)
                    {
                        if (job.ExpiryDateTime > now)
                        {
                            result = job.Id;
                        }
                        else
                        {
                            ErrorMsg = "Reset job expired.";
                            _logger.LogInformation("Reset job expired.");
                        }
                    }
                    else
                    {
                        ErrorMsg = "Reset job had been completed";
                        _logger.LogInformation("Reset job had been completed");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset job not been completed");
                ErrorMsg = ExceptionTools.GetInnerExceptionMessage(ex);
            }

            return result;
        }
                
        #region rakesh
        public async Task<PasswordSettingViewModel> GetpasswordlengthAsync()
        {
            PasswordSettingViewModel result = new PasswordSettingViewModel();
            try
            {
                _logger.LogInformation("getting password length");
                var res = entity.PasswordSettings.Where(x => x.IsDeleted == false).ToList();
                if (res != null)
                {
                    result.Passwordsetting = new List<PasswordSettingList>();
                    foreach (var item in res)
                    {
                        result.Passwordsetting.Add(new PasswordSettingList
                        {
                            Id = item.Id,
                            MinPwdLength = Convert.ToInt32(item.MinPwdLength),
                            MinUpperCase = Convert.ToInt32(item.MinUpperCase),
                            MinLowerCase = Convert.ToInt32(item.MinLowerCase),
                            MinSpecialCharacter = Convert.ToInt32(item.MinSpecialCharacter),
                            MinNumeric = Convert.ToInt32(item.MinNumeric)
                        });
                    }

                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "not getting password length");
            }

            return await Task.FromResult<PasswordSettingViewModel>(result);
        }

        public async Task<User> GetUserAsync(APIRequest request)
        {
            User? result = null;
            ResetViewModel? result1 = JsonConvert.DeserializeObject<ResetViewModel>(request.Model.ToString());
            try
            {
                _logger.LogInformation("Get user");
                result = entity.Users.Include(x => x.Departments).FirstOrDefault(x => x.Email.ToUpper() == result1.Username.ToUpper() && x.IsDeleted == false);

                if (result != null)
                {
                    if (result.Departments != null && result.Departments.Count() > 0)
                    {
                        var dept = result.Departments.Where(x => x.IsDeleted == false);
                        if (dept != null && dept.Count() > 0)
                        {
                            result.Departments = dept.ToList();
                        }
                        else
                        {
                            result.Departments = null;
                        }
                    }

                    // sort Role List
                    var userRole = entity.Users.Include(x => x.Roles).FirstOrDefault(x => x.Email.ToUpper() == result1.Username.ToUpper() && x.IsDeleted == false);
                    if (userRole != null && userRole.Roles != null && userRole.Roles.Count > 0)
                    {
                        var userRoleList = userRole.Roles.Where(x => x.IsDeleted == false);

                        if (userRoleList != null && userRoleList.Count() > 0)
                        {
                            result.Roles = userRoleList.ToList();
                        }
                    }

                    // sort ModuleList
                    if (result.Roles != null && result.Roles.Count > 0)
                    {
                        List<int> roleIds = result.Roles.Select(x => x.Id).ToList();
                        if (roleIds != null && roleIds.Count > 0)
                        {
                            var newRoles = entity.Roles.Include(x => x.Modules).Where(x => roleIds.Contains(x.Id));
                            if (newRoles != null && newRoles.Count() > 0)
                            {
                                result.ModuleList = new List<Module>();
                                foreach (Role role in newRoles)
                                {
                                    if (role.Modules != null && role.Modules.Count > 0)
                                    {
                                        var tmp = role.Modules.Where(x => x.IsDeleted == false);
                                        if (tmp != null && tmp.Count() > 0)
                                        {
                                            result.ModuleList.AddRange(tmp.ToList());
                                        }
                                    }
                                }
                                if (result.ModuleList.Count == 0)
                                {
                                    result.ModuleList = null;
                                }


                            }
                        }
                    }

                }
                else
                {
                    UsersSessionsTracking ust = new UsersSessionsTracking();
                    ust.UserName = result1.Username;
                    ust.Status = "Login Failed";
                    ust.AttemptedDateTime = DateTime.Now;
                    ust.Remarks = "Invalid UserName";
                    entity.UsersSessionsTrackings.Add(ust);
                    entity.SaveChanges();
                    result = null;
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.InnerException.Message) && ex.InnerException.Message.Contains(" error occurred while establishing a connection to SQL Server"))
                {
                    ErrorMsg = " Error occurred while establishing a connection to SQL Server";

                }
                else
                {
                    ErrorMsg = ex.Message;
                    _logger.LogError(ex, "get user failed");
                }
            }

            return await Task.FromResult<User>(result);
        }

        public async Task<string> ChangePasswordAsync(string hashPassword, APIRequest request)
        {
            string result = null;
            bool epasshistory = false;
            ResetViewModel? result1 = JsonConvert.DeserializeObject<ResetViewModel>(request.Model.ToString());

            try
            {
                _logger.LogInformation("Change password");
                User user = entity.Users.FirstOrDefault(x => x.Email.ToLower() == result1.Username.ToLower() && x.IsDeleted == false);
                if (user != null)
                {
                    if (CryptographyHash.VerifyHashedPassword(user.PasswordHash, result1.NewPassword))
                    {
                        epasshistory = true;
                        //result = "Password already used please try another..!!";
                        result = "Please try another password..!!";
                    }
                    else
                    {
                        var changepswdlist = entity.ChangePasswordAudits.Where(x => x.UserId == user.Id).OrderByDescending(x => x.CreatedDateTime).ToList().Take(3);
                        foreach (var item in changepswdlist)
                        {
                            if (CryptographyHash.VerifyHashedPassword(item.Password, result1.NewPassword))
                            {
                                epasshistory = true;
                                //result = "Password already used please try another..!!";
                                result = "Please try another password..!!";
                            }
                        }

                        if (!epasshistory)
                        {
                            user.PasswordHash = hashPassword;
                            user.UpdateDateTime = DateTime.Now;
                            user.UpdateBy = result1.Username;
                            entity.SaveChanges();

                            ChangePasswordAudit cpa = new ChangePasswordAudit
                            {
                                UserId = user.Id,
                                Password = hashPassword,
                                CreatedDateTime = DateTime.Now,
                                CreatedBy = result1.Username
                            };
                            entity.ChangePasswordAudits.Add(cpa);
                            entity.SaveChanges();

                            //QuestionAnswer qa = entity.QuestionAnswers.FirstOrDefault(x => x.UserId == user.Id);
                            //if (qa != null)
                            //{
                            //    qa.QuestionId = model.QuestionId;
                            //    qa.Answer = model.Answer;
                            //    entity.SaveChanges();
                            //}
                            //else
                            //{
                            //    qa = new QuestionAnswer();
                            //    qa.QuestionId = model.QuestionId;
                            //    qa.Answer = model.Answer;
                            //    qa.UserId = user.Id;

                            //    entity.QuestionAnswers.Add(qa);
                            //    entity.SaveChanges();
                            //}
                        }
                    }
                }
                else
                {
                    result = "User not found.";
                    _logger.LogInformation("User not found");
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.InnerException.Message) && ex.InnerException.Message.Contains(" error occurred while establishing a connection to SQL Server"))
                {
                    ErrorMsg = " Error occurred while establishing a connection to SQL Server";
                }
                else
                {
                    ErrorMsg = ex.Message;
                    _logger.LogError(ex, "failed to change password");
                }
            }


            return await Task.FromResult<string>(result);
        }

        public async Task<int> ValidateUsernameAsync(APIRequest request)
        {
            int result = 0;
            var uname = request.UserName.ToString();
            try
            {
                _logger.LogInformation("validate user name");
                var res = entity.Users.FirstOrDefault(x => x.Email == uname);
                if (res != null)
                {
                    result = 1;
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to validate user name");
                // throw;
            }
            return await Task.FromResult<int>(result);
        }

        public async Task<string> ResetForgotPasswordAsync(string hashPassword, ResetCodeViewModel model)
        {
            string result = null;
            bool epasshistory = false;


            try
            {
                _logger.LogInformation("Reset forgot password");
                User? user = entity.Users.FirstOrDefault(x => x.Email.ToLower() == model.Username.ToLower());
                if (user != null)
                {

                    //new code
                    if (CryptographyHash.VerifyHashedPassword(user.PasswordHash, model.NewPassword))
                    {
                        epasshistory = true;
                        //result = "Password already used please try another..!!";
                        result = "Please try another password..!!";
                    }
                    else
                    {
                        var changepswdlist = entity.ChangePasswordAudits.Where(x => x.UserId == user.Id).OrderByDescending(x => x.CreatedDateTime).ToList().Take(3);
                        foreach (var item in changepswdlist)
                        {
                            if (CryptographyHash.VerifyHashedPassword(item.Password, model.NewPassword))
                            {
                                epasshistory = true;
                                //result = "Password already used please try another..!!";
                                result = "Please try another password..!!";
                            }
                        }

                        if (!epasshistory)
                        {
                            user.PasswordHash = hashPassword;
                            user.UpdateDateTime = DateTime.Now;
                            user.UpdateBy = model.Username;
                            entity.SaveChanges();

                            ChangePasswordAudit cpa = new ChangePasswordAudit
                            {
                                UserId = user.Id,
                                Password = hashPassword,
                                CreatedDateTime = DateTime.Now,
                                CreatedBy = model.Username
                            };
                            entity.ChangePasswordAudits.Add(cpa);
                            entity.SaveChanges();
                        }
                    }

                    //new code end

                    //user.PasswordHash = hashPassword;
                    //user.UpdateDateTime = DateTime.Now;
                    //user.UpdateBy = model.Username;
                    //entity.SaveChanges();
                }
                else
                {
                    result = "User not found.";
                    _logger.LogInformation("User not found");
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.InnerException.Message) && ex.InnerException.Message.Contains(" error occurred while establishing a connection to SQL Server"))
                {
                    ErrorMsg = " Error occurred while establishing a connection to SQL Server";
                }
                else
                {
                    // ErrorMsg = ex.Message;
                    _logger.LogError(ex, "failed to reset forgot password");
                }
            }


            return await Task.FromResult<string>(result);
        }

        public async Task<int> ResetLockOutAsync(string userName)
        {
            int repeatcount = 0;

            try
            {
                _logger.LogInformation("Reset lockout password");
                var pswd = entity.PasswordSettings.FirstOrDefault(x => x.Id == 1 && x.IsDeleted == false);
                var user = entity.Users.FirstOrDefault(x => x.Email.ToLower() == userName.ToLower() && x.IsDeleted == false).Id;
                User? c1 = entity.Users.FirstOrDefault(x => x.Id == user);
                if (c1.AccessFailedCount == pswd.MaxPwdFailedCount)
                {
                    repeatcount = -1;
                }
                else if (c1.AccessFailedCount < pswd.MaxPwdFailedCount)
                {
                    c1.LockoutEndDateUtc = null;
                    c1.LockoutEnabled = false;
                    c1.AccessFailedCount = 0;
                    c1.UpdateDateTime = DateTime.Now;
                    c1.UpdateBy = userName;
                    entity.SaveChanges();
                    repeatcount = 1;

                    UsersSessionsTracking ust = new UsersSessionsTracking();
                    ust.UserName = userName;
                    ust.Status = "Login Successful";
                    ust.LoginDateTime = DateTime.Now;
                    ust.AttemptedDateTime = DateTime.Now;
                    ust.Remarks = "Valid UserName and Password";
                    entity.UsersSessionsTrackings.Add(ust);
                    entity.SaveChanges();


                }
            }
            catch (Exception ex)
            {
                repeatcount = 0;
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "failed reset lockout");
            }

            return await Task.FromResult<int>(repeatcount);
        }

        public async Task<int> SaveUserSessionAsync(string userName)
        {
            int sessionId = 0;

            try
            {
                _logger.LogInformation("Save user session");
                var user = entity.Users.FirstOrDefault(x => x.Email.ToLower() == userName.ToLower() && x.IsDeleted == false).Id;
                UserSessionTracking c1 = new UserSessionTracking();

                c1.UserId = user;
                c1.Status = false;
                c1.LoginTime = DateTime.Now;
                c1.CreatedBy = userName;
                c1.CreatedDateTime = DateTime.Now;
                entity.UserSessionTrackings.Add(c1);
                entity.SaveChanges();
                sessionId = c1.Id;
            }
            catch (Exception ex)
            {
                sessionId = 0;
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "failed save user session");
            }

            return await Task.FromResult<int>(sessionId);
        }

        public async Task<int> GetLockOutCount()
        {
            int repeatcount = 0;

            try
            {
                _logger.LogInformation("Get lockout count");
                var pswd = entity.PasswordSettings.FirstOrDefault(x => x.Id == 1 && x.IsDeleted == false);

                repeatcount = Convert.ToInt32(pswd.MaxPwdFailedCount);
            }
            catch (Exception ex)
            {
                repeatcount = 0;
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "failed get lockout count");
            }

            return await Task.FromResult<int>(repeatcount);
        }

        public async Task<int> CheckLockOutAsync(string userName)
        {
            int repeatcount = 0;

            try
            {
                _logger.LogError("Check lockout");
                var pswd = entity.PasswordSettings.FirstOrDefault(x => x.Id == 1 && x.IsDeleted == false);
                var user = entity.Users.FirstOrDefault(x => x.Email.ToLower() == userName.ToLower() && x.IsDeleted == false).Id;
                User? c1 = entity.Users.FirstOrDefault(x => x.Id == user);
                if (c1.AccessFailedCount == pswd.MaxPwdFailedCount)
                {
                    repeatcount = Convert.ToInt32(pswd.MaxPwdFailedCount);
                }
                else
                {
                    c1.AccessFailedCount = c1.AccessFailedCount + 1;
                    c1.LockoutEndDateUtc = null;
                    c1.LockoutEnabled = true;
                    c1.UpdateDateTime = DateTime.Now;
                    c1.UpdateBy = userName;
                    entity.SaveChanges();
                    repeatcount = c1.AccessFailedCount;
                }
            }
            catch (Exception ex)
            {
                repeatcount = 0;
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "failed to check lockout");
            }

            return await Task.FromResult<int>(repeatcount);
        }

        //  Logout Save Details
        public async Task<int> LogoutSaveAsync(string userName)
        {
            int repeatcount = 0;
            try
            {
                _logger.LogInformation("Save Logout details");
                var pswd = entity.UsersSessionsTrackings.OrderBy(x => x.AttemptedDateTime).LastOrDefault(x => x.UserName == userName && x.LoginDateTime != null && x.LogoutDateTime == null);
                
                if (pswd!=null)
                {
                    pswd.LogoutDateTime= DateTime.Now;
                    entity.SaveChanges();
                }
                repeatcount = 1;
            }
            catch (Exception ex)
            {
                repeatcount = 0;
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "failed save lockout DateTime");
            }

            return await Task.FromResult<int>(repeatcount);
        }

        public async Task<int> WrongPasswordAsync(string userName)
        {
            int repeatcount = 0;
            try
            {
                _logger.LogInformation("Save wrong password details");
                //var pswd = entity.UsersSessionsTrackings.OrderBy(x => x.AttemptedDateTime).LastOrDefault(x => x.UserName == userName && x.LoginDateTime != null && x.LogoutDateTime == null);

                UsersSessionsTracking _ust = new UsersSessionsTracking();
                _ust.UserName = userName;
                _ust.Status = "Login Failed";
                _ust.AttemptedDateTime = DateTime.Now;
                _ust.Remarks = "Invalid Password";
                entity.UsersSessionsTrackings.Add(_ust);
                entity.SaveChanges();
                
                repeatcount = 1;
            }
            catch (Exception ex)
            {
                repeatcount = 0;
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "failed save lockout DateTime");
            }

            return await Task.FromResult<int>(repeatcount);
        }

        #endregion

    }
}

