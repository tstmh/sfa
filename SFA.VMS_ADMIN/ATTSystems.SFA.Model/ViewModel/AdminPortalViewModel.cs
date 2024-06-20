using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.Model.ViewModel
{
    public class AdminPortalViewModel
    {
        public int Id { get; set; }
        public string? staffEntry { get; set; }
        public string? staffExit { get; set; }
        public string? accessPoint { get; set; }
        public string? Status { get; set; }
        public string? scheduleName { get; set; }
        public string? floorId { get; set; }
        public string? floorName { get; set; }
        public string? doorId { get; set; }
        public string? doorName { get; set; }
        public string? Temperature { get; set; }
        public string? memberType { get; set; }
        public string? Staff { get; set; }
        public string? Student { get; set; }
        public string? deviceId { get; set; }
        public string? deviceName { get; set; }
        public string? ipAddress { get; set; }
        public string? Days { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string? Purpose { get; set; }
        public string? ruleName { get; set; }
    }
}
