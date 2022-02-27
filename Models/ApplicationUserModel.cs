 using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    public class ApplicationUserModel
    {
        public string Username { get; set; }
        public string Email { get; set; }

        [Required]
        [StringLength(
             100,
            ErrorMessage = "The {0} must be at least {2} characters long.",
             MinimumLength = 8)]
              [DataType(DataType.Password)]
             [Display(Name = "Password")]
        public string Password { get; set; }
        public string Fullname { get; set; }
        public string Phonenumber { get; set; }
        public string SSN { get; set; }
    }
}
