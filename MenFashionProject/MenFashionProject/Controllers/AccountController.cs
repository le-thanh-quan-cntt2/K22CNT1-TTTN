using Microsoft.AspNetCore.Mvc;
using MenFashionProject.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
        public IActionResult Login()
        {
            return View();
        }

        // 4. XỬ LÝ ĐĂNG NHẬP (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
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

                // Nếu là Admin thì chuyển vào trang Admin
                if (user.Role == 1)
                {
                    return Redirect("/Admin/Home");
                }
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
    }
}