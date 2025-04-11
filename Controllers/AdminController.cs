using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Furni.Models;
using Microsoft.AspNetCore.Identity;
using Furni.ViewModel;

namespace Furni.Controllers;

public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ILogger<AdminController> logger, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }


    public async Task<IActionResult> UserManagement()
    {
        var users = _userManager.Users.Select(user => new UserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address
        }).ToList();

        return View("UserManagement/Index", users);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateUserView(string Id)
    {
        if (ModelState.IsValid)
        {
            // Tìm người dùng theo ID
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound($"User with ID {Id} not found.");
            }

            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };

            return View("UserManagement/Update", userViewModel);
        }
        else
            return RedirectToAction("UserManagement");

        // Nếu ModelState không hợp lệ, trả về lại view với dữ liệu hiện tại
        // return View("UserManagement/Update", model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateUser(UserViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Tìm người dùng theo ID
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound($"User with ID {model.Id} not found.");
            }

            // Cập nhật thông tin người dùng
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            // Lưu thay đổi
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("UserManagement");
            }

            // Xử lý lỗi nếu cập nhật thất bại
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            // return View("UserManagement/Update", model);
        }

        // Nếu ModelState không hợp lệ, trả về lại view với dữ liệu hiện tại
        return View("UserManagement/Update", model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("UserManagement");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View("UserManagement/Index", _userManager.Users.ToList());
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
