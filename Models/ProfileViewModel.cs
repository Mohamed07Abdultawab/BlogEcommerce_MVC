using System.ComponentModel.DataAnnotations;

namespace BlogEcommerce.Models
{
    public class ProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "City")]
        public string? City { get; set; }

        [Display(Name = "Zip Code")]
        public string? ZipCode { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Profile Picture URL")]
        [Url]
        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "Bio")]
        [StringLength(500)]
        public string? Bio { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public DateTime MemberSince { get; set; }

        // Statistics
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalReviews { get; set; }
        public int TotalComments { get; set; }
    }
}