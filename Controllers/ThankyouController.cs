using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Furni.Models;

namespace Furni.Controllers;

public class ThankyouController : Controller
{
    private readonly ILogger<ThankyouController> _logger;

    public ThankyouController(ILogger<ThankyouController> logger)
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
