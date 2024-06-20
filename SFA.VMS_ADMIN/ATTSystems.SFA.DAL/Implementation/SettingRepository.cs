namespace ATTSystems.SFA.DAL.Implementations
{
    using ATTSystems.NetCore;
    using ATTSystems.NetCore.Model.DBModel;
    using ATTSystems.NetCore.Model.HttpModel;
    using ATTSystems.NetCore.Model.ViewModel;
    using ATTSystems.SFA.DAL.Interface;
    using ATTSystems.SFA.Model.DBModel;
    using ATTSystems.SFA.Model.ViewModel;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    public class SettingRepository : IBaseRepository, IDisposable, ISetting
    {
        #region Standard Practice to create a Data Repo

        private string ErrorMsg = string.Empty;
        private DataContext entity;
        private readonly ILogger<SettingRepository> _logger;
        public SettingRepository(ILogger<SettingRepository> logger)
        {
            entity = new DataContext();
            _logger = logger;
        }

        ~SettingRepository()
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

        public async Task<User?> GetUserAsync(int? id)
        {
            ErrorMsg = string.Empty;
            User? user = new User();

          
            try
            {
                _logger.LogInformation("Get user");
                user = entity.Users.Include(x => x.Departments).Include(x => x.Roles).FirstOrDefault(x => x.Id == id && x.IsDeleted == false);
                if (user != null)
                {
                    // clean up the deleted roles
                    if (user.Roles != null && user.Roles.Count > 0)
                    {
                        var roleList = user.Roles.Where(x => x.IsDeleted == false);
                        if (roleList != null)
                        {
                            user.Roles = roleList.ToList();
                        }
                    }

                    // Clean up the deleted Department
                    if (user.Departments != null)
                    {
                        var deptList = user.Departments.Where(x => x.IsDeleted == false);
                        if (deptList != null)
                        {
                            user.Departments = deptList.ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                _logger.LogError(ex, "Failed to get user");
            }

            

            return await Task.FromResult<User?>(user);
        }

        public async Task<string> UpdateRoleAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            string roleId = "0";

            RoleViewModel model = JsonConvert.DeserializeObject<RoleViewModel>(request.Model.ToString());

            //using (var entity = new DBContext())
            //{
            try
            {
                Role role = entity.Roles.FirstOrDefault(x => x.Id != model.Id && x.Name == model.Name && x.IsDeleted == false);

                if (role == null)
                {
                    role = entity.Roles.Include(x => x.Modules).FirstOrDefault(x => x.Id == model.Id);

                    if (role != null)
                    {
                        var moduleIdList = model.ModuleList.Select(x => x.Id);
                        var moduleList = entity.Modules.Where(x => moduleIdList.Contains(x.Id));
                        if (moduleList != null && moduleList.Count() > 0)
                        {
                            //var mlist = role.Modules;
                            //foreach (var item in mlist)
                            //{
                            //    role.Modules.Remove(item);
                            //}
                            string allowableModule = string.Empty;
                            role.Modules.Clear();
                            foreach (Module module in moduleList)
                            {
                                role.Modules.Add(module);
                                allowableModule = string.Format("{0}, {1}", allowableModule, module.ModuleId);
                            }

                            role.AllowPermission = string.IsNullOrEmpty(allowableModule) ? string.Empty : allowableModule.Substring(2);
                            role.Name = model.Name;
                            role.Description = model.Description;
                            role.UpdateBy = request.UserName;
                            role.UpdateDateTime = DateTime.Now;

                            entity.SaveChanges();
                            roleId = role.Id.ToString();
                        }
                        else
                        {
                            ErrorMsg = "Module not found in the system.";
                        }
                    }
                    else
                    {
                        ErrorMsg = "Role not found in the system.";
                    }
                }
                else
                {
                    ErrorMsg = "Same Role Name found Role in the system.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update role");
            }
            return await Task.FromResult<string>(roleId);
        }

        public async Task<int> DeleteUserAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            int result = 0;


            try
            {
                _logger.LogInformation("Delete user");
                string[] idArr = request.RequestString.Split(',');
                foreach (string id in idArr)
                {
                    int userId = 0;
                    int.TryParse(id, out userId);
                    User user = entity.Users.FirstOrDefault(x => x.Id == userId);
                    if (user != null)
                    {
                        UsersAudit ua = new UsersAudit();
                        ua.UserName = user.UserName;
                        ua.UserNameAudit = user.UserName;
                        ua.PasswordHash = user.PasswordHash;
                        ua.Email = user.Email;
                        ua.EmailAudit = user.Email;
                        ua.UpdateBy = request.UserName;
                        ua.UpdateDateTime = DateTime.Now;
                        ua.Remarks = "User Deleted";
                        entity.UsersAudits.Add(ua);

                        user.IsDeleted = true;
                        user.UpdateBy = request.UserName;
                        user.UpdateDateTime = DateTime.Now;
                        entity.SaveChanges();
                        result += 1;
                    }
                    else
                    {
                        result = -1;
                        ErrorMsg = "User not exist in the system.";
                        _logger.LogInformation("User not exist in the system.");
                    }
                }
            }
            catch (Exception ex)
            {
                // write to log
                ErrorMsg = ex.Message;
                ErrorMsg = "System internal error.";
                _logger.LogError(ex, "Failed to delete user");
                if (result > 0)
                {
                    ErrorMsg = string.Format("{0} Partial deleted [{1}]", ErrorMsg, result);
                }
                else
                {
                    result = -1;
                }
            }
            //}

            return await Task.FromResult<int>(result);
        }

        public async Task<int> UpdateUserAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            int roleId = 0;
            //new
            //ChangePasswordAudit cpa = new ChangePasswordAudit();


            //new
            UserViewModel? model = JsonConvert.DeserializeObject<UserViewModel>(request.Model.ToString());
            //using (var entity = new DBContext())
            //{
            try
            {
                if (model != null)
                {
                    string remarks = string.Empty;
                    _logger.LogInformation("Update user");
                    User? user = entity.Users.FirstOrDefault(x => x.UserName.ToLower() == model.UserName.ToLower() && x.Id != model.Id && x.IsDeleted == false);

                    if (user == null) // no duplicate User name
                    {
                        user = entity.Users.Include(x => x.Roles).Include(x => x.Departments).FirstOrDefault(x => x.Id == model.Id && x.IsDeleted == false);
                        if (user != null)
                        {
                            UsersAudit ua = new UsersAudit();
                            if (request.RequestString != null)
                            {
                                user.PasswordHash = CryptographyHash.HashPassword("User@1234567");
                                ua.PasswordHash = CryptographyHash.HashPassword("User@1234567");
                                remarks = "Password updated,";
                            }
                            if (user.UserName != model.UserName)
                            {
                                remarks = "UserName updated,";
                            }
                            if (user.Email != model.Email)
                            {
                                remarks += "Email updated,";
                            }
                            user.UserName = model.UserName;
                            user.Email = model.Email;
                            user.UpdateBy = request.UserName;
                            user.UpdateDateTime = DateTime.Now;
                            user.LockoutEnabled = false;
                            user.AccessFailedCount = 0;
                            //new
                           
                            ua.UserName = model.UserName;
                            ua.Email = model.Email;
                            ua.UpdateBy = request.UserName;
                            ua.UpdateDateTime = DateTime.Now;
                            ua.LockoutEnabled = false;
                            ua.AccessFailedCount = 0;
                            ua.PasswordHash=user.PasswordHash;
                            ua.UserNameAudit = model.UserName;
                            ua.EmailAudit = model.Email;
                            ua.UpdateBy = request.UserName;
                            ua.UpdateDateTime = DateTime.Now;
                            ua.Remarks = remarks;
                            entity.UsersAudits.Add(ua);
                            //cpa.UserId = user.Id;
                            //cpa.Password = user.PasswordHash;
                            //cpa.CreatedDateTime = DateTime.Now;
                            //cpa.CreatedBy = model.UserName;

                            //entity.ChangePasswordAudits.Add(cpa);
                            //entity.SaveChanges();

                            //new

                            var roleIdList = model.RoleList.Select(x => x.Id);
                            var roleList = entity.Roles.Where(x => roleIdList.Contains(x.Id));
                            if (roleList != null)
                            {
                                user.Roles.Clear();
                                foreach (Role role in roleList)
                                {
                                    user.Roles.Add(role);
                                }
                            }

                            //var deptIdList = model.DepartmentList.Select(x => x.Department_ID);
                            //var deptList = entity.Departments.Where(x => deptIdList.Contains(x.DepartmentId));
                            //if (deptList != null)
                            //{
                            //    user.Departments.Clear();
                            //    foreach (Department dept in deptList)
                            //    {
                            //        user.Departments.Add(dept);
                            //    }
                            //}

                            entity.SaveChanges();
                            roleId = 1;
                        }
                        else
                        {
                            roleId = -1;
                            ErrorMsg = "User not exist in the system.";
                            _logger.LogInformation("User not exist in the system.");
                        }
                    }
                    else
                    {
                        roleId = -2;
                        ErrorMsg = "Same User name exist in the system.";
                        _logger.LogInformation("Same User name exist in the system.");
                    }
                }
            }
            catch (Exception ex)
            {
                roleId = 0;
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "Failed to update user");
                // write to log
            }
            //}

            return await Task.FromResult<int>(roleId);
        }

        public async Task<int> AddUserAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            int roleId = 0;

            UserViewModel? model = JsonConvert.DeserializeObject<UserViewModel>(request.Model.ToString());
            ChangePasswordAudit cpa = new ChangePasswordAudit();
            //using (var entity = new DBContext())
            //{
            try
            {
                _logger.LogInformation("Add user");
                User? user = entity.Users.FirstOrDefault(x => /*x.Email.ToLower() == model.Email.ToLower() && */x.UserName.ToLower() == model.UserName.ToLower() && x.IsDeleted == false);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        EmailConfirmed = true,
                        PasswordHash = CryptographyHash.HashPassword(model.Password),
                        SecurityStamp = string.Empty,
                        PhoneNumber = "0",
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEndDateUtc = null,
                        LockoutEnabled = false,
                        AccessFailedCount = 0,
                        IsDeleted = false,
                        CreateBy = request.UserName,
                        CreateDateTime = DateTime.Now,
                        UpdateBy = string.Empty,
                        UpdateDateTime = null,
                    };

                    var roleIdList = model.RoleList.Select(x => x.Id);
                    var roleList = entity.Roles.Where(x => roleIdList.Contains(x.Id));
                    if (roleList != null)
                    {
                        foreach (Role role in roleList)
                        {
                            user.Roles.Add(role);
                        }
                    }

                    //var deptIdList = model.DepartmentList.Select(x => x.Department_ID);
                    //var deptList = entity.Departments.Where(x => deptIdList.Contains(x.DepartmentId));
                    //if (deptList != null)
                    //{
                    //    foreach (Department dept in deptList)
                    //    {
                    //        user.Departments.Add(dept);
                    //    }
                    //}
                    entity.Users.Add(user);
                    entity.SaveChanges();
                    roleId = 1;
                    //new

                    cpa.UserId = user.Id;
                    cpa.Password = user.PasswordHash;
                    cpa.CreatedDateTime = DateTime.Now;
                    cpa.CreatedBy = model.UserName;

                    entity.ChangePasswordAudits.Add(cpa);
                    entity.SaveChanges();

                    //new
                }
                else
                {
                    roleId = -1;
                    ErrorMsg = "User email exist in the system.";
                    _logger.LogInformation("User email exist in the system.");
                }
            }
            catch (Exception ex)
            {
                roleId = 0;
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "failed to add user");
                // write to log
            }
            //}

            return await Task.FromResult<int>(roleId);
        }

        public async Task<string> AddRoleAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            string roleId = "0";

            RoleViewModel? model = JsonConvert.DeserializeObject<RoleViewModel>(request.Model.ToString());

            try
            {
                _logger.LogInformation("Add Role");
                Role role = entity.Roles.FirstOrDefault(x => x.Name == model.Name && x.IsDeleted == false);

                if (role == null)
                {

                    var moduleIdList = model.ModuleList.Select(x => x.Id);
                    var moduleList = entity.Modules.Where(x => moduleIdList.Contains(x.Id));

                    if (moduleList != null && moduleList.Count() > 0)
                    {
                        role = new Role
                        {
                            Name = model.Name,
                            CreateBy = request.UserName,
                            CreateDateTime = DateTime.Now,
                            Description = model.Description,
                            IsDeleted = false,
                            UpdateBy = string.Empty,
                            UpdateDateTime = null,
                        };

                        string allowableModule = string.Empty;
                        foreach (Module module in moduleList)
                        {
                            role.Modules.Add(module);
                            allowableModule = string.Format("{0}, {1}", allowableModule, module.ModuleId);
                        }

                        role.AllowPermission = string.IsNullOrEmpty(allowableModule) ? string.Empty : allowableModule.Substring(2);

                        User? user = entity.Users.FirstOrDefault(x => x.UserName.ToLower() == request.UserName.ToLower());
                        if (user != null)
                        {
                            role.Users.Add(user);
                        }

                        if (request.UserName.ToLower() != "administrator") // create a new record for administrator
                        {
                            User? adminUser = entity.Users.FirstOrDefault(x => x.UserName.ToLower() == request.UserName.ToLower());
                            if (adminUser != null)
                            {
                                role.Users.Add(adminUser);
                            }
                        }
                        entity.Roles.Add(role);
                        entity.SaveChanges();
                        roleId = "1";
                    }
                    else
                    {
                        ErrorMsg = "No Module found in the system.";
                        _logger.LogInformation("No Module found in the system.");
                    }
                }
                else
                {
                    ErrorMsg = "Role name exist in the system.";
                    _logger.LogInformation("Role name exist in the system.");
                }
            }
            catch (Exception ex)
            {
                roleId = "0";
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "Failed to add role");
            }
            return await Task.FromResult<string>(roleId);
        }

        public async Task<List<Module>> GetModuleListAsync()
        {
            List<Module>? result = null;
            _logger.LogInformation("Get module list");
            try
            {
                var modules = entity.Modules.Where(x => x.IsDeleted == false);
                result = modules != null ? modules.ToList() : null;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                _logger.LogError(ex, "Failed to get module list");
            }
            return await Task.FromResult<List<Module>>(result);
        }

        public async Task<Role> GetRoleAsync(int id)
        {
            Role? result = null;
            _logger.LogInformation("Get role");
            try
            {
                result = entity.Roles.Include(x => x.Modules).FirstOrDefault(x => x.Id == id);
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                _logger.LogError(ex, "Failed to get role");
            }
            return await Task.FromResult<Role>(result);
        }

        public async Task<List<Role>> GetRoleListAsync(string userName)
        {
            List<Role>? result = null;
            _logger.LogInformation("Get role list");
            try
            {
                List<Role> user = entity.Roles.Where(x => x.IsDeleted == false).ToList();
                if (user != null)
                {
                    var roleList = user;
                    result = roleList != null ? roleList.ToList() : null;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                _logger.LogError(ex, "Failed to get role list");
            }
            return await Task.FromResult<List<Role>>(result);
        }

        public async Task<List<Department>> GetDepartmentListAsync(string userName)
        {
            List<Department> result = null;
            try
            {
                _logger.LogInformation("Get Department");
                var deptList = entity.Departments.Where(x => x.IsDeleted == false);
                result = deptList != null ? deptList.ToList() : null;

            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                _logger.LogError(ex, "Failed to get department");
            }


            return await Task.FromResult<List<Department>>(result);

        }

        public async Task<Department> GetDepartmentAsync(int id)
        {
            Department? result = null;

            result = entity.Departments.FirstOrDefault(x => x.DepartmentId == id);


            return await Task.FromResult<Department>(result);
        }

        public async Task<int> UpdateDepartmentAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            int deptId = 0;

            DepartmentViewModel? model = JsonConvert.DeserializeObject<DepartmentViewModel>(request.Model.ToString());

            _logger.LogInformation("Update department");
            try
            {
                Department? dept = entity.Departments.FirstOrDefault(x => x.DepartmentId != model.Department_ID && x.DepartmentName == model.Department_Name);
                if (dept == null)
                {
                    dept = entity.Departments.FirstOrDefault(x => x.DepartmentId == model.Department_ID);

                    if (dept != null)
                    {
                        dept.DepartmentName = model.Department_Name;
                        dept.Description = model.Description;
                        dept.UpdateDateTime = DateTime.Now;
                        dept.UpdateBy = request.UserName;
                        entity.SaveChanges();
                        deptId = dept.DepartmentId;
                    }
                    else
                    {
                        ErrorMsg = "Department not found in system.";
                    }
                }
                else
                {
                    ErrorMsg = "Same department name found in system.";
                }

            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                //write to log
                deptId = -1;
                ErrorMsg = "System internal error.";
                _logger.LogError(ex, "Failed update department");
            }
            //}

            return await Task.FromResult<int>(deptId);
        }

        public async Task<int> AddDepartmentAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            int deptId = 0;


            DepartmentViewModel? model = JsonConvert.DeserializeObject<DepartmentViewModel>(request.Model.ToString());
            _logger.LogInformation("ADD department");
            try
            {
                Department? dept = entity.Departments.FirstOrDefault(x => x.DepartmentName.ToLower() == model.Department_Name.ToLower() && x.IsDeleted == false);
                if (dept == null)
                {
                    User? user = entity.Users.FirstOrDefault(x => x.UserName.ToLower() == request.UserName.ToLower());
                    if (user != null)
                    {
                        deptId = entity.Departments.Any() ? entity.Departments.Select(x => x.DepartmentId).Max() + 1 : 1;
                        dept = new Department
                        {
                            DepartmentId = deptId,
                            Description = model.Description,
                            DepartmentName = model.Department_Name,
                            IsDeleted = false,
                            //UpdateDateTime = null,
                            CreateDateTime = DateTime.Now,
                            CreateBy = string.IsNullOrEmpty(request.UserName) ? "" : request.UserName,
                        };
                        //dept.Users.Add(user);
                        //if (request.UserName.ToLower() != "administrator") // create a new record for administrator
                        //{
                        //    User adminUser = entity.Users.FirstOrDefault(x => x.UserName.ToLower() == request.UserName.ToLower());
                        //    if (adminUser != null)
                        //    {
                        //        dept.Users.Add(adminUser);
                        //    }
                        //}
                        entity.Departments.Add(dept);
                        entity.SaveChanges();
                    }
                    else
                    {
                        ErrorMsg = string.Format("System did not find user with username = {0}", request.UserName);
                    }
                }
                else
                {
                    ErrorMsg = "System found similar department name.";
                    _logger.LogInformation("System found similar department name.");
                }
            }
            catch (Exception ex)
            {
                deptId = -1;
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "Failed to add department.");
            }
            //}
            return await Task.FromResult<int>(deptId);
        }

        public async Task<int> DeleteRoleAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            int result = 0;

            _logger.LogInformation("Delete role");
            try
            {
                string[] idArr = request.RequestString.Split(',');

                foreach (string id in idArr)
                {
                    int roleId = 0;
                    int.TryParse(id, out roleId);
                    Role? role = entity.Roles.FirstOrDefault(x => x.Id == roleId);
                    if (role != null)
                    {
                        role.IsDeleted = true;
                        role.UpdateDateTime = DateTime.Now;
                        role.UpdateBy = request.UserName;
                        entity.SaveChanges();
                        result += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                _logger.LogError(ex, "Failed to delete role");
                ErrorMsg = "System internal error.";

                if (result > 0)
                {
                    ErrorMsg = string.Format("{0} Partial deleted [{1}]", ErrorMsg, result);
                }
                else
                {
                    result = -1;
                }
            }

            return await Task.FromResult<int>(result);
            //}
        }

        public async Task<int> DeleteDepartmentAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            int result = 0;

            _logger.LogInformation("Delete Department");
            try
            {
                string[] idArr = request.RequestString.Split(',');

                foreach (string id in idArr)
                {
                    int deptId = 0;

                    if (int.TryParse(id, out deptId) && deptId > 0)
                    {
                        Department? dept = entity.Departments.FirstOrDefault(x => x.DepartmentId == deptId);
                        if (dept != null)
                        {
                            dept.IsDeleted = true;
                            dept.UpdateDateTime = DateTime.Now;
                            dept.UpdateBy = request.UserName;
                            entity.SaveChanges();
                            result += 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                _logger.LogError(ex, "Failed to department");
                ErrorMsg = "System internal error.";

                if (result > 0)
                {
                    ErrorMsg = string.Format("{0} Partial deleted [{1}]", ErrorMsg, result);
                }
                else
                {
                    result = -1;
                }
            }

            return await Task.FromResult<int>(result);
            //}
        }

        public async Task<List<User>> GetUserListAsync(string userName)
        {
            List<User> result = null;
            _logger.LogInformation("Get user list");
            try
            {
                var list = entity.Users.Include(x => x.Departments).Include(x => x.Roles).Where(x => x.IsDeleted == false);
                if (list != null)
                {
                    result = list.ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                _logger.LogError(ex, "Failed to get user list");
            }
            //}

            return await Task.FromResult<List<User>>(result);
        }

        public async Task<List<Department>> GetActiveDepartmentListAsync()
        {
            List<Department> result = null;

            _logger.LogInformation("Get active department list");
            try
            {
                var list = entity.Departments.Where(x => x.IsDeleted == false);
                if (list != null)
                {
                    result = list.ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                _logger.LogError(ex, "Gailed to get active department list");
            }
            //}

            return await Task.FromResult<List<Department>>(result);
        }

        public async Task<List<Role>> GetActiveRoleListAsync()
        {
            List<Role> result = null;
            _logger.LogInformation("Get Active role list");
            try
            {
                var roleList = entity.Roles.Where(x => x.IsDeleted == false);
                if (roleList != null)
                {
                    result = roleList.ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                _logger.LogError(ex, "Failed to Get Active role list");
            }
            //}

            return await Task.FromResult<List<Role>>(result);
        }

        #region rakesh
        public async Task<List<PasswordSetting>> PasswordSettingListAsync()
        {
            List<PasswordSetting> result = null;

            try
            {
                _logger.LogInformation("Get Password setting list");
                var list = entity.PasswordSettings.Where(x => x.IsDeleted == false);
                if (list != null)
                {
                    result = list.ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                _logger.LogError(ex, "Failed get password setting list");
            }

            return await Task.FromResult<List<PasswordSetting>>(result);
        }

        public async Task<List<PasswordSetting>> GetPasswordSettingAsync(int id)
        {
            List<PasswordSetting> result = null;

            try
            {
                _logger.LogInformation("Get password setting edit");
                var list = entity.PasswordSettings.Where(x => x.Id == id);
                if (list != null)
                {
                    result = list.ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                _logger.LogError(ex, "Failed edit password setting list");
            }

            return await Task.FromResult<List<PasswordSetting>>(result);
        }

        public async Task<int> UpdatePswSettingAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            int passwrdId = 0;
            PasswordSettingViewModel model = JsonConvert.DeserializeObject<PasswordSettingViewModel>(request.Model.ToString());

            try
            {
                _logger.LogInformation("Update password setting");
                PasswordSetting pwd = entity.PasswordSettings.FirstOrDefault(x => x.Id == model.Id && x.IsDeleted == false);
                if (pwd != null)
                {
                    pwd.MaxPwdLife = model.MaxPwdLife;
                    pwd.MaxPwdFailedCount = model.MaxPwdFailedCount;
                    pwd.MinPwdLength = model.MinPwdLength;
                    pwd.MinLowerCase = model.MinLowerCase;
                    pwd.MinUpperCase = model.MinUpperCase;
                    pwd.MinNumeric = model.MinNumeric;
                    pwd.MinSpecialCharacter = model.MinSpecialCharacter;
                    pwd.ModifiedDate = DateTime.Now;
                    pwd.UpdatedBy = request.UserName;
                    entity.SaveChanges();
                    passwrdId = 1;
                }
                else
                {
                    passwrdId = -1;
                    ErrorMsg = "Password setting not exists in the system.";
                }
            }
            catch (Exception ex)
            {
                passwrdId = 0;
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "Failed to update password setting");
            }

            return await Task.FromResult<int>(passwrdId);
        }
        #endregion

    }
}
