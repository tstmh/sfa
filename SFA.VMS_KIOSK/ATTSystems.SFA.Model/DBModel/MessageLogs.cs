using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.Model.DBModel
{
    public class MessageLogs
    {
        public int Id { get; set; }
        public string? Message { get; set; }

        public string? Recipient { get; set; }

        public string? CC { get; set; }

        public int SentStatus { get; set; }

        public DateTime? SentDateTime { get; set; }
        public string? RespMessage { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string? CardNumber { get; set; }
        public bool? IsPushed { get; set; }
    }
}
