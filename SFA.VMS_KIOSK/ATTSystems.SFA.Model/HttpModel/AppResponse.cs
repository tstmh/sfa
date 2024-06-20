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
              
        public List<KioskViewModel> KioskLists { get; set; }
        public List<KioskViewList> listKiosk { get; set; }
        public List<UnitsDetailList> UnitsDetailLists { get; set; }

        public int locationid
        {
            get; set;
        }

        public List<MobileDeviceViewList> ListMobileDevice { get; set; }
        public MobileDeviceViewModel MobileDeviceViewModel { get; set; }
        public List<MobileUnitsDetailList> mobileunitsDetailLists { get; set; }
    }
}
