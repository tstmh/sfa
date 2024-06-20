using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.Model.DBModel;

//using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.DAL.Interface
{
    public interface IMobileDevice
    {
        public Task<MobileDeviceViewModel> AsyncRegisternric(APIRequest request);
        public Task<int> AsyncSavenricRegisterdtls(APIRequest request);

        public Task<MobileDeviceViewModel> AsyncRegisterPassport(APIRequest request);
        public Task<int> AsyncSavePassportRegisterdtls(APIRequest request);

        //Load

        public Task<List<VisitType>> GetNricVisitorTypeAsync(APIRequest request);
        public Task<List<VisitType>> AsyncVisitorTypePassport(APIRequest request);

        //Load UNIT DETAILS NRIC
        public Task<MobileDeviceViewModel> GetLocationUnitIDSAsync(APIRequest req);

        public Task<int> MobileValidateUnitIDAsync(APIRequest req);

        public Task<string> MobileUpdateCardNumber(APIRequest request);

        public Task<MobileDeviceViewModel> AsyncExitsSaveNric(APIRequest request);
        public Task<MobileDeviceViewModel> AsyncExitsSavePassport(APIRequest request);
    }

}
