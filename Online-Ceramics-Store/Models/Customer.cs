using System;
using System.ComponentModel.DataAnnotations;
namespace Online_Ceramics_Store.Models
{
	public class Customer
	{
		public Customer()
		{
		}
		public int cust_id { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Full name must be between 2-20 letters")]
        public string full_name { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "mail must be with @ in the middle")]
        public string email { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "password must be between 2-20 letters")]
        public string password { get; set; }


        [Required]
        [RegularExpression(@"^05\d{8}$", ErrorMessage = "Please enter a valid phone number")]
        public string phone { get; set; }


        [Required]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "city must be between 2-20 letters")]
        public string city { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "address must be between 4-40 letters")]
        public string address { get; set; }








    }
}

