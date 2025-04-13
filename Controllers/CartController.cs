using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Furni.Models;
using Furni.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Furni.Controllers;

public class CartController : Controller
{
    private readonly ILogger<CartController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(ILogger<CartController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

        // Lấy giỏ hàng từ cơ sở dữ liệu, bao gồm các mục trong giỏ hàng và thông tin sản phẩm
        var cart = await _context.Carts
            .Include(c => c.CartItems) // Bao gồm các mục trong giỏ hàng
            .ThenInclude(ci => ci.Product) // Bao gồm thông tin sản phẩm trong mỗi mục
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            // Nếu giỏ hàng chưa tồn tại, tạo mới
            cart = new Cart
            {
                UserId = userId,
                TotalPrice = 0,
                CreatedDate = DateTime.Now,
                CartItems = new List<CartItem>()
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return View(cart); // Truyền giỏ hàng đến view
    }

    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
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
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            // Nếu giỏ hàng chưa tồn tại, tạo mới
            cart = new Cart
            {
                UserId = userId,
                TotalPrice = 0,
                CreatedDate = DateTime.Now,
                CartItems = new List<CartItem>()
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

        if (cartItem == null)
        {
            // Nếu sản phẩm chưa có trong giỏ hàng, thêm mới
            cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity
            };

            cart.CartItems.Add(cartItem);
        }
        else
        {
            // Nếu sản phẩm đã có trong giỏ hàng, tăng số lượng
            cartItem.Quantity += quantity;
        }

        // Lấy thông tin sản phẩm để cập nhật tổng giá trị giỏ hàng
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            cart.TotalPrice += product.Price * quantity;
        }

        // Lưu thay đổi vào cơ sở dữ liệu
        await _context.SaveChangesAsync();

        return RedirectToAction("Index"); // Chuyển hướng về trang giỏ hàng
    }

    public async Task<IActionResult> RemoveItem(int id)
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
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return RedirectToAction("Index"); // Nếu giỏ hàng không tồn tại, chuyển hướng về trang giỏ hàng
        }

        // Tìm mục giỏ hàng cần xóa
        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == id);

        if (cartItem != null)
        {
            // Cập nhật tổng giá trị giỏ hàng
            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product != null)
            {
                cart.TotalPrice -= product.Price * cartItem.Quantity;
            }

            // Xóa mục giỏ hàng
            cart.CartItems.Remove(cartItem);
            _context.CartItems.Remove(cartItem); // Xóa mục giỏ hàng khỏi cơ sở dữ liệu
        }

        // Lưu thay đổi vào cơ sở dữ liệu
        await _context.SaveChangesAsync();

        return RedirectToAction("Index"); // Chuyển hướng về trang giỏ hàng
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
