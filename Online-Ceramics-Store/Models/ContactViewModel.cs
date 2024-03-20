using System;
using System.ComponentModel.DataAnnotations;

namespace Online_Ceramics_Store.Models
{
	public class ContactViewModel
	{

        public ContactViewModel() { }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "mail must be with @ in the middle")]
        public string recipientEmail { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Full name must be between 2-20 letters")]
        public string fullName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Full name must be between 2-20 letters")]
        public string subject { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Full name must be between 2-20 letters")]
        public string message { get; set; }
    }
}