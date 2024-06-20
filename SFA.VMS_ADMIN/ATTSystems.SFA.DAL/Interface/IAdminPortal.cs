using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.DAL.Interface
{
    public interface IAdminPortal
    {
        #region Santosh Wali
        public string GetErrorMsg();
        public Task<RegistrationViewModel> GetDashboardListAsync(APIRequest req);
        public Task<int> ReactivateVisitorAsync(APIRequest req);
        public Task<RegistrationViewModel> GetNricOrPassportNotExistenceAsync(APIRequest req);
        public Task<int> SaveBatchExcelFiles(APIRequest req);

        public Task<RegistrationViewModel> GetRegistrationListAsync(APIRequest req);

        public Task<RegistrationViewModel> GetGetBlackListAsync(APIRequest req);
        public Task<RegistrationViewModel> GetManualCheckInListAsync(APIRequest req);
        public Task<int> ManualCheckInSaveAsync(APIRequest req);
        public Task<int> blacklist_Trigger(APIRequest req);
        public Task<RegistrationViewModel> GetoverstayerToExcelAsync(APIRequest req);
        public Task<RegistrationViewModel> GetExportBlackListAsync(APIRequest req);

        //Export Registered Visitors
        public Task<RegistrationViewModel> GetRegistrationsToExcelAsync(APIRequest req);
        #endregion

        #region Rakesh
        public Task<List<Location>> GetsingleregLocationAsync(APIRequest request);
        public Task<List<VisitType>> GetSingleVisitorTypeAsync(APIRequest request);
        public Task<List<VisitorIdentity>> GetSinglevstidentityAsync(APIRequest request);
        public Task<int> SingleregsaveAsync(APIRequest req);
        public Task<RegistrationViewModel> EditVstRegAsync(string id);
        public Task<int> UpdateVstRegAsync(APIRequest request);
        public Task<int> DeleteVstAsync(APIRequest request);
        #endregion

        //unitID Method
        public Task<RegistrationViewModel> GetLocationUnitIDSAsync(APIRequest req);
        public Task<int> ValidateUnitIDAsync(APIRequest req);
        public Task<int> UpdateNRICPassportAsync(APIRequest request);

        //ViewAllDetails
        public Task<RegistrationViewModel> ViewAllDetailsAsync(string req);       
    }
}
