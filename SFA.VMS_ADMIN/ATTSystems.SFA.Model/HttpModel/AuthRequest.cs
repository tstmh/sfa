using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.Model.HttpModel
{
    public class AuthRequest
    {
        public AuthRequest()
        {
            RequestType = string.Empty;
            RequestString = string.Empty;
            UserKey = string.Empty;
            UserCode = string.Empty;
            Message = string.Empty;
            Model = string.Empty;
        }

        public string RequestType { get; set; }
        public string RequestString { get; set; }
        public string UserKey { get; set; }
        public string UserCode { get; set; }
        public string Message { get; set; }
        public object Model { get; set; }
    }
}
