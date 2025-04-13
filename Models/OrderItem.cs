using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Furni.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; } // Khóa chính của mục đơn hàng

        [Required]
        public int OrderId { get; set; } // Khóa ngoại liên kết với đơn hàng

        [ForeignKey("OrderId")]
        public Order Order { get; set; } // Điều hướng đến đơn hàng

        [Required]
        public int ProductId { get; set; } // Khóa ngoại liên kết với sản phẩm

        [ForeignKey("ProductId")]
        public Product Product { get; set; } // Điều hướng đến sản phẩm

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } // Số lượng sản phẩm

        [Required]
        public decimal Price { get; set; } // Giá của sản phẩm tại thời điểm đặt hàng
    }
}