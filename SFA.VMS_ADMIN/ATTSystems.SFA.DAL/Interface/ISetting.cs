namespace ATTSystems.SFA.DAL.Interface
{
    using ATTSystems.NetCore.Model.DBModel;
    using ATTSystems.NetCore.Model.HttpModel;
    using ATTSystems.SFA.Model.DBModel;

    public interface ISetting
    {
        public string GetErrorMsg();
        public Task<User?> GetUserAsync(int? id);

        public Task<string> UpdateRoleAsync(APIRequest request);

        public Task<int> DeleteUserAsync(APIRequest request);

        public Task<int> UpdateUserAsync(APIRequest request);

        public Task<int> AddUserAsync(APIRequest request);

        public Task<string> AddRoleAsync(APIRequest request);

        public Task<List<Module>> GetModuleListAsync();

        public Task<Role> GetRoleAsync(int id);

        public Task<List<Role>> GetRoleListAsync(string userName);

        public Task<List<Department>> GetDepartmentListAsync(string userName);

        public Task<Department> GetDepartmentAsync(int id);

        public Task<int> UpdateDepartmentAsync(APIRequest request);

        public Task<int> AddDepartmentAsync(APIRequest request);

        public Task<int> DeleteRoleAsync(APIRequest request);

        public Task<int> DeleteDepartmentAsync(APIRequest request);

        public Task<List<User>> GetUserListAsync(string userName);

        public Task<List<Department>> GetActiveDepartmentListAsync();

        public Task<List<Role>> GetActiveRoleListAsync();
        #region Rakesh
        public Task<List<PasswordSetting>> PasswordSettingListAsync();
        public Task<List<PasswordSetting>> GetPasswordSettingAsync(int id);
        public Task<int> UpdatePswSettingAsync(APIRequest request);

        #endregion


    }
}
