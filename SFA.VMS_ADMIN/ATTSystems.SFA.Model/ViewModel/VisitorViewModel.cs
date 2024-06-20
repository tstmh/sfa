using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.Model.ViewModel
{
    public class VisitorViewModel
    {
        public VisitorViewModel()
        {
            visitorsLists = new List<VisitorsList>();
            cards = new List<CardIssueList>();
            visitorTransactions = new List<VisitorTransactionList>();
            terminals = new List<TerminalList>();
            LogLists = new List<MessageLogList>();
        }
        public List<VisitorsList> visitorsLists { get; set; }
        public List<CardIssueList> cards { get; set; }
        public List<VisitorTransactionList> visitorTransactions { get; set; }
        public List<TerminalList> terminals { get; set; }
        public List<MessageLogList> LogLists { get; set; }
    }
    public class VisitorsList
    {

        public string? VisitorName { get; set; }
        public int? IdType { get; set; }
        public string? NricOrPassport { get; set; }
        public string? VisitorContanctNo { get; set; }
        public string? Email { get; set; }
        public string? VisitorVisitDesc { get; set; }
        public string? BlockNo { get; set; }
        public string? UnitNo { get; set; }
        public string? CompanyName { get; set; }
        public int VisitTypeId { get; set; }
        public long? TerminalId { get; set; }
        public int VisitorStatus { get; set; }
        public string? VehicleNo { get; set; }
        public int? PurposeOfVisitId { get; set; }
        public DateTime? VistStartDateTime { get; set; }
        public DateTime? VistEndDateTime { get; set; }
        public bool IsBlockListed { get; set; }
        public string? CreateBy { get; set; }
        public DateTime CreateDateTime { get; set; }
        public bool? IsDeleted { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public string? ReasonForBlacklist { get; set; }
        public int? UploadtoController { get; set; }
        public string? BlacklistBy { get; set; }
        public DateTime? BlacklistDateTime { get; set; }
        public string? RegistrationBy { get; set; }
        public int? ManualCheckIn { get; set; }
        public string? ManualCheckInBy { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? EnabledOverStayer { get; set; }
        public bool? pushVisitors { get; set; }
        public bool? IsDisabled { get; set; }
        public bool? PushOverStayer { get; set; }
        public bool? OverStayer { get; set; }
        public int LocationId { get; set; }
        public string? locationsid { get; set; }
    }
    public class CardIssueList
    {
        public string? CardNumber { get; set; }

        public DateTime? IssueDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreateDateTime { get; set; }

        public string? NricOrPassport { get; set; }

        public bool IsPushed { get; set; }
    }
    public class VisitorTransactionList
    {
        public int TransID { get; set; }
        public long VisitorRegistrationId { get; set; }
        public DateTime? TransactionDateTime { get; set; }
        public int? TransactionType { get; set; }
        public string? ExitDoor { get; set; }
        public int? LocationId { get; set; }
        public DateTime? EntryDateTime { get; set; }
        public DateTime? ExitDateTime { get; set; }
        public int? EntryTerminalId { get; set; }
        public string? ReactivatedBy { get; set; }
        public DateTime? ReactivatedDateTime { get; set; }
        public string? ReasonToReactivate { get; set; }
        public string? EntryDoor { get; set; }
        public bool Flag { get; set; }
        public int? ExitTerminalId { get; set; }
        public string? NricOrPassport { get; set; }
        public bool EntryPushed { get; set; }
        public bool ExitPushed { get; set; }
        public bool IsDisabled { get; set; }

    }
    public class TerminalList
    {
        public bool? IsOnline { get; set; }
        public int? TerminalId { get; set; }
    }
    public class MessageLogList
    {
        public string? Message { get; set; }
        public string? Recipient { get; set; }
        public string? CC { get; set; }
        public int SentStatus { get; set; }
        public DateTime SentDateTime { get; set; }
        public string? CardNumber { get; set; }
    }
}
