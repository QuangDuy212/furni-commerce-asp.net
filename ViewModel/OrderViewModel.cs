using System.ComponentModel.DataAnnotations;

namespace Furni.ViewModel
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now; // Ngày đặt hàng

        public decimal TotalAmount { get; set; } // Tổng giá trị đơn hàng

        public string Status { get; set; }
    }
}
