using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Furni.Models;

namespace Furni.Controllers;

public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;

    public AdminController(ILogger<AdminController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult UserManagement()
    {
        return View("UserManagement/Index");
    }

    public IActionResult Order()
    {
        return View("Order/Index");
    }

    public IActionResult Product()
    {
        return View("Product/Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
