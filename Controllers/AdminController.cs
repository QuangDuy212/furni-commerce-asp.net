using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Furni.Models;
using Microsoft.AspNetCore.Identity;
using Furni.ViewModel;
using ClosedXML.Excel;
using System.ComponentModel;

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

    [HttpGet]
    public async Task<IActionResult> UpdateUserView(string id)
    {
        // Tìm người dùng theo ID
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        // Chuyển đổi từ ApplicationUser sang UserViewModel
        var userViewModel = new UserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address
        };

        // Trả về view với UserViewModel
        return View("UserManagement/Update", userViewModel);
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
    [HttpGet]
    public async Task<IActionResult> DeleteUser(string id)
    {
        // Tìm người dùng theo ID
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        // Xóa người dùng
        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            // Chuyển hướng về trang UserManagement sau khi xóa thành công
            return RedirectToAction("UserManagement");
        }

        // Xử lý lỗi nếu xóa thất bại
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        // Trả về lại danh sách người dùng nếu có lỗi
        var users = _userManager.Users.Select(u => new UserViewModel
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Address = u.Address
        }).ToList();

        return View("UserManagement/Index", users);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMultipleUsers(List<string> ids)
    {
        if (ids == null || !ids.Any())
        {
            ModelState.AddModelError(string.Empty, "No users selected for deletion.");
            return RedirectToAction("UserManagement");
        }

        foreach (var id in ids)
        {
            // Tìm người dùng theo ID
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Xóa người dùng
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
        }

        // Chuyển hướng về trang UserManagement sau khi xóa
        return RedirectToAction("UserManagement");
    }



    [HttpGet]
    public IActionResult CreateUserView()
    {
        // Trả về view để hiển thị form tạo người dùng
        return View("UserManagement/Create");
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Tạo một đối tượng ApplicationUser mới
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address
            };

            // Tạo người dùng với mật khẩu mặc định
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Chuyển hướng về trang UserManagement sau khi tạo thành công
                return RedirectToAction("UserManagement");
            }

            // Xử lý lỗi nếu tạo thất bại
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Nếu ModelState không hợp lệ, trả về lại view với dữ liệu hiện tại
        return View("UserManagement/Create", model);
    }


    [HttpPost]
    public IActionResult ExportSelectedUsers(List<string> ids)
    {
        if (ids == null || !ids.Any())
        {
            return BadRequest("No users selected for export.");
        }

        // Lấy danh sách người dùng theo ID
        var users = _userManager.Users
            .Where(user => ids.Contains(user.Id))
            .Select(user => new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Address
            }).ToList();

        // Tạo file Excel bằng ClosedXML
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Users");

            // Tạo tiêu đề cột
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Full Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Phone Number";
            worksheet.Cell(1, 5).Value = "Address";

            // Điền dữ liệu vào các hàng
            for (int i = 0; i < users.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = users[i].Id;
                worksheet.Cell(i + 2, 2).Value = users[i].FullName;
                worksheet.Cell(i + 2, 3).Value = users[i].Email;
                worksheet.Cell(i + 2, 4).Value = users[i].PhoneNumber;
                worksheet.Cell(i + 2, 5).Value = users[i].Address;
            }

            // Lưu file Excel vào bộ nhớ
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                // Trả về file Excel
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> ImportUsers(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please upload a valid Excel file.");
        }

        try
        {
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                // Mở file Excel bằng ClosedXML
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                    var rows = worksheet.RowsUsed();

                    foreach (var row in rows.Skip(1)) // Bỏ qua dòng tiêu đề
                    {
                        try
                        {
                            // Kiểm tra và loại bỏ hyperlink trong cột Email (nếu có)
                            var emailCell = row.Cell(3);
                            if (emailCell.HasHyperlink)
                            {
                                emailCell.Hyperlink = null; // Loại bỏ hyperlink
                            }

                            var user = new ApplicationUser
                            {
                                UserName = row.Cell(1).GetValue<string>(),
                                FullName = row.Cell(2).GetValue<string>(),
                                Email = emailCell.GetValue<string>(),
                                PhoneNumber = row.Cell(4).GetValue<string>(),
                                Address = row.Cell(5).GetValue<string>()
                            };

                            var password = row.Cell(6).GetValue<string>(); // Lấy mật khẩu từ cột thứ 6

                            // Tạo người dùng với mật khẩu từ file Excel
                            var result = await _userManager.CreateAsync(user, password);
                            if (!result.Succeeded)
                            {
                                foreach (var error in result.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Ghi log lỗi và bỏ qua dòng bị lỗi
                            _logger.LogError($"Error processing row: {ex.Message}");
                            continue;
                        }
                    }
                }
            }

            return Ok("Users imported successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred while importing users: {ex.Message}");
        }
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
