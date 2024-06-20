using ATTSystems.SFA.Model.ViewModel;
using ATTSystems.SFA.Web.Models.ViewModel;

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
               
        public int resultId { get; set; }
        public string? AccessToken { get; set; }
        public int resultCode { get; set; }
        public string? resultDescription { get; set; }

        public List<OnlinePortalViewList>? listOnlinePort { get; set; }
        public OnlinePortalViewModel? OnlinePortalViewModel { get; set; }
        public List<OnlineUnitsDetailList>? OnlineUnitsDetailLists { get; set; }

        public List<RegistrationViewList>? registrationViewLists { get; set; }
        public List<RegistrationViewList>? registrationViewListsall { get; set; }

        public RegistrationViewModel? rmodel { get; set; }

        public List<RegistrationViewList>? EntryViewLists { get; set; }
        public List<RegistrationViewList>? StayoverViewLists { get; set; }
        public List<UnitsDetailList>? UnitsDetailLists { get; set; }
        public List<VisitorAccessList>? VisitorAccessLists { get; set; }
        public List<TransList>? transLists { get; set; }
        public List<TransList>? transLists1 { get; set; }
        public List<VsttypList>? VsttypList { get; set; }
        public List<PasswordSettingViewModel>? pwdstngView { get; set; }
        public List<PasswordSettingList>? Passwordsetting { get; set; }
        public int EntryCount { get; set; }
        public int ExitCount { get; set; }
        public int LiveCount { get; set; }
        public int StayoverCount { get; set; }
        public List<UserSessionTrackingList>? _usersessiontrackinglist { get; set; }

    }
}
