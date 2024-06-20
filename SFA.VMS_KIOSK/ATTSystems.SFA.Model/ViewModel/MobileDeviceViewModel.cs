using ATTSystems.NetCore.Model.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.Model.ViewModel
{
    public class MobileDeviceViewModel
    {
        public MobileDeviceViewModel()
        {
            ListMobileDevice = new List<MobileDeviceViewList>();
            visitorTypeLists = new List<VisitorTypeList>();
            mobileunitsDetailLists = new List<MobileUnitsDetailList>();
        }
        public long staffid { get; set; }
        public int visitId { get; set; }

        [Required(ErrorMessage = "NRIC is required")]
        public string? NRICNumber { get; set; }
        public int Id { get; set; }
        public string? passportNumber { get; set; }
        public string? visitorName { get; set; }
        public string? mobileNumber { get; set; }
        public string? visitPurpose { get; set; }
        public string? vehicleNumber { get; set; }
        public string? visitorTypeName { get; set; }
        public int VisitTypeId { get; set; }
        public string? emailId { get; set; }
        public string? location { get; set; }
        public int locationid { get; set; }
        public string? companyName { get; set; }
        public DateTime QRdate { get; set; }
        public DateTime? Fromdate { get; set; }
        public DateTime? Todate { get; set; }
        public int jurongLocatin { get; set; }
        public int pasirLocatin { get; set; }
        public string? blockNo { get; set; }
        public string? unitNo { get; set; }
        public string passunitNo { get; set; }

        public int NId { get; set; }
        public string MobileApp { get; set; }
        public string? NRICPassport { get; set; }
        public List<MobileDeviceViewList> ListMobileDevice { get; set; }
        public List<MobileDeviceViewModel> VisitorLocationList { get; set; }
        public List<VisitorTypeList> visitorTypeLists { get; set; }
        public List<MobileUnitsDetailList> mobileunitsDetailLists { get; set; }

        public static implicit operator MobileDeviceViewModel(int v)
        {
            throw new NotImplementedException();
        }

        public static implicit operator int(MobileDeviceViewModel v)
        {
            throw new NotImplementedException();
        }
    }
    public class MobileDeviceViewList
    {
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
        public string? listNRICPassport { get; set; }
        public string? listVisitTypeid { get; set; }

    }
    public class VisitorTypeList
    {
        public int listId { get; set; }
        public string? TypeName { get; set; }
    }

    public class MobileUnitsDetailList
    {
        public int Id { get; set; }

        public int? LocationId { get; set; }
        public string? BlockNo { get; set; }
        public string? UnitNo { get; set; }
        public string? UnitId { get; set; }

    }
}

