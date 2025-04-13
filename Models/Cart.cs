using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Furni.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        // Thuộc tính để lưu tổng giá trị của giỏ hàng
        public decimal TotalPrice { get; set; }

        // Thuộc tính để lưu ngày tạo giỏ hàng
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Thiết lập quan hệ 1-1 với ApplicationUser
        [Required]
        public string UserId { get; set; } // Khóa ngoại liên kết với ApplicationUser

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } // Điều hướng đến ApplicationUser

        // Quan hệ 1-Nhiều với CartItem
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}