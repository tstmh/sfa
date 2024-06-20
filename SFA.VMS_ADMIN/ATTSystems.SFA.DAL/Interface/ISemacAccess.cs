using ATTSystems.NetCore.Model.DBModel;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.HttpModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.DAL.Interface
{
    public interface ISemacAccess
    {
        public Task<int> InsertEntry(APIRequest request);
        public Task<int> UpdateExit(APIRequest request);
        public Task<int> UpdateTerminalStatus(APIRequest request);
        public Task<TokenAuth?> GetUserAsync(AuthRequest request);
        public Task<Setting?> GetExpiryDurationAsync();
        public Task<int> UpdateTokenAuth(TokenAuth? tAuth);
        public Task<int> PushVisitorAsync(APIRequest request);
        public Task<int> PushCardDetailsAsync(APIRequest request);
        public Task<int> PushOverStayerAsync(APIRequest request);
        public Task<int> PushVisitorLocationsAsync(APIRequest request);
        public Task<int> PushMessageLogsAsync(APIRequest request);
    }
}
