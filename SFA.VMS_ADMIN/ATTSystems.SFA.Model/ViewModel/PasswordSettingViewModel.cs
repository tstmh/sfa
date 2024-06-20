using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.Model.ViewModel
{
    public class PasswordSettingViewModel
    {
        public PasswordSettingViewModel()
        {
            Passwordsetting = new List<PasswordSettingList>();
            PwdstngView = new List<PasswordSettingViewModel>();
        }
        public int Id { get; set; }
        public int MaxPwdLife { get; set; }
        public int MaxPwdFailedCount { get; set; }
        public int MinPwdLength { get; set; }
        public int MinLowerCase { get; set; }
        public int MinUpperCase { get; set; }
        public int MinNumeric { get; set; }
        public int MinSpecialCharacter { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<PasswordSettingList>? Passwordsetting { get; set; }
        public List<PasswordSettingViewModel>? PwdstngView { get; set; }
    }
    public class PasswordSettingList
    {
        public int Id { get; set; }
        public int MaxPwdLife { get; set; }
        public int MaxPwdFailedCount { get; set; }
        public int MinPwdLength { get; set; }
        public int MinLowerCase { get; set; }
        public int MinUpperCase { get; set; }
        public int MinNumeric { get; set; }
        public int MinSpecialCharacter { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}

