using ATTSystems.NetCore.Model.DBModel;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.DAL.Interface
{
    public interface IReport
    {
        public Task<ReportViewModel> GetVisitorAccessListtAsync(APIRequest req);
        public Task<ReportViewModel> GetBlockVisitorAccessListtAsync(APIRequest req);
        public Task<ReportViewModel> GetAudittrailAsync(APIRequest req);
        public Task<ReportViewModel> GetSearchVisitoraccessAsync(APIRequest request);
        public Task<ReportViewModel> GetSearchblkVisitorAsync(APIRequest request);
        public Task<ReportViewModel> GetSearchAudittrailAsync(APIRequest request);
        public Task<List<VisitType>> GetVisitorTypeAsync(APIRequest request);
        public Task<List<Location>> GetLocationAsync(APIRequest request);
        #region graph
        public Task<ReportViewModel> GetSearchlinechartAsync(APIRequest request);
        public Task<ReportViewModel> GetDefaultlinechartAsync(APIRequest request);

        #endregion
        #region User Audit Trial
        public Task<List<User>> GetUserAsync(APIRequest request);
        public Task<ReportViewModel> GetUserVisitorListtAsync(APIRequest request);
        public Task<ReportViewModel> GetSearchVisitorUseraccessAsync(APIRequest request);
        #endregion



        #region User Audit Trial


        public Task<List<User>> LoginUserAsync(APIRequest request);
        public Task<ReportViewModel> LoginUserTrackingAsync(APIRequest req);

        public Task<ReportViewModel> SearchLoginUserTrackingAsync(APIRequest req);

        #endregion

    }
}
