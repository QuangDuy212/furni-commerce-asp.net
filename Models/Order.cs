using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Furni.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; } // Khóa chính của đơn hàng

        [Required]
        public string UserId { get; set; } // Khóa ngoại liên kết với người dùng

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } // Điều hướng đến người dùng

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now; // Ngày đặt hàng

        [Required]
        public decimal TotalAmount { get; set; } // Tổng giá trị đơn hàng

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Trạng thái đơn hàng (Pending, Completed, Canceled, etc.)

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); // Danh sách các mục trong đơn hàng
    }
}