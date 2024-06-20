namespace ATTSystems.SFA.Web.Models
{
    public class QRTicketModel
    {
        public PrintTicketRequest Model { get; set; }

    }
    public class PrintTicketRequest
    {
        public string? StartDate { get; set; }

        public string? EndDate { get; set; }

        public string? QRdata { get; set; }
    }

}
