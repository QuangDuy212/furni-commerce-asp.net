using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Furni.Models;
using Furni.Data;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace Furni.Controllers;

public class ShopController : Controller
{
    private readonly ILogger<ShopController> _logger;
    private readonly ApplicationDbContext _context; // Thêm ApplicationDbContext để truy cập database

    public ShopController(ILogger<ShopController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(int? page)
    {
        int pageSize = 8; // Số sản phẩm trên mỗi trang
        int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1

        var products = await _context.Products.ToListAsync();

        // Sử dụng X.PagedList để phân trang
        var pagedProducts = products.ToPagedList(pageNumber, pageSize);

        return View(pagedProducts);
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
