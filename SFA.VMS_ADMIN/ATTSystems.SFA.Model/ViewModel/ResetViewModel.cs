using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.Model.ViewModel
{
    public class ResetViewModel
    {
        public ResetViewModel()
        {
            securityQuestions = new List<SecurityQuestions>();
        }
        public string? Username { get; set; }
        public string? SecurityQuestion { get; set; }

        [Display(Name = "Security Question")]
        //[Required]
        //[Range(1, 10, ErrorMessage = "Security Question is required.")]
        public int QuestionId { get; set; }

        [Display(Name = "Answer")]
        //[Required(ErrorMessage = "{0} is required.")]
        public string? Answer { get; set; }
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        //[Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 10)]
        public string? OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        //[RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{12,}$", ErrorMessage = "Passwords must be at least 12 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. Example@12345*)")]
        //[Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 10)]
        public string? NewPassword { get; set; }
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        //[RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{12,}$", ErrorMessage = "Passwords must be at least 12 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. Example@12345*)")]
        public string? ConfirmPassword { get; set; }
        public List<SecurityQuestions> securityQuestions { get; set; }

    }
    public class SecurityQuestions
    {
        public int id { get; set; }
        public string? Question { get; set; }
    }
}
