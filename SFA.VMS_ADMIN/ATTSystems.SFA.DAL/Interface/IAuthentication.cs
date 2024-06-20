namespace ATTSystems.SFA.DAL.Interface
{
    using ATTSystems.NetCore.Model.DBModel;
    using ATTSystems.NetCore.Model.HttpModel;
    using ATTSystems.SFA.Model.ViewModel;

    public interface IAuthentication
    {

        public string GetErrorMsg();
        public User? GetUser(string userName);
        public List<Module> GetUserModuleByRole(ICollection<Role> roleList);
        public bool ResetSecureCode(int rowId, string request);
        public bool RequestAccountReset(APIRequest request);
        public List<Setting> GetSettingByType(string type);
        public int GetUserByRowId(string key);
        //public string ChangePasswordAsync(string userName, string hashPassword);
        #region rakesh 
        public Task<PasswordSettingViewModel> GetpasswordlengthAsync();
        public Task<User> GetUserAsync(APIRequest request);
        public Task<string> ChangePasswordAsync(string hashPassword, APIRequest request);
        public Task<int> ValidateUsernameAsync(APIRequest request);
        public Task<string> ResetForgotPasswordAsync(string hashPassword, ResetCodeViewModel model);
        public Task<int> ResetLockOutAsync(string userName);

        public Task<int> SaveUserSessionAsync(string userName);
        public Task<int> GetLockOutCount();

        public Task<int> CheckLockOutAsync(string userName);
        public Task<int> LogoutSaveAsync(string userName);
        public Task<int> WrongPasswordAsync(string userName);
        #endregion
    }
}
