using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Furni.Models;
using Microsoft.AspNetCore.Identity;
using Furni.ViewModel;
using ClosedXML.Excel;
using OfficeOpenXml;
using System.ComponentModel;
using Furni.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Furni.Controllers;

[Authorize(Roles = "ADMIN")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminController(ILogger<AdminController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _logger = logger;
        _userManager = userManager;
        _context = context;
    }

    public IActionResult Index()
    {
        var totalUsers = _userManager.Users.Count();
        var totalProducts = _context.Products.Count();
        var totalOrders = _context.Orders.Count();

        // Tính tổng số đơn hàng theo trạng thái
        var pendingOrders = _context.Orders.Count(o => o.Status == "Pending");
        var completedOrders = _context.Orders.Count(o => o.Status == "Completed");
        var canceledOrders = _context.Orders.Count(o => o.Status == "Canceled");

        // Tính tỷ lệ phần trăm
        double pendingPercentage = totalOrders > 0 ? (double)pendingOrders / totalOrders * 100 : 0;
        double completedPercentage = totalOrders > 0 ? (double)completedOrders / totalOrders * 100 : 0;
        double canceledPercentage = totalOrders > 0 ? (double)canceledOrders / totalOrders * 100 : 0;

        var dashboardViewModel = new DashboardViewModel
        {
            TotalUsers = totalUsers,
            TotalProducts = totalProducts,
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            CompletedOrders = completedOrders,
            CanceledOrders = canceledOrders,
            PendingOrdersPercentage = pendingPercentage,
            CompletedOrdersPercentage = completedPercentage,
            CanceledOrdersPercentage = canceledPercentage
        };

        return View(dashboardViewModel);
    }


    public async Task<IActionResult> UserManagement()
    {
        var users = await _userManager.Users.ToListAsync(); // Lấy danh sách tất cả người dùng

        var userViewModels = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user); // Lấy danh sách Role của người dùng
            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = string.Join(", ", roles) // Gộp các Role thành chuỗi (nếu có nhiều Role)
            });
        }

        return View("UserManagement/Index", userViewModels);
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

            // Lưu thay đổi thông tin người dùng
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("UserManagement/Update", model);
            }

            // Lấy danh sách Role hiện tại của người dùng
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Xóa tất cả các Role hiện tại
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeRolesResult.Succeeded)
            {
                foreach (var error in removeRolesResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("UserManagement/Update", model);
            }

            // Gán Role mới cho người dùng
            if (!string.IsNullOrEmpty(model.Role))
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (!addRoleResult.Succeeded)
                {
                    foreach (var error in addRoleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("UserManagement/Update", model);
                }
            }

            // Nếu tất cả thành công, chuyển hướng về trang UserManagement
            return RedirectToAction("UserManagement");
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

                // Thiết lập giấy phép miễn phí
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                // Mở file Excel bằng EPPlus
                using (var package = new OfficeOpenXml.ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // Lấy sheet đầu tiên
                    var rowCount = worksheet.Dimension.Rows; // Số lượng hàng trong sheet

                    for (int row = 2; row <= rowCount; row++) // Bỏ qua dòng tiêu đề
                    {
                        try
                        {
                            // Lấy dữ liệu từ các cột
                            var email = worksheet.Cells[row, 1].Text.Trim();
                            var fullName = worksheet.Cells[row, 2].Text.Trim();
                            var phoneNumber = worksheet.Cells[row, 3].Text.Trim();
                            var address = worksheet.Cells[row, 4].Text.Trim();
                            var password = worksheet.Cells[row, 5].Text.Trim();
                            var role = worksheet.Cells[row, 6].Text.Trim();

                            // Kiểm tra nếu email đã tồn tại
                            var existingUser = await _userManager.FindByEmailAsync(email);
                            if (existingUser != null)
                            {
                                _logger.LogWarning($"User with email {email} already exists. Skipping...");
                                continue;
                            }

                            // Tạo người dùng mới
                            var user = new ApplicationUser
                            {
                                UserName = email,
                                Email = email,
                                FullName = fullName,
                                PhoneNumber = phoneNumber,
                                Address = address,
                                EmailConfirmed = true // Đặt email đã xác nhận
                            };

                            var createResult = await _userManager.CreateAsync(user, password);
                            if (!createResult.Succeeded)
                            {
                                foreach (var error in createResult.Errors)
                                {
                                    _logger.LogError($"Error creating user {email}: {error.Description}");
                                }
                                continue;
                            }

                            // Gán Role cho người dùng
                            if (!string.IsNullOrEmpty(role))
                            {
                                var addRoleResult = await _userManager.AddToRoleAsync(user, role);
                                if (!addRoleResult.Succeeded)
                                {
                                    foreach (var error in addRoleResult.Errors)
                                    {
                                        _logger.LogError($"Error assigning role {role} to user {email}: {error.Description}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Ghi log lỗi và bỏ qua dòng bị lỗi
                            _logger.LogError($"Error processing row {row}: {ex.Message}");
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

    public async Task<IActionResult> ProductManagement()
    {
        var products = await _context.Products.Select(product => new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            ImageUrl = product.ImageUrl
        }).ToListAsync();

        return View("ProductManagement/Index", products);
    }

    [HttpGet]
    public async Task<IActionResult> UpdateProductView(int id)
    {
        // Find product by ID
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound($"Product with ID {id} not found.");
        }

        // Convert Product to ProductViewModel
        var productViewModel = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            ImageUrl = product.ImageUrl
        };

        // Return view with ProductViewModel
        return View("ProductManagement/Update", productViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProduct(ProductViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Find product by ID
            var product = await _context.Products.FindAsync(model.Id);
            if (product == null)
            {
                return NotFound($"Product with ID {model.Id} not found.");
            }

            // Update product information
            product.Name = model.Name;
            product.Price = model.Price;
            product.ImageUrl = model.ImageUrl;

            // Save changes
            await _context.SaveChangesAsync();
            return RedirectToAction("ProductManagement");
        }

        // If ModelState is invalid, return to the view with current data
        return View("ProductManagement/Update", model);
    }

    [HttpGet]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        // Find product by ID
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound($"Product with ID {id} not found.");
        }

        // Delete the product
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        // Redirect to ProductManagement after successful deletion
        return RedirectToAction("ProductManagement");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMultipleProducts(List<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            ModelState.AddModelError(string.Empty, "No products selected for deletion.");
            return RedirectToAction("ProductManagement");
        }

        var products = await _context.Products.Where(p => ids.Contains(p.Id)).ToListAsync();
        _context.Products.RemoveRange(products);
        await _context.SaveChangesAsync();

        // Redirect to ProductManagement after deletion
        return RedirectToAction("ProductManagement");
    }

    [HttpGet]
    public IActionResult CreateProductView()
    {
        // Return view to display product creation form
        return View("ProductManagement/Create");
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductViewModel model)
    {
        if (ModelState.IsValid)
        {
            string imageUrl = null;

            if (model.ImageFile != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
                var extension = Path.GetExtension(model.ImageFile.FileName);
                var newFileName = $"{fileName}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", newFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                imageUrl = $"/images/{newFileName}";
            }

            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                ImageUrl = imageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("ProductManagement");
        }

        return View("ProductManagement/Create", model);
    }

    [HttpPost]
    public async Task<IActionResult> ExportSelectedProducts(List<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            return BadRequest("No products selected for export.");
        }

        // Get list of products by IDs
        var products = await _context.Products
            .Where(product => ids.Contains(product.Id))
            .Select(product => new
            {
                product.Id,
                product.Name,
                product.Price,
                product.ImageUrl
            }).ToListAsync();

        // Create Excel file with ClosedXML
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Products");

            // Create column headers
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Price";
            worksheet.Cell(1, 4).Value = "Image URL";

            // Fill data into rows
            for (int i = 0; i < products.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = products[i].Id;
                worksheet.Cell(i + 2, 2).Value = products[i].Name;
                worksheet.Cell(i + 2, 3).Value = products[i].Price;
                worksheet.Cell(i + 2, 4).Value = products[i].ImageUrl;
            }

            // Save Excel file to memory
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                // Return Excel file
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> ImportProducts(IFormFile file)
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

                // Open Excel file with ClosedXML
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1); // Get first sheet
                    var rows = worksheet.RowsUsed();

                    foreach (var row in rows.Skip(1)) // Skip header row
                    {
                        try
                        {
                            var product = new Product
                            {
                                Name = row.Cell(2).GetValue<string>(),
                                Price = row.Cell(3).GetValue<decimal>(),
                                ImageUrl = row.Cell(4).GetValue<string>()
                            };

                            _context.Products.Add(product);
                        }
                        catch (Exception ex)
                        {
                            // Log error and skip problematic row
                            _logger.LogError($"Error processing row: {ex.Message}");
                            continue;
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("ProductManagement");
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred while importing products: {ex.Message}");
        }
    }

    public async Task<IActionResult> OrderManagement()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View("OrderManagement/Index", orders);
    }

    [HttpGet]
    public async Task<IActionResult> OrderDetails(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound($"Order with ID {id} not found.");
        }

        return View("OrderManagement/Details", order);
    }

    [HttpGet]
    public async Task<IActionResult> UpdateOrder(int id)
    {
        // Lấy thông tin đơn hàng theo ID
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound($"Order with ID {id} not found.");
        }

        // Tạo ViewModel để truyền dữ liệu đến view
        var orderViewModel = new OrderViewModel
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
        };

        return View("OrderManagement/Update", orderViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound($"Order with ID {id} not found.");
        }

        order.Status = status;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        return RedirectToAction("OrderManagement");
    }

    [HttpGet]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound($"Order with ID {id} not found.");
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return RedirectToAction("OrderManagement");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMultipleOrders(List<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            ModelState.AddModelError(string.Empty, "No orders selected for deletion.");
            return RedirectToAction("OrderManagement");
        }

        var orders = await _context.Orders
            .Where(o => ids.Contains(o.Id))
            .ToListAsync();

        _context.Orders.RemoveRange(orders);
        await _context.SaveChangesAsync();

        return RedirectToAction("OrderManagement");
    }

    [HttpPost]
    public async Task<IActionResult> ExportSelectedOrders(List<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            return BadRequest("No orders selected for export.");
        }

        var orders = await _context.Orders
            .Where(o => ids.Contains(o.Id))
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ToListAsync();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Orders");

            // Tạo tiêu đề cột
            worksheet.Cell(1, 1).Value = "Order ID";
            worksheet.Cell(1, 2).Value = "Order Date";
            worksheet.Cell(1, 3).Value = "Total Amount";
            worksheet.Cell(1, 4).Value = "Status";

            // Điền dữ liệu vào các hàng
            for (int i = 0; i < orders.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = orders[i].Id;
                worksheet.Cell(i + 2, 2).Value = orders[i].OrderDate.ToString("dd/MM/yyyy");
                worksheet.Cell(i + 2, 3).Value = orders[i].TotalAmount;
                worksheet.Cell(i + 2, 4).Value = orders[i].Status;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> ImportOrders(IFormFile file)
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

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed();

                    foreach (var row in rows.Skip(1)) // Bỏ qua dòng tiêu đề
                    {
                        try
                        {
                            var order = new Order
                            {
                                OrderDate = row.Cell(2).GetValue<DateTime>(),
                                TotalAmount = row.Cell(3).GetValue<decimal>(),
                                Status = row.Cell(4).GetValue<string>()
                            };

                            _context.Orders.Add(order);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing row: {ex.Message}");
                            continue;
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("OrderManagement");
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred while importing orders: {ex.Message}");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
