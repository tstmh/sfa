using ATTSystems.SFA.Model.ViewModel;

namespace ATTSystems.SFA.Model.HttpModel
{
    public class AppResponse
    {
        public AppResponse()
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

        public List<OnlinePortalViewList>? listOnlinePort { get; set; }
        public OnlinePortalViewModel? OnlinePortalViewModel { get; set; }
        public List<OnlineUnitsDetailList>? OnlineUnitsDetailLists { get; set; }
    }
}
