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
    public interface IKiosk
    {
        public Task<KioskViewModel> GetExistingPassportNoAsync(APIRequest request);
        public Task<List<VisitType>> GetVisitorTypeAsync(APIRequest request);
        public Task<KioskViewModel> SaveKioskVisitorDetailsAsync(APIRequest request);
        public Task<KioskViewModel> GetUnitIdListAsync(APIRequest req);
        public Task<int> ValidateUnitIDAsync(APIRequest req);

        public Task<KioskViewModel> UpdateVisitorRegLocationMapping(APIRequest request);

        public Task<string> UpdateCardNumber(APIRequest request);
    }
}
