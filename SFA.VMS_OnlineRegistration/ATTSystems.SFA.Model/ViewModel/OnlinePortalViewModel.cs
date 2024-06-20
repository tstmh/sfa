using ATTSystems.NetCore.Model.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.Model.ViewModel
{
    public class OnlinePortalViewModel
    {
        public OnlinePortalViewModel()
        {
            onlinePortalList = new List<OnlinePortalViewList>();
            OnlineUnitsDetailLists = new List<OnlineUnitsDetailList>();
            VisitorLocationList = new List<OnlinePortalViewModel>();

        }
        public int visitId { get; set; }
        public string? NRICNumber { get; set; }
        public string? passportNumber { get; set; }
        public string? visitorName { get; set; }
        public string? mobileNumber { get; set; }
        public string? visitPurpose { get; set; }
        public string? vehicleNumber { get; set; }
        public string? visitorType { get; set; }
        public string? emailId { get; set; }
        public string? location { get; set; }
        public int locationid { get; set; }
        public string? companyName { get; set; }
        public DateTime QRdate { get; set; }  
        public DateTime? Fromdate { get; set; }
        public DateTime? Todate { get; set; }
        public int jurongLocatin { get; set; }
        public int pasirLocatin {  get; set; }
        public string? blockNo { get; set; }
        public string? unitNo { get; set; }
        public int NId { get; set; }
        public List<OnlinePortalViewList> onlinePortalList { get; set; }
        public List<OnlinePortalViewModel> VisitorLocationList { get; set; }
        public List<OnlineUnitsDetailList> OnlineUnitsDetailLists { get; set; }

    }
    public class OnlinePortalViewList
    {
        public OnlinePortalViewList() { 

        }   
        public int listId { get; set; }
        public string? listNRICNumber { get; set; }
        public string? listvisitorName { get; set; }
        public string? listmobileNumber { get; set; }
        public string? listvisitPurpose { get; set; }
        public string? listvehicleNumber { get; set; }
        public string? listvisitorType { get; set; }
        public int listvisitorTypeId { get; set; }
        public string? listLocation { get; set; }
        public string? listcompanyName { get; set; }
        public string? listPassportNumber { get; set; }
        public string? listEmail { get; set; }
    }
    public class OnlineUnitsDetailList
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string? BlockNo { get; set; }
        public string? UnitNo { get; set; }
        public string? UnitId { get; set; }

    }

}

