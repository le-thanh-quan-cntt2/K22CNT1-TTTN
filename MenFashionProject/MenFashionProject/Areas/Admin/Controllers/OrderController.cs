using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenFashionProject.Models;
using Microsoft.AspNetCore.Authorization; 

namespace MenFashionProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] 
    public class OrderController : Controller
    {
        private readonly MenFashionContext _context;

        public OrderController(MenFashionContext context)
        {
            _context = context;
        }

        // 1. Danh sách đơn hàng
        public IActionResult Index()
        {
            var orders = _context.Orders.OrderByDescending(o => o.CreatedDate).ToList();
            return View(orders);
        }

        // 2. Xem chi tiết đơn hàng
        public IActionResult ViewOrder(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id);
            var details = _context.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.OrderId == id)
                .ToList();

            ViewBag.OrderDetails = details;
            return View(order);
        }

        // 3. Cập nhật trạng thái đơn hàng
        [HttpPost]
        public IActionResult UpdateStatus(int id, int status)
        {
            var order = _context.Orders.Find(id);
            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges(); // Lưu trạng thái mới vào CSDL
            }
            // Load lại trang chi tiết để thấy thay đổi
            return RedirectToAction("ViewOrder", new { id = id });
        }
        // 4. Xóa đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo mật, chống giả mạo request
        public IActionResult Delete(int id)
        {
            // Tìm đơn hàng và bao gồm cả chi tiết (OrderDetails)
            var order = _context.Orders
                .Include(o => o.OrderDetails) // Quan trọng: nạp chi tiết để xóa
                .FirstOrDefault(o => o.OrderId == id);

            if (order != null)
            {
                // 1. Xóa chi tiết đơn hàng trước (Tránh lỗi khóa ngoại FK)
                _context.OrderDetails.RemoveRange(order.OrderDetails);

                // 2. Xóa đơn hàng chính
                _context.Orders.Remove(order);

                // 3. Lưu thay đổi
                _context.SaveChanges();
            }

            // Xóa xong quay về danh sách
            return RedirectToAction("Index");
        }
    }
}