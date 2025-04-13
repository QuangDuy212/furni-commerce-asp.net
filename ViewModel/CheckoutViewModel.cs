using System.ComponentModel.DataAnnotations;
using Furni.Models;

namespace Furni.ViewModel
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
