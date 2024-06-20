using ATTSystems.SFA.Model.ViewModel;

namespace ATTSystems.SFA.Web.Models
{
    public class JsonResp
    {
        // Take note of the Javascript, the variable response as following format. Cannot declare ResultCode <= xx
        public int resultCode { get; set; }
        public string? resultDescription { get; set; }
        public string? resultType { get; set; }
        public string? resultID { get; set; }
        public List<UnitsDetailList>? UnitsDetailLists { get; set; }
    }
}
