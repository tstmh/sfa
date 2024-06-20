using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Web.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.DAL.Interface
{
    public interface IOnlinePortal
    {
        #region Nikitha
        public Task<OnlinePortalViewModel> GetExistingPassportNoAsync(APIRequest request);
        public Task<List<VisitType>> GetVisitorTypeAsync(APIRequest request);
        public Task<List<Location>> GetLocationListAsync(APIRequest request);
        public Task<OnlinePortalViewModel> SaveVisitorPassportDetailsAsync(APIRequest request);
        public Task<OnlinePortalViewModel> GetUnitIdListAsync(APIRequest req);
        public Task<int> ValidateUnitIDAsync(APIRequest req);

        public Task<OnlinePortalViewModel> GetNricverifynoAsync(APIRequest request);
        public Task<List<VisitType>> GetNricVisitorTypeAsync(APIRequest request);
        public Task<List<Location>> GetNricLocationAsync(APIRequest request);
        public Task<int> NricdtlsaveinfoAsync(APIRequest request);
        public Task<OnlinePortalViewModel> UpdatePassVisitorReRegistration(APIRequest request);
        public Task<OnlinePortalViewModel> UpdateNRICVisitorReRegistration(APIRequest request);

        public Task<string> UpdateCardNumber(APIRequest request);
        #endregion
    }
}
