using MenFashionProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MenFashionProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly MenFashionContext _context;

        public AccountController(MenFashionContext context)
        {
            _context = context;
        }

        // 1. ĐĂNG KÝ (GET)
        public IActionResult Register()
        {
            return View();
        }

        // 2. XỬ LÝ ĐĂNG KÝ (POST)
        [HttpPost]
        public IActionResult Register(User user)
        {
            // 1. Tự động gán UserName bằng Email (để không bị lỗi null)
            user.UserName = user.Email;

            // 2. Xóa lỗi validation của UserName (vì mình đã gán rồi)
            ModelState.Remove("UserName");

            if (ModelState.IsValid)
            {
                // Kiểm tra Email đã tồn tại chưa
                var check = _context.Users.FirstOrDefault(s => s.Email == user.Email);
                if (check == null)
                {
                    user.Role = 0; // 0: Khách hàng
                    user.CreatedDate = DateTime.Now;

                    _context.Users.Add(user);
                    _context.SaveChanges();

                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.Error = "Email này đã được sử dụng";
                }
            }
            return View();
        }

      
        // 3. ĐĂNG NHẬP (GET)
        public IActionResult Login(string? returnUrl) 
        {
            ViewBag.ReturnUrl = returnUrl; 
            return View();
        }

        // 4. XỬ LÝ ĐĂNG NHẬP (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl) // 1. Thêm tham số returnUrl
        {
            // Tìm user trong DB
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // Tạo thông tin định danh (Claim)
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role == 1 ? "Admin" : "Customer"),
            new Claim("FullName", user.FullName ?? "")
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Ghi nhận đăng nhập
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                // --- XỬ LÝ ĐIỀU HƯỚNG ---

                // Ưu tiên 1: Nếu là Admin thì luôn vào trang quản trị
                if (user.Role == 1)
                {
                    return Redirect("/Admin/Home");
                }

                // Ưu tiên 2: Nếu có ReturnUrl hợp lệ (ví dụ khách đang mua hàng dở)
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Mặc định: Về trang chủ
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Sai thông tin đăng nhập";
            return View();
        }

        // 5. ĐĂNG XUẤT
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        // 1. TRANG LỊCH SỬ & VIP
        public IActionResult History()
        {
            // Lấy email/username người dùng đang đăng nhập
            var userEmail = User.Identity.Name;
            if (userEmail == null) return RedirectToAction("Login");

            // Lấy thông tin User từ DB
            var user = _context.Users.FirstOrDefault(u => u.UserName == userEmail || u.Email == userEmail);
            if (user == null) return RedirectToAction("Login");

            // Lấy danh sách đơn hàng của User này
            var orders = _context.Orders
                .Where(o => o.UserId == user.UserId)
                .OrderByDescending(o => o.CreatedDate)
                .ToList();

            // --- TÍNH TOÁN VIP ---
            // Chỉ tính tổng tiền các đơn đã "Hoàn thành" (Status == 4)
            // Giả sử: 1=Mới, 2=Đóng gói, 3=Giao, 4=Hoàn thành, 5=Hủy
            var totalSpent = orders.Where(o => o.Status == 4).Sum(o => o.TotalAmount ?? 0);

            ViewBag.TotalSpent = totalSpent; // Tổng chi tiêu
            ViewBag.User = user;

            // Xếp hạng
            if (totalSpent >= 10000000) ViewBag.Rank = "Diamond"; // > 10 triệu
            else if (totalSpent >= 5000000) ViewBag.Rank = "Gold"; // > 5 triệu
            else if (totalSpent >= 2000000) ViewBag.Rank = "Silver"; // > 2 triệu
            else ViewBag.Rank = "Member";

            return View(orders);
        }

        // 2. XEM CHI TIẾT 1 ĐƠN HÀNG
        public IActionResult OrderDetails(int id)
        {
            var userEmail = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.UserName == userEmail);

            // Tìm đơn hàng (Phải đúng ID đơn và đúng User đó sở hữu)
            var order = _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderId == id && o.UserId == user.UserId);

            if (order == null) return NotFound();

            return View(order);
        }
        // 3. TRANG THÔNG TIN TÀI KHOẢN (PROFILE)
        public IActionResult Profile()
        {
            // Lấy User đang đăng nhập
            var userEmail = User.Identity.Name;
            if (userEmail == null) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.UserName == userEmail);
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        // 4. XỬ LÝ CẬP NHẬT THÔNG TIN
        [HttpPost]
        public IActionResult UpdateProfile(int UserId, string FullName, string Phone, string Address, string Email)
        {
            var user = _context.Users.Find(UserId);
            if (user != null)
            {
                user.FullName = FullName;
                user.Phone = Phone;
                user.Address = Address;
                user.Email = Email; // Cho phép sửa Email nếu muốn

                _context.Update(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }
            return RedirectToAction("Profile");
        }

        // [THÊM MỚI] 5. HỦY ĐƠN HÀNG
        [HttpPost]
        public IActionResult CancelOrder(int id)
        {
            // 1. Xác thực người dùng
            var userEmail = User.Identity.Name;
            if (userEmail == null) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.UserName == userEmail);
            if (user == null) return RedirectToAction("Login");

            // 2. Tìm đơn hàng (phải đúng ID và đúng chủ sở hữu)
            // Cần Include OrderDetails để biết sản phẩm nào mà hoàn kho
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == id && o.UserId == user.UserId);

            if (order == null) return NotFound();

            // 3. Kiểm tra điều kiện: Chỉ cho hủy khi Status = 1 (Mới / Chờ xác nhận)
            if (order.Status == 1)
            {
                // --- CẬP NHẬT TRẠNG THÁI ---
                order.Status = 5; // Giả sử 5 là "Đã hủy" (khớp với logic hiển thị badge đỏ trong View)

                // --- HOÀN LẠI TỒN KHO ---
                foreach (var item in order.OrderDetails)
                {
                    // Tìm biến thể sản phẩm (Size + Color) trong kho
                    var productAttr = _context.ProductAttributes.FirstOrDefault(p =>
                        p.ProductId == item.ProductId &&
                        p.Size == item.Size &&
                        p.Color == item.Color
                    );

                    // Nếu tìm thấy thì cộng lại số lượng
                    if (productAttr != null)
                    {
                        productAttr.Quantity += item.Quantity;
                    }
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Đơn hàng đã được xử lý, không thể hủy.";
            }

            // Quay lại trang chi tiết đơn hàng
            return RedirectToAction("OrderDetails", new { id = id });
        }
    }
}