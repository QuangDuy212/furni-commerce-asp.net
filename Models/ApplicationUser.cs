using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Furni.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        // Quan hệ 1-1 với Cart
        public Cart Cart { get; set; }
    }
}