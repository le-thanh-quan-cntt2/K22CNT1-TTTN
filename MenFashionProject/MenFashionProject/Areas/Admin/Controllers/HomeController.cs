using Microsoft.AspNetCore.Mvc;
using MenFashionProject.Models;

using Microsoft.AspNetCore.Authorization;

namespace MenFashionProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly MenFashionContext _context;

        public HomeController(MenFashionContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Thống kê cơ bản
            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalOrders = _context.Orders.Count();
            ViewBag.TotalRevenue = _context.Orders.Sum(o => o.TotalAmount); // Tổng doanh thu

            // Lấy 5 đơn hàng mới nhất
            var newOrders = _context.Orders.OrderByDescending(o => o.CreatedDate).Take(5).ToList();

            return View(newOrders);
        }
    }
}