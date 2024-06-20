using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.Model.ViewModel
{
    public class ReportViewModel
    {
        public ReportViewModel()
        {
            VisitorAccessLists = new List<VisitorAccessList>();
            VsttypList = new List<VsttypList>();
            transLists = new List<TransList>();
            transLists1 = new List<TransList>();
            _usersessiontrackinglist = new List<UserSessionTrackingList>();

        }
        public long Id { get; set; }
        public string? VisitorName { get; set; }
        public int IdType { get; set; }
        public int Aditid { get; set; }
        public string? NricOrPassport { get; set; }
        public string? NricOrPassport1 { get; set; }
        public int VisitTypeId { get; set; }
        public DateTime? CreateDateTime { get; set; }
        public string? VistStartDateTime { get; set; }
        public string? VistEndDateTime { get; set; }
        public DateTime? BlacklistDateTime { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? LocationName { get; set; }
        public string? VisitTypeName { get; set; }
        public string? WorkTypeName { get; set; }
        public string? Name { get; set; }
        public string? ReasonForBlacklist { get; set; }
        public string? BlacklistBy { get; set; }
        public int LocationId { get; set; }
        public DateTime? VisitStratDateTimelst { get; set; }
        public DateTime? VisitEndDateTimelst { get; set; }
        public int ManualCheckIn { get; set; }
        public string? ManualCheckInBy { get; set; }
        public string? AuditType { get; set; }
        public int AuditTypeId { get; set; }
        public int Searchid { get; set; }
       
        public string? Nooftranscation { get; set; }
        public List<TransList>? transLists { get; set; }
        public List<VisitorAccessList> VisitorAccessLists { get; set; }
        public string? Remarks { get; set; }
        public IEnumerable<object>? VisitorAccessList { get; set; }
        public List<VsttypList>? VsttypList { get; set; }
        public List<TransList>? transLists1 { get; set; }
        public List<UserSessionTrackingList> _usersessiontrackinglist { get; set; }
        public DateTime FDate { get; set; }
        public DateTime TDate { get; set; }
        public string? UserName { get; set; }
        public int UserId { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        public string? ModifiedDateTime1 { get; set; }
        public int ModifiedTypeId { get; set; }
        public int UserEmail { get; set; }
        public int ModifiedId { get; set; }
        public int ModifiedTypeIdlst { get; set; }
        public string? Modulename { get; set; }

        //Login User
        public DateTime? LFomDate { get; set; }
        public DateTime? LToDate { get; set; }
        public string? LogUserName { get; set; }


    }
    public class VisitorAccessList
    {
        public long Idlst { get; set; }
        public string? VisitorNamelst { get; set; }
        public int IdTypelst { get; set; }
        public int Aditidlst { get; set; }
        public string? NricOrPassportlst { get; set; }
        public string? NricOrPassport1lst { get; set; }
        public int VisitTypeIdlst { get; set; }
        public DateTime? CreateDateTimelst { get; set; }
        public DateTime? EntryDateTimelst { get; set; }
        public DateTime? ExitDateTimelst { get; set; }
        public string? EntryDateTimelst1 { get; set; }
        public string? ExitDateTimelst1 { get; set; }
        public DateTime? TransactionDateTimelst { get; set; }
        public string? VisitTypeNamelst { get; set; }
        public DateTime? BlacklistDateTimelst { get; set; }
        public string? BlacklistDateTimelst1 { get; set; }
        
        public string? LocationNamelst { get; set; }
        public int LocationIdlst { get; set; }

        public DateTime? StartEndDatelst { get; set; }
        public DateTime? EndDatelst { get; set; }
        public string? WorkTypeNamelst { get; set; }
        public string? StartDatelst { get; set; }
        public string? Namelst { get; set; }
        public string? ReasonForBlacklistlst { get; set; }
        public string? Blackliststatus { get; set; }
        public string? BlacklistBylst { get; set; }
        public DateTime? VisitStratDateTimelst { get; set; }
        public DateTime? VisitEndDateTimelst { get; set; }
        public string? VisitStratDateTimelst1 { get; set; }
        public string? VisitEndDateTimelst1 { get; set; }
        public int ManualCheckInlst { get; set; }
        public string? ManualCheckInBylst { get; set; }
        public string? AuditType { get; set; }
        public string? UserNamelst { get; set; }
        public int UserIdlst { get; set; }
        public string? ModifiedBylst { get; set; }
        public string? ModifiedDateTime1lst { get; set; }
        public DateTime? ModifiedDateTimelst { get; set; }
        public int ModifiedTypeIdlst { get; set; }
        public string? UserEmaillst { get; set; }
        public string? Remarks { get; set; }
    }
    public class VsttypList
    {
        public int lVstId { get; set; }
        public string? lVstTypeNmae { get; set; }
        public int lVsttypeId { get; set; }
        public bool lVstIsSelected { get; set; }
    }
    public class TransList
    {

        public int Idlst { get; set; }
        public DateTime Datelst { get; set; }
        public string? Datelst1 { get; set; }
        public int Countlst { get; set; }
        public string? Vsttypnamelst { get; set; }

    }

    public class UserSessionTrackingList
    {
        public long Id { get; set; }
        public string? UserName { get; set; }
        public string? Status { get; set; }
        public DateTime? LoginDateTime { get; set; }
        public DateTime? LogoutDateTime { get; set; }
        public DateTime? AttemptedDateTime { get; set; }
        public string? Remarks { get; set; }

    }



}
