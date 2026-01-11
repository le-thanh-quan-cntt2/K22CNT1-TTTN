using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenFashionProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace MenFashionProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly MenFashionContext _context;

        public CustomerController(MenFashionContext context)
        {
            _context = context;
        }

        // 1. Danh sách khách hàng
        public IActionResult Index()
        {
            // Chỉ lấy Role = 0 (Khách hàng), không lấy Admin
            var customers = _context.Users
                .Where(u => u.Role == 0)
                .OrderByDescending(u => u.CreatedDate)
                .ToList();
            return View(customers);
        }

        // 2. Xem chi tiết & Lịch sử mua hàng
        public IActionResult Details(int id)
        {
            // Lấy thông tin khách + Kèm theo danh sách Đơn hàng họ đã mua
            var customer = _context.Users
                .Include(u => u.Orders)
                .FirstOrDefault(u => u.UserId == id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // 3. Xóa tài khoản khách hàng
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}