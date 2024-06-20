using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.Model.ViewModel
{
    public class KioskViewModel
    {
        public KioskViewModel()
        {
            KioskList = new List<KioskViewList>();
            UnitsDetailLists= new List<UnitsDetailList>();
        }
        
        public int visitId { get; set; }
        public string? NRICPassport { get; set; }
        public string? visitorName { get; set; }
        public string? mobileNumber { get; set; }
        public string? visitPurpose { get; set; }
        public string? vehicleNumber { get; set; }
        public string? visitorType { get; set; }
        public string? companyName { get; set; }
        public string? blockNo {  get; set; }    
        public string? unitNo {  get; set; }
        public string? emailId { get; set; }
        public int NId { get; set; }
        public int alertId { get; set; }
        public DateTime Fromdate { get; set; }
        public DateTime Todate { get; set; }
        public string? NricFromdate { get; set; }
        public string? NricTodate { get; set; }
        public int IdType {  get; set; }
        public int locationid { get; set; }
        public List<KioskViewList> KioskList { get; set; }
        public List<UnitsDetailList> UnitsDetailLists { get; set; }      

    }
    public class KioskViewList
    {
        public int listId { get; set; }
        public string? listNRICPassport { get; set; }
        public string? listvisitorName { get; set; }
        public string? listmobileNumber { get; set; }
        public string? listvisitPurpose { get; set; }
        public string? listvehicleNumber { get; set; }
        public string? listvisitorType { get; set; }
        public string? listcompanyName { get; set; }
        public string? listPassportNumber { get; set; }
        public string? listEmail { get; set; }
    }

    public class UnitsDetailList
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string? BlockNo { get; set; }
        public string? UnitNo { get; set; }
        public string? UnitId { get; set; }

    }
}

