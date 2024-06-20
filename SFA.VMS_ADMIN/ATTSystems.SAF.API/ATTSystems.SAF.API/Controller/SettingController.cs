namespace ATTSystems.SAF.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using ATTSystems.NetCore.Model.HttpModel;
    using ATTSystems.SFA.DAL.Implementations;
    using ATTSystems.NetCore.Model.DBModel;
    using ATTSystems.NetCore.Logger;
    using ATTSystems.NetCore.Model.ViewModel;
    using ATTSystems.SFA.Model.HttpModel;
    using ATTSystems.SFA.DAL.Interface;
    using ATTSystems.SFA.DAL;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using ATTSystems.SFA.Model.DBModel;
    using ATTSystems.SFA.Model.ViewModel;

    [Route("api/Setting")]
    [ApiController]
    public class SettingController : ControllerBase
    {
        //private IConfiguration config;
        private ISetting _setting;
        private IBaseRepository _ibaseReposit;
        private readonly ILogger<SettingController> _logger;
        public SettingController(ISetting setting, IBaseRepository baseRepository, ILogger<SettingController> logger)
        {
            _setting = setting;
            _ibaseReposit = baseRepository;
            _logger = logger;
        }

        [HttpPost]
        [Route("GetModuleList")]
        public async Task<IActionResult> GetModuleList(APIRequest request)
        {
            APIResponse result = new APIResponse();
            // HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Get module list");
            try
            {
                //SettingRepository _repository = new SettingRepository();
                List<Module> moduleList = await _setting.GetModuleListAsync();

                if (moduleList != null && moduleList.Count > 0)
                {
                    result.ModuleList = new List<NetCore.Model.ViewModel.ModuleViewModel>();
                    foreach (Module module in moduleList)
                    {
                        result.ModuleList.Add(new NetCore.Model.ViewModel.ModuleViewModel
                        {
                            Id = module.Id,
                            ModuleId = module.ModuleId,
                            ModuleName = module.ModuleName,
                        });
                    }

                    result.Message = "Success.";

                }
                else
                {
                    result.Message = _setting.GetErrorMsg();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get module list is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: GetModuleList, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }

            //response.Content = new StringContent(JsonConvert.SerializeObject(result), System.Text.Encoding.UTF8, "application/json");
            //return await Task.FromResult<HttpResponseMessage>(response);
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateDepartment")]
        public async Task<IActionResult> UpdateDepartment(APIRequest request)
        {
            APIResponse result = new APIResponse();
            // HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Update department");
            try
            {
               // SettingRepository _repository = new SettingRepository();
                int dbResult = await _setting.UpdateDepartmentAsync(request);
                if (dbResult > 0)
                {
                    result.Code = dbResult;
                    result.Succeeded = true;
                    result.Message = "Department sucessfully updated";
                }
                else
                {
                    result.Code = dbResult;
                    result.Succeeded = false;
                    result.Message = _setting.GetErrorMsg();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Update department is getting error");
               // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: UpdateDepartment, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }

            //response.Content = new StringContent(JsonConvert.SerializeObject(result), System.Text.Encoding.UTF8, "application/json");
            //return await Task.FromResult<HttpResponseMessage>(response);
            return Ok(result);
        }

        [HttpPost]
        [Route("AddDepartment")]
        public async Task<IActionResult> AddDepartment(APIRequest request)
        {
            APIResponse result = new APIResponse();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Add Department");
            try
            {
                //SettingRepository _repository = new SettingRepository();
                int dbResult = await _setting.AddDepartmentAsync(request);

                if (dbResult > 0)
                {
                    result.Code = dbResult;
                    result.Succeeded = true;
                    result.Message = "New Department sucessfully created";
                }
                else
                {
                    result.Code = dbResult;
                    result.Succeeded = false;
                    result.Message = _setting.GetErrorMsg();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Add department is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: AddDepartment, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }

            //response.Content = new StringContent(JsonConvert.SerializeObject(result), System.Text.Encoding.UTF8, "application/json");
            //return await Task.FromResult<HttpResponseMessage>(response);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetDepartmentList")]
        public async Task<IActionResult> GetDepartmentList(APIRequest request)
        {
            APIResponse result = new APIResponse();
            // HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Get Department list");

            try
            {
                //SettingRepository _repository = new SettingRepository();
                List<Department> deptList = await _setting.GetDepartmentListAsync(request.UserName);

                if (deptList != null && deptList.Count > 0)
                {
                    result.DepartmentList = new List<NetCore.Model.ViewModel.DepartmentViewModel>();
                    foreach (Department dept in deptList)
                    {
                        result.DepartmentList.Add(new NetCore.Model.ViewModel.DepartmentViewModel
                        {
                            Department_ID = dept.DepartmentId,
                            Department_Name = dept.DepartmentName,
                            Description = dept.Description
                        });
                    }
                    result.Message = "Success.";

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get department list is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: GetDepartmentList, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
            //response.Content = new StringContent(JsonConvert.SerializeObject(result), System.Text.Encoding.UTF8, "application/json");
            //return await Task.FromResult<HttpResponseMessage>(response);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetDepartment")]
        public async Task<IActionResult> GetDepartment(APIRequest request)
        {
            APIResponse result = new APIResponse();
            // HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Get Department");

            try
            {
                int id = 0;
                if (int.TryParse(request.RequestString, out id))
                {
                    //SettingRepository _repository = new SettingRepository();
                    Department dept = await _setting.GetDepartmentAsync(id);
                    if (dept != null)
                    {
                        DepartmentViewModel model = new DepartmentViewModel
                        {
                            Department_ID = dept.DepartmentId,
                            Department_Name = dept.DepartmentName,
                            Description = dept.Description
                        };
                        result.DepartmentList = new List<DepartmentViewModel>();
                        result.DepartmentList.Add(model);
                        result.Message = "Department retrieval ok.";
                        result.Code = dept.DepartmentId;
                        result.Succeeded = true;
                    }
                    else
                    {
                        result.Message = _setting.GetErrorMsg();
                        if (string.IsNullOrEmpty(result.Message))
                        {
                            result.Message = "System unable retrieve department.";
                        }
                    }
                }
                else
                {
                    result.Message = "Invalid Department Id";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get Department is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: GetDepartment, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }

            //response.Content = new StringContent(JsonConvert.SerializeObject(result), System.Text.Encoding.UTF8, "application/json");
            //return await Task.FromResult<HttpResponseMessage>(response);
            return Ok(result);
        }

        [HttpPost]
        [Route("DeleteDepartment")]
        public async Task<IActionResult> DeleteDepartment(APIRequest request)
        {
            APIResponse result = new APIResponse();
            // HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Delete Department");

            try
            {
               // SettingRepository _repository = new SettingRepository();
                result.Code = await _setting.DeleteDepartmentAsync(request);

                if (result.Code > 0)
                {
                    result.Succeeded = true;
                    result.Message = "Department sucessfully deleted.";
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = _setting.GetErrorMsg();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete department is getting error");
               // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: DeleteDepartment, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }

            //response.Content = new StringContent(JsonConvert.SerializeObject(result), System.Text.Encoding.UTF8, "application/json");
            //return await Task.FromResult<HttpResponseMessage>(response);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetRole")]
        public async Task<IActionResult> GetRole(APIRequest request)
        {
            APIResponse result = new APIResponse();
            // HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Get role list");

            try
            {
                //SettingRepository _repository = new SettingRepository();
                int roleId = 0;
                int.TryParse(request.RequestString, out roleId);

                Role role = await _setting.GetRoleAsync(roleId);

                if (role != null)
                {
                    List<Module> moduleList = await _setting.GetModuleListAsync();

                    if (moduleList != null && moduleList.Count > 0)
                    {
                        RoleViewModel model = new RoleViewModel
                        {
                            Id = role.Id,
                            AllowPermission = role.AllowPermission,
                            Description = role.Description,
                            Name = role.Name,
                            ModuleList = new List<ModuleViewModel>(),
                        };

                        foreach (Module module in moduleList)
                        {
                            Module selectedModule = role.Modules.FirstOrDefault(x => x.Id == module.Id);
                            model.ModuleList.Add(new ModuleViewModel
                            {
                                Id = module.Id,
                                ModuleId = module.ModuleId,
                                ModuleName = module.ModuleName,
                                IsSelected = selectedModule != null ? true : false,
                            });
                        }
                        result.RoleList = new List<RoleViewModel>();
                        result.RoleList.Add(model);

                        result.Message = "Got It";
                        result.Code = 1;
                        result.Succeeded = true;
                    }
                    else
                    {
                        result.Message = "No module found in system.";
                        _logger.LogInformation("No module found in system.");
                    }
                }
                else
                {
                    result.Message = "Role is not found in the system.";
                    _logger.LogInformation("Role is not found in the system.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get role list is getting error");
               // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: GetRole, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }

            //response.Content = new StringContent(JsonConvert.SerializeObject(result), System.Text.Encoding.UTF8, "application/json");

            //return await Task.FromResult<HttpResponseMessage>(response);
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateRole")]
        public async Task<IActionResult> UpdateRole(APIRequest request)
        {
            APIResponse result = new APIResponse();
            // HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Update role");

            try
            {
                //SettingRepository _repository = new SettingRepository();

                string dbResult = await _setting.UpdateRoleAsync(request);
                if (dbResult != "0")
                {
                    result.Code = 1;
                    result.Succeeded = true;
                    result.Message = "New Role successfully updated";
                }
                else
                {
                    result.Code = -1;
                    result.Succeeded = false;
                    result.Message = _setting.GetErrorMsg();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update role is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: UpdateRole, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }

            
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateUser")]
        public async Task<IActionResult> UpdateUser(APIRequest request)
        {
            APIResponse result = new APIResponse();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Update user");
            try
            {
               
                int dbResult = await _setting.UpdateUserAsync(request);

                if (dbResult > 0)
                {
                    result.Code = 1;
                    result.Succeeded = true;
                    result.Message = "New User sucessfully created";
                }
                else
                {
                    result.Code = -1;
                    result.Succeeded = false;
                    result.Message = _setting.GetErrorMsg();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update user is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: UpdateUser, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }

            
            return Ok(result);
        }

        [HttpPost]
        [Route("AddUser")]
        public async Task<IActionResult> AddUser(APIRequest request)
        {
            APIResponse result = new APIResponse();
            // HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Add user");
            try
            {
                //SettingRepository _repository = new SettingRepository();
                int dbResult = await _setting.AddUserAsync(request);

                if (dbResult > 0)
                {
                    result.Code = 1;
                    result.Succeeded = true;
                    result.Message = "New User successfully created";
                }
                else
                {
                    result.Code = -1;
                    result.Succeeded = false;
                    result.Message = _setting.GetErrorMsg();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Add user is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Info, Type: API, Controller: Setting, Action: AddUser, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }

           
            return Ok(result);
        }

        [HttpPost]
        [Route("DeleteUser")]
        public async Task<IActionResult> DeleteUser(APIRequest request)
        {
            APIResponse result = new APIResponse();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Delete user");

            try
            {
                //SettingRepository _repository = new SettingRepository();
                result.Code = await _setting.DeleteUserAsync(request);

                if (result.Code > 0)
                {
                    result.Succeeded = true;
                    result.Message = "User sucessfully deleted.";
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = _setting.GetErrorMsg();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Delete user is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: DeleteUser, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }

            
            return Ok(result);
        }


        [HttpPost]
        [Route("AddRole")]
        public async Task<IActionResult> AddRole(APIRequest request)
        {
            APIResponse result = new APIResponse();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Add role");

            try
            {
               // SettingRepository _repository = new SettingRepository();
                string dbResult = await _setting.AddRoleAsync(request);

                if (dbResult != "0")
                {
                    result.Code = 1;
                    result.Succeeded = true;
                    result.Message = "New Role successfully created";
                }
                else
                {
                    result.Code = -1;
                    result.Succeeded = false;
                    result.Message = _setting.GetErrorMsg();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Add role is getting error");
                // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: AddRole, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }

            
            return Ok(result);
        }

        [HttpPost]
        [Route("DeleteRole")]
        public async Task<IActionResult> DeleteRole(APIRequest request)
        {
            APIResponse result = new APIResponse();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Delete role");

            try
            {
                //SettingRepository _repository = new SettingRepository();
                result.Code = await _setting.DeleteRoleAsync(request);

                if (result.Code > 0)
                {
                    result.Succeeded = true;
                    result.Message = "Role sucessfully deleted.";
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = _setting.GetErrorMsg();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete role is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: DeleteRole, Message: {0} ", ex.Message));
                result.Message = ex.Message;

            }

            
            return Ok(result);
        }

        [HttpPost]
        [Route("GetRoleList")]
        public async Task<IActionResult> GetRoleList(APIRequest request)
        {
            APIResponse result = new APIResponse();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Get role list");

            try
            {
                //SettingRepository _repository = new SettingRepository();
                List<Role> roleList = await _setting.GetRoleListAsync(request.UserName);

                if (roleList != null && roleList.Count > 0)
                {
                    result.RoleList = new List<NetCore.Model.ViewModel.RoleViewModel>();
                    foreach (Role role in roleList)
                    {
                        result.RoleList.Add(new NetCore.Model.ViewModel.RoleViewModel
                        {
                            Id = role.Id,
                            Name = role.Name,
                            Description = role.Description,
                            AllowPermission = role.AllowPermission,
                        });
                    }

                    result.Message = "Success.";
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get role list is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: GetRoleList, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }

            
            return Ok(result);
        }

        [HttpPost]
        [Route("GetUserList")]
        public async Task<IActionResult> GetUserList(APIRequest request)
        {
            APIResponse result = new APIResponse();
            _logger.LogInformation("Get user list");
            try
            {
                //SettingRepository _repository = new SettingRepository();
                List<User> userList = await _setting.GetUserListAsync(request.UserName);
                if (userList != null && userList.Count > 0)
                {
                    result.UserList = new List<UserViewModel>();
                    foreach (User user in userList)
                    {
                        result.UserList.Add(new UserViewModel
                        {
                            Id = user.Id,
                            Email = user.Email,
                            UserName = user.UserName,
                            RoleItems = GetRoleDelimitedString(user.Roles),
                            DepartmentItems = GetDepartmentDelimitedString(user.Departments),
                        });
                    }

                    result.Code = 1;
                    result.Message = "Got It";
                    result.Succeeded = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get user list is getting error ");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: GetUserList, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
           
            return Ok(result);
        }

        [HttpPost]
        [Route("GetRoleDepartmentList")]
        public async Task<IActionResult> GetRoleDepartmentList(APIRequest request)
        {
            APIResponse result = new APIResponse();
            // HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            _logger.LogInformation("Get Role department list");
            try
            {
                
                List<Role> roleList = await _setting.GetActiveRoleListAsync();
                if (roleList != null && roleList.Count > 0)
                {
                    List<Department> deptList = await _setting.GetActiveDepartmentListAsync();

                    if (deptList != null && deptList.Count > 0)
                    {
                        // process role list here
                        // process Department list here

                        result.RoleList = new List<NetCore.Model.ViewModel.RoleViewModel>();
                        foreach (Role role in roleList)
                        {
                            result.RoleList.Add(new NetCore.Model.ViewModel.RoleViewModel
                            {
                                Id = role.Id,
                                Name = role.Name,
                                Description = role.Description,
                                AllowPermission = role.AllowPermission,
                            });
                        }

                        result.DepartmentList = new List<NetCore.Model.ViewModel.DepartmentViewModel>();
                        foreach (Department dept in deptList)
                        {
                            result.DepartmentList.Add(new NetCore.Model.ViewModel.DepartmentViewModel
                            {
                                Department_ID = dept.DepartmentId,
                                Department_Name = dept.DepartmentName,
                                Description = dept.Description
                            });
                        }
                        result.Code = 1;
                        result.Message = "Got It";
                        result.Succeeded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get role department list is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: GetRoleDepartmentList, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
           
            return Ok(result);
        }

        [HttpPost]
        [Route("GetUser")]
        public async Task<IActionResult> GetUser(APIRequest request)
        {
            APIResponse result = new APIResponse();
            _logger.LogInformation("Get user");
            try
            {
               
                int userId = 0;
                int.TryParse(request.RequestString, out userId);
                User user = await _setting.GetUserAsync(userId);
                if (user != null)
                {
                    UserViewModel userModel = new UserViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email
                    };

                    List<Role> roleList = await _setting.GetActiveRoleListAsync();
                    userModel.RoleList = new List<RoleViewModel>();
                    foreach (Role role in roleList)
                    {
                        userModel.RoleList.Add(new NetCore.Model.ViewModel.RoleViewModel
                        {
                            Id = role.Id,
                            Name = role.Name,
                            Description = role.Description,
                            AllowPermission = role.AllowPermission,
                            IsSelected = (user.Roles != null && user.Roles.Count > 0 && user.Roles.FirstOrDefault(x => x.Id == role.Id) != null) ? true : false,
                        });
                    }

                    List<Department> deptList = await _setting.GetActiveDepartmentListAsync();
                    userModel.DepartmentList = new List<DepartmentViewModel>();
                    foreach (Department dept in deptList)
                    {
                        userModel.DepartmentList.Add(new NetCore.Model.ViewModel.DepartmentViewModel
                        {
                            Department_ID = dept.DepartmentId,
                            Department_Name = dept.DepartmentName,
                            Description = dept.Description,
                            IsSelected = (user.Departments != null && user.Departments.Count > 0 && user.Departments.FirstOrDefault(x => x.DepartmentId == dept.DepartmentId) != null) ? true : false,
                        });
                    }

                    result.UserList = new List<UserViewModel>();
                    result.UserList.Add(userModel);
                    result.Code = 1;
                    result.Message = "Got It";
                    result.Succeeded = true;
                }
                else
                {
                    result.Message = "User not found in system.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get user is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: GetUser, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }            
            return Ok(result);
        }

        private string GetRoleDelimitedString(ICollection<Role> roleList)
        {
            string result = string.Empty;
            foreach (Role role in roleList)
            {
                if (role.IsDeleted)
                    continue;

                string roleName = string.Format("[{0}]", role.Name);

                if (!result.Contains(roleName))
                {
                    result = string.Format("{0}, {1}", result, roleName);
                }
            }
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Substring(2);
            }
            return result;
        }

        private string GetDepartmentDelimitedString(ICollection<Department> departList)
        {
            string result = string.Empty;

            foreach (Department dept in departList)
            {
                if (dept.IsDeleted)
                    continue;

                string deptName = string.Format("[{0}]", dept.DepartmentName);

                if (!result.Contains(deptName))
                {
                    result = string.Format("{0}, {1}", result, deptName);
                }
            }
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Substring(2);
            }
            return result;
        }


        #region rakesh
        [HttpPost]
        [Route("PasswordSettingList")]
        public async Task<IActionResult> PasswordSettingList(APIRequest request)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Get password setting list");

            try
            {
                
                List<PasswordSetting> PWDList = await _setting.PasswordSettingListAsync();
                if (PWDList != null && PWDList.Count > 0)
                {
                    result.pwdstngView = new List<PasswordSettingViewModel>();
                    foreach (PasswordSetting pwd in PWDList)
                    {
                        result.pwdstngView.Add(new PasswordSettingViewModel
                        {
                            Id = pwd.Id,
                            MaxPwdLife = Convert.ToInt32(pwd.MaxPwdLife),
                            MaxPwdFailedCount = Convert.ToInt32(pwd.MaxPwdFailedCount),
                            MinPwdLength = Convert.ToInt32(pwd.MinPwdLength),
                            MinLowerCase = Convert.ToInt32(pwd.MinLowerCase),
                            MinUpperCase = Convert.ToInt32(pwd.MinUpperCase),
                            MinNumeric = Convert.ToInt32(pwd.MinNumeric),
                            MinSpecialCharacter = Convert.ToInt32(pwd.MinSpecialCharacter),
                        });
                    }

                    result.Code = 1;
                    result.Message = "Got It";
                    result.Succeeded = true;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Password setting list is getting error");
                // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: PasswordSettingList, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }

            
            return Ok(result);
        }

        [HttpPost]
        [Route("GetPasswordSetting")]
        public async Task<IActionResult> GetPasswordSetting(APIRequest request)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Get password setting");

            try
            {
               
                int pwdid = 0;
                int.TryParse(request.RequestString, out pwdid);

                List<PasswordSetting> pwdsetting = await _setting.GetPasswordSettingAsync(pwdid);
                if (pwdsetting != null)
                {
                    result.pwdstngView = new List<PasswordSettingViewModel>();
                    foreach (PasswordSetting pwd in pwdsetting)
                    {
                        result.pwdstngView.Add(new PasswordSettingViewModel
                        {
                            Id = pwd.Id,
                            MaxPwdLife = Convert.ToInt32(pwd.MaxPwdLife),
                            MaxPwdFailedCount = Convert.ToInt32(pwd.MaxPwdFailedCount),
                            MinPwdLength = Convert.ToInt32(pwd.MinPwdLength),
                            MinLowerCase = Convert.ToInt32(pwd.MinLowerCase),
                            MinUpperCase = Convert.ToInt32(pwd.MinUpperCase),
                            MinNumeric = Convert.ToInt32(pwd.MinNumeric),
                            MinSpecialCharacter = Convert.ToInt32(pwd.MinSpecialCharacter),
                        });
                    }
                    result.Code = 1;
                    result.Message = "Got It";
                    result.Succeeded = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Password setting is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: GetPasswordSetting, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
            
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdatePswSetting")]
        public async Task<IActionResult> UpdatePswSetting(APIRequest request)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Update password setting");
            try
            {
                
                int dbResult = await _setting.UpdatePswSettingAsync(request);

                if (dbResult > 0)
                {
                    result.Code = 1;
                    result.Succeeded = true;
                    result.Message = "Password settings are successfully updated.";
                }
                else
                {
                    result.Code = -1;
                    result.Succeeded = false;
                    result.Message = _setting.GetErrorMsg();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update password setting is getting error");
               // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: UpdatePswSetting, Message: {0} ", ex.Message));
                
            }
            return Ok(result);
        }


        [HttpPost]
        [Route("Passwordvalidation")]
        public async Task<IActionResult> Passwordvalidation(/*UserViewModel model*/ APIRequest request)
        {
            APIResponse result = new APIResponse();
            UserViewModel? result1 = JsonConvert.DeserializeObject<UserViewModel>(request.Model.ToString());
            _logger.LogInformation("Password validation");
            try
            {
               
                int pswdid = 1;
                List<PasswordSetting> pwdsetting = await _setting.GetPasswordSettingAsync(pswdid);
                if (pwdsetting != null)
                {
                    int countedLetters = 0;
                    int upper_count = 0;
                    int lower_count = 0;
                    int number_count = 0;
                    int special_count = 0;
                    if (result1.Password != null)
                    {
                        foreach (char c in result1.Password)
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

                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password validation is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Setting, Action: Passwordvalidation, Message: {0} ", ex.Message));
            }

            return Ok(result);
        }
        #endregion

    }
}
