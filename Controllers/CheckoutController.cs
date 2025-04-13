using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Furni.Models;
using Furni.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Furni.Controllers;

public class CheckoutController : Controller
{
    private readonly ILogger<CheckoutController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CheckoutController(ILogger<CheckoutController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // Lấy UserId của người dùng đang đăng nhập
        var userId = _userManager.GetUserId(User);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
        }

        // Lấy giỏ hàng của người dùng
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
        {
            // Nếu giỏ hàng rỗng, chuyển hướng về trang giỏ hàng
            return RedirectToAction("Index", "Cart");
        }

        // Truyền giỏ hàng đến view
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        // Lấy UserId của người dùng đang đăng nhập
        var userId = _userManager.GetUserId(User);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
        }

        // Lấy giỏ hàng của người dùng
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
        {
            // Nếu giỏ hàng rỗng, chuyển hướng về trang giỏ hàng
            return RedirectToAction("Index", "Cart");
        }

        // Tạo đơn hàng mới
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            TotalAmount = cart.TotalPrice,
            Status = "Pending",
            OrderItems = cart.CartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                Price = ci.Product.Price
            }).ToList()
        };

        // Lưu đơn hàng vào cơ sở dữ liệu
        _context.Orders.Add(order);

        // Xóa giỏ hàng sau khi tạo đơn hàng
        _context.CartItems.RemoveRange(cart.CartItems);
        cart.CartItems.Clear();
        cart.TotalPrice = 0;

        await _context.SaveChangesAsync();

        // Chuyển hướng đến trang "Thank You"
        return RedirectToAction("Index", "Thankyou");
    }

    public IActionResult OrderConfirmation(int orderId)
    {
        // Hiển thị trang xác nhận đơn hàng
        var order = _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefault(o => o.Id == orderId);

        if (order == null)
        {
            return RedirectToAction("Index", "Home"); // Nếu không tìm thấy đơn hàng, chuyển hướng về trang chủ
        }

        return View(order);
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
