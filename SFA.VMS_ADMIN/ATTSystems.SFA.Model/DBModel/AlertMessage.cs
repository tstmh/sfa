using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.Model.DBModel
{
    public class AlertMessage
    {
        public int Id { get; set; }
        public string? MessageString { get; set; }
        public string? Recipient { get; set; }
        public string? CC { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsEnabled { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string? CreatedBy { get; set; }

    }
}
