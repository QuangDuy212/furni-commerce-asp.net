using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Furni.Models;

namespace Furni.Controllers;

public class CheckoutController : Controller
{
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(ILogger<CheckoutController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
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
