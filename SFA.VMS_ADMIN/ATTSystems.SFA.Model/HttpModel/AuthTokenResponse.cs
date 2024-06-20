using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.Model.HttpModel
{
    public class AuthTokenResponse
    {
        public AuthTokenResponse()
        {
            Succeeded = false;
            Code = 0;
            Message = "none";
            ID = 0;
        }

        public bool Succeeded
        {
            get;
            set;
        }

        public int Code
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public int ID
        {
            get;
            set;
        }

        public int resultId { get; set; }

        public string? AccessToken { get; set; }
        public int? resultCode { get; set; }
        public string? resultDescription { get; set; }


    }
}
