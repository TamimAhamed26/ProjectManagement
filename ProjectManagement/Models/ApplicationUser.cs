using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(25, ErrorMessage = "Name must be less than or equal 25 characters.")]
        public string? Name { get; set; }

        [StringLength(11, ErrorMessage = "Phone no must be 11 characters.")]
        public string? PhoneNo { get; set; }

        [StringLength(200, ErrorMessage = "Path cannot be longer than 200 characters.")]
        public string? PicturePath { get; set; }

        [NotMapped]
        public IFormFile? Picture { get; set; }
    }
}
