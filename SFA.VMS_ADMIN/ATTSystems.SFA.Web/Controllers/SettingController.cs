namespace ATTSystems.SFA.Web.Controllers
{
    using ATTSystems.NetCore.Logger;
    using ATTSystems.NetCore.Model.HttpModel;
    using ATTSystems.NetCore.Model.ViewModel;
    using ATTSystems.NetCore.Utilities;
    using ATTSystems.SFA.Model.ViewModel;
    using ATTSystems.SFA.Web.Helper;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    //using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    [NoDirectAccess]
    public class SettingController : Controller
    {
        private IConfiguration config;
        private readonly ILogger<SettingController> _logger;

        public SettingController(IConfiguration _configuration, ILogger<SettingController> logger)
        {
            config = _configuration;
            _logger = logger;
        }
        #region Users Part

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ListUser()
        {
            _logger.LogInformation("Getting user list");
            APIRequest req = new()
            {
                RequestType = "ListUser",
                RequestString = User?.Identity?.Name ?? string.Empty,
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty,
                Model = string.Empty
            };

            List<UserViewModel>? result = null;
            string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
            var resp = await WebAPIHelper.APIRequestAsync(baseuri, "/Setting/GetUserList/", req);

            if (resp != null && resp.UserList != null)
            {
                result = resp.UserList;
                _logger.LogInformation("Getting user list");
            }
            else
            {
                result = new List<UserViewModel>();
                _logger.LogInformation("not getting user list");
            }
            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> CreateUserModal()
        {
            _logger.LogInformation("Create user modal");
            PasswordSettingViewModel model1 = new();

            APIRequest req = new()
            {
                RequestType = "GetRoleDepartmentList",
                RequestString = User?.Identity?.Name ?? string.Empty,
                UserName = string.Empty,
                Message = string.Empty
            };

            UserViewModel? result = null;
            string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
            var resp = await WebAPIHelper.APIRequestAsync(baseuri, "/Setting/GetRoleDepartmentList/", req);
            var res = await WebAPIHelper.AppRequestAsync("/Auth/Getpasswordlength/", req);
            if (res != null)
            {
                if (res.Passwordsetting != null)
                {
                    if (res.Passwordsetting.Count > 0)
                    {
                        model1.Passwordsetting = res.Passwordsetting;
                        ViewBag.pswdlenght = res.Passwordsetting[0].MinPwdLength;
                        ViewBag.minupper = res.Passwordsetting[0].MinUpperCase;
                        ViewBag.minlower = res.Passwordsetting[0].MinLowerCase;
                        ViewBag.minspch = res.Passwordsetting[0].MinSpecialCharacter;
                        ViewBag.minnum = res.Passwordsetting[0].MinNumeric;
                    }
                }
            }
            if (resp != null)
            {
                if (resp.RoleList != null)
                {
                    if (resp.DepartmentList != null)
                    {
                        result = new UserViewModel();
                        result.DepartmentList = resp.DepartmentList;
                        result.RoleList = resp.RoleList;
                    }
                    else
                    {
                        _logger.LogInformation("Department list not available");
                    }
                }
                else
                {
                    _logger.LogInformation("Role list not available");
                }
            }
            return PartialView("_AddUser", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateUser(UserViewModel model, List<string> multirole, List<string> multidept)
        {
            _logger.LogInformation("Create user");
            dynamic showMessageString = string.Empty;
            string errMsg = string.Empty;
            try
            {
                if (ModelState.IsValid && model != null)
                {
                    if (multirole != null && multirole.Count > 0)
                    {
                        if (model.Password == model.ConfirmPassword)
                        {
                            model.RoleList = new List<RoleViewModel>();
                            foreach (string rId in multirole)
                            {
                                int roleId = 0;
                                if (int.TryParse(rId, out roleId))
                                {
                                    model.RoleList.Add(new RoleViewModel { Id = roleId, });
                                }
                            }
                            APIRequest request = new()
                            {
                                Model = model,
                                UserName = User?.Identity?.Name ?? string.Empty,
                            };

                            string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
                            var result = await Helper.WebAPIHelper.Addasync("/Setting/Passwordvalidation/", request);
                            if (result != null)
                            {
                                if (result.Code == -3)
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
                                    APIResponse? resp = await WebAPIHelper.APIRequestAsync(baseuri, "/Setting/AddUser/", request);
                                    if (resp == null || !resp.Succeeded)
                                    {
                                        string err = resp != null ? resp.Message : string.Empty;
                                        showMessageString = new
                                        {
                                            Code = 300,
                                            Message = "Failed. User is not created.\n" + err,
                                            ModalType = "Add",
                                        };
                                        _logger.LogInformation("Failed. User is not created.\n");
                                    }
                                    else
                                    {
                                        showMessageString = new
                                        {
                                            Code = 200,
                                            Message = string.Format("User is successfully created.", resp.Code),
                                            ModalType = "Add",
                                        };
                                        _logger.LogInformation("User is successfully created.");
                                    }
                                }
                            }
                            else
                            {
                                result = null;
                            }
                        }
                    }
                    else
                    {
                        errMsg = "System did not find any Role being selected.";
                        _logger.LogInformation("System did not find any Role being selected.");
                    }

                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        showMessageString = new
                        {
                            Code = 202,
                            Message = errMsg,
                            ModalType = "Add",
                        };
                    }
                }
                else
                {
                    string errmsg = string.Empty;
                    foreach (string key in ViewData.ModelState.Keys)
                    {
                        string modelStateError = string.Empty;
                        var modelState = ViewData.ModelState[key];
                        if (modelState != null && modelState.Errors != null && modelState.Errors.Count > 0)
                        {
                            errmsg = string.Format("{0}|{1}:{2}", errmsg, key, modelState.Errors.First().ErrorMessage);
                        }
                    }
                    showMessageString = new
                    {
                        Code = 201,
                        Message = !string.IsNullOrEmpty(errmsg) ? errmsg.Substring(1) : errmsg,
                        ModalType = "Add",
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Create user getting error.");
                return Json(showMessageString);
            }
            return Json(showMessageString);
        }

        [HttpGet]
        public async Task<ActionResult> EditUserModal(string id)
        {
            _logger.LogInformation("Edit user modal");
            UserViewModel? result = null;

            APIRequest req = new()
            {
                RequestType = "GetSingleUser",
                RequestString = id,
                UserName = string.Empty,
                Message = string.Empty
            };
            string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
            var resp = await WebAPIHelper.APIRequestAsync(baseuri, "/Setting/GetUser/", req);

            if (resp != null && resp.Succeeded && resp.UserList != null && resp.UserList.Count > 0)
            {
                result = resp.UserList.FirstOrDefault();

                if (result != null)
                {
                    result.ConfirmPassword = "111111";
                    result.Password = "111111";
                }
                else
                {
                    string errmsg = (resp != null && !string.IsNullOrEmpty(resp.Message)) ? resp.Message : "Invalid input";
                    _logger.LogInformation("Invalid input");
                }
            }
            else
            {
                string errmsg = (resp != null && !string.IsNullOrEmpty(resp.Message)) ? resp.Message : "Invalid input";
                _logger.LogInformation("Invalid input");
            }
            return PartialView("_EditUser", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUser(UserViewModel model, List<string> multirole, params string[] selectedRoles)
        {
            _logger.LogInformation("Edit user");
            string? chk = null;
            dynamic showMessageString = string.Empty;
            try
            {
                if (ModelState.IsValid && model != null)
                {
                    if (multirole != null && multirole.Count > 0)
                    {
                        model.RoleList = new List<RoleViewModel>();
                        foreach (string rId in multirole)
                        {
                            int roleId = 0;
                            if (int.TryParse(rId, out roleId))
                            {
                                model.RoleList.Add(new RoleViewModel { Id = roleId, });
                            }
                        }
                        if (selectedRoles.Length > 0)
                        {
                            if (selectedRoles != null)
                            {
                                chk = selectedRoles[0];
                            }
                        }
                        APIRequest request = new()
                        {
                            Model = model,
                            UserName = User?.Identity?.Name ?? string.Empty,
                            RequestString = chk,
                        };
                        string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
                        APIResponse? resp = await WebAPIHelper.APIRequestAsync(baseuri, "/Setting/UpdateUser/", request);
                        if (resp == null || !resp.Succeeded)
                        {
                            string err = resp != null ? resp.Message : string.Empty;
                            showMessageString = new
                            {
                                Code = 300,
                                Message = "Failed. User is not updated.\n" + err,
                                ModalType = "Update",
                            };
                            _logger.LogInformation("Failed. User is not updated.\n");
                        }
                        else
                        {
                            showMessageString = new
                            {
                                Code = 200,
                                Message = string.Format("User is successfully updated."),
                                ModalType = "Update",
                            };
                            _logger.LogInformation("User is successfully updated.");
                        }
                    }
                }
                else
                {
                    string errmsg = string.Empty;
                    foreach (string key in ViewData.ModelState.Keys)
                    {
                        string modelStateError = string.Empty;
                        var modelState = ViewData.ModelState[key];
                        if (modelState!=null && modelState.Errors != null && modelState.Errors.Count > 0)
                        {
                            errmsg = string.Format("{0}|{1}:{2}", errmsg, key, modelState.Errors.First().ErrorMessage);
                        }
                    }

                    showMessageString = new
                    {
                        Code = 201,
                        Message = !string.IsNullOrEmpty(errmsg) ? errmsg.Substring(1) : errmsg,
                        ModalType = "Update",
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User updated is getting error.");
                return Json(showMessageString);
            }
            return Json(showMessageString);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteUser(string rowId)
        {
            _logger.LogInformation("Delete user");
            dynamic showMessageString = string.Empty;
            try
            {
                APIRequest req = new()
                {
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = "Delete User",
                    RequestString = rowId,
                    RequestType = "Delete",
                };
                string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
                var resp = await WebAPIHelper.APIRequestAsync(baseuri, "/Setting/DeleteUser/", req);
                if (resp != null && resp.Succeeded)
                {
                    showMessageString = new
                    {
                        resultCode = 200,
                        Message = string.Format("User is successfully deleted"),
                        ModalType = "Delete",
                    };
                    _logger.LogInformation("User is successfully deleted");
                }
                else
                {
                    showMessageString = new
                    {
                        resultCode = 300,
                        Message = "Failed! User is not updated.",
                        ModalType = "Delete",
                    };
                    _logger.LogInformation("Failed! User is not updated.");
                }
                return Json(showMessageString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User deleted is getting error");
                return Json(showMessageString);
            }
        }

        #endregion


        #region Roles Part

        [HttpGet]
        public async Task<ActionResult> ListRole()
        {
            _logger.LogInformation("getting roles");
            APIRequest req = new()
            {
                RequestType = "ListRole",
                RequestString = User?.Identity?.Name ?? string.Empty,
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty
            };

            List<RoleViewModel>? result = null;
            string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
            var resp = await WebAPIHelper.APIRequestAsync(baseuri, "/Setting/GetRoleList/", req);
            if (resp != null && resp.RoleList != null)
            {
                result = resp.RoleList;
                _logger.LogInformation("getting roles list");
            }
            else
            {
                result = new List<RoleViewModel>();
                _logger.LogInformation("not getting roles");
            }
            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> CreateRoleModal()
        {
            _logger.LogInformation("Create role modal");
            APIRequest req = new()
            {
                RequestType = "GetModuleList",
                RequestString = User?.Identity?.Name ?? string.Empty,
                UserName = string.Empty,
                Message = string.Empty
            };

            RoleViewModel? result = null;

            string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
            var resp = await WebAPIHelper.APIRequestAsync(baseuri, "/Setting/GetModuleList/", req);

            if (resp != null && resp.ModuleList != null)
            {
                result = new RoleViewModel();
                result.ModuleList = resp.ModuleList;
            }
            else
            {
                _logger.LogInformation("Module not available");
            }
            return PartialView("_AddRole", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateRole(RoleViewModel model, params string[] selectedRoles)
        {
            _logger.LogInformation("Create role");
            dynamic showMessageString = string.Empty;
            try
            {
                if (ModelState.IsValid && model != null)
                {
                    if (selectedRoles != null && selectedRoles.Length > 0)
                    {
                        model.ModuleList = new List<ModuleViewModel>();
                        foreach (string item in selectedRoles)
                        {
                            model.ModuleList.Add(new ModuleViewModel { Id = int.Parse(item) });
                        }
                        APIRequest request = new()
                        {
                            Model = model,
                            UserName = User?.Identity?.Name ?? string.Empty,
                        };
                        string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
                        APIResponse? resp = await WebAPIHelper.APIRequestAsync(baseuri, "/Setting/AddRole/", request);
                        if (resp == null || !resp.Succeeded)
                        {
                            string err = resp != null ? resp.Message : string.Empty;
                            showMessageString = new
                            {
                                Code = 300,
                                Message = "Failed. Role is not created.\n" + err,
                                ModalType = "Add",
                            };
                            _logger.LogInformation("Failed. Role is not created.\n");
                        }
                        else
                        {
                            showMessageString = new
                            {
                                Code = 200,
                                Message = string.Format("Role is created successfully .", resp.Code),
                                ModalType = "Add",
                            };
                            _logger.LogInformation("Role is created successfully.");
                        }
                    }
                    else
                    {
                        showMessageString = new
                        {
                            Code = 202,
                            Message = "Please Select Module. ",
                            ModalType = "Add",
                        };
                    }
                }
                else
                {
                    string errmsg = string.Empty;
                    foreach (string key in ViewData.ModelState.Keys)
                    {
                        string modelStateError = string.Empty;
                        var modelState = ViewData.ModelState[key];
                        if (modelState!=null && modelState.Errors != null && modelState.Errors.Count > 0)
                        {
                            errmsg = string.Format("{0}|{1}:{2}", errmsg, key, modelState.Errors.First().ErrorMessage);
                        }
                    }
                    showMessageString = new
                    {
                        Code = 201,
                        Message = !string.IsNullOrEmpty(errmsg) ? errmsg.Substring(1) : errmsg,
                        ModalType = "Add",
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Createrole is getting error");
            }
            return Json(showMessageString);
        }

        [HttpGet]
        public async Task<ActionResult> EditRoleModal(string id)
        {
            _logger.LogInformation("Edit role modal");
            APIRequest req = new()
            {
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = "Get role for editing by id",
                RequestString = id,
                RequestType = "Get single role for editing",
            };
            string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
            var resp = await WebAPIHelper.APIRequestAsync(baseuri, "/Setting/GetRole/", req);

            if (resp != null && resp.Succeeded && resp.RoleList != null && resp.RoleList.Count > 0)
            {
                RoleViewModel? model = resp.RoleList.FirstOrDefault();
                return PartialView("_EditRole", model);
            }
            else
            {
                _logger.LogInformation("invalid input");
                string errmsg = (resp != null && !string.IsNullOrEmpty(resp.Message)) ? resp.Message : "Invalid input";
                return PartialView(errmsg, null);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditRole(RoleViewModel model, params string[] selectedRoles)
        {
            _logger.LogInformation("Edit role");
            dynamic showMessageString = string.Empty;
            try
            {
                if (ModelState.IsValid && model != null)
                {
                    if (selectedRoles != null && selectedRoles.Length > 0)
                    {
                        model.ModuleList = new List<ModuleViewModel>();

                        foreach (string item in selectedRoles)
                        {
                            model.ModuleList.Add(new ModuleViewModel { Id = int.Parse(item) });
                        }

                        APIRequest request = new()
                        {
                            UserName = User?.Identity?.Name ?? string.Empty,
                            Model = model,
                        };
                        string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
                        APIResponse? resp = await WebAPIHelper.APIRequestAsync(baseuri, "/Setting/UpdateRole/", request);
                        if (resp == null || !resp.Succeeded)
                        {
                            string err = resp != null ? resp.Message : string.Empty;
                            showMessageString = new
                            {
                                resultCode = 300,
                                Message = "Failed. Role is not updated.\n" + err,
                                ModalType = "Update",
                            };
                            _logger.LogInformation("Failed. Role is not updated.\n");
                        }
                        else
                        {
                            showMessageString = new
                            {
                                resultCode = 200,
                                Message = string.Format("Role is successfully updated."),
                                ModalType = "Update",
                            };
                            _logger.LogInformation("Role is successfully updated.");
                        }
                    }
                    else
                    {
                        showMessageString = new
                        {
                            resultCode = 202,
                            Message = "System did not found and module selected. ",
                            ModalType = "Update",
                        };
                        _logger.LogInformation("System did not found and module selected.");
                    }
                }
                else
                {
                    string errmsg = string.Empty;
                    foreach (string key in ViewData.ModelState.Keys)
                    {
                        string modelStateError = string.Empty;
                        var modelState = ViewData.ModelState[key];
                        if (modelState!=null && modelState.Errors != null && modelState.Errors.Count > 0)
                        {
                            errmsg = string.Format("{0}|{1}:{2}", errmsg, key, modelState.Errors.First().ErrorMessage);
                        }
                    }
                    showMessageString = new
                    {
                        resultCode = 201,
                        Message = !string.IsNullOrEmpty(errmsg) ? errmsg.Substring(1) : errmsg,
                        ModalType = "Update",
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "edit role is getting error");
                return Json(showMessageString);
            }
            return Json(showMessageString);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteRole(string rowId)
        {
            _logger.LogInformation("Delete role");
            dynamic showMessageString = string.Empty;
            try
            {
                APIRequest req = new()
                {
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = "Delete Role",
                    RequestString = rowId,
                    RequestType = "Delete",
                };
                string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
                var resp = await WebAPIHelper.APIRequestAsync(baseuri, "/Setting/DeleteRole/", req);
                if (resp != null && resp.Succeeded)
                {
                    showMessageString = new
                    {
                        resultCode = 200,
                        Message = string.Format("Role is successfully deleted"),
                        ModalType = "Delete",
                    };
                    _logger.LogInformation("Role is successfully deleted");
                }
                else
                {
                    showMessageString = new
                    {
                        resultCode = 300,
                        Message = "Failed! Role setting is not updated.",
                        ModalType = "Delete",
                    };
                    _logger.LogInformation("Failed! Role setting is not updated.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete role is getting error");
                return Json(showMessageString);
            }
            return Json(showMessageString);
        }
        #endregion

        #region rakesh
        [HttpGet]
        public async Task<ActionResult> ListPasswordSetting()
        {
            _logger.LogInformation("Getting password setting details");
            APIRequest req = new APIRequest
            {
                RequestType = "ListPasswordSetting",
                Message = string.Empty
            };

            List<PasswordSettingViewModel>? result = null;
            var response = await WebAPIHelper.AppRequestAsync("/Setting/PasswordSettingList/", req);

            if (response != null && response.pwdstngView != null)
            {
                result = response.pwdstngView;
                _logger.LogInformation("Getting password setting details");
            }
            else
            {
                result = new List<PasswordSettingViewModel>();
                _logger.LogInformation("not Getting password setting details");
            }
            return View(result);
        }


        [HttpGet]
        public async Task<ActionResult> EditPasswordSettingModal(string id)
        {
            _logger.LogInformation("Edit password setting modal");
            dynamic showMessageString = string.Empty;
            APIRequest req = new()
            {
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = "editing",
                RequestString = id,
                RequestType = "editing",
            };


            var response = await WebAPIHelper.AppRequestAsync("/Setting/GetPasswordSetting/", req);
            if (response != null && response.Succeeded && response.pwdstngView != null && response.pwdstngView.Count > 0)
            {
                PasswordSettingViewModel? model = response.pwdstngView.FirstOrDefault();
                return PartialView("_EditPasswordSetting", model);
            }
            else
            {
                string errmsg = (response != null && !string.IsNullOrEmpty(response.Message)) ? response.Message : "Invalid input";
                _logger.LogInformation("Invalid input");
            }
            return Json(showMessageString);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePswSetting(PasswordSettingViewModel model)
        {
            _logger.LogInformation("Update password setting modal");
            string chk = string.Empty;
            dynamic showMessageString = string.Empty;
            try
            {
                APIRequest request = new()
                {
                    Model = model,
                    UserName = User?.Identity?.Name ?? string.Empty,                    
                };

                var response = await WebAPIHelper.AppRequestAsync("/Setting/UpdatePswSetting/", request);
                if (response == null || !response.Succeeded)
                {
                    string err = response != null ? response.Message : string.Empty;
                    showMessageString = new
                    {
                        Code = 300,
                        Message = "Failed. Password Setting are not updated.",
                        ModalType = "Update",
                    };
                    _logger.LogInformation("Failed. Password Setting are not updated.");
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 200,
                        Message = string.Format("Password Settings are successfully updated."),
                        ModalType = "Update",
                    };
                    _logger.LogInformation("Password Settings are successfully updated.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Password Settings is getting error.");
            }
            return Json(showMessageString);
        }

        #endregion
    }
}