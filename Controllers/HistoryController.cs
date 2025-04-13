using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Furni.Models;
using Microsoft.AspNetCore.Identity;
using Furni.Data;
using Microsoft.EntityFrameworkCore;

namespace Furni.Controllers;

public class HistoryController : Controller
{
    private readonly ILogger<CheckoutController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HistoryController(ILogger<CheckoutController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

        // Lấy danh sách đơn hàng của người dùng
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders); // Truyền danh sách đơn hàng đến view
    }

    public async Task<IActionResult> Details(int orderId)
    {
        // Lấy UserId của người dùng đang đăng nhập
        var userId = _userManager.GetUserId(User);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
        }

        // Lấy thông tin đơn hàng
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            return RedirectToAction("Index"); // Nếu không tìm thấy đơn hàng, chuyển hướng về trang lịch sử
        }

        return View(order); // Truyền thông tin đơn hàng đến view
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
