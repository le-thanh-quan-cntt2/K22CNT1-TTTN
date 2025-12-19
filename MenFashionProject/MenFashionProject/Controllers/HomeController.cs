using MenFashionProject.Models; // Gọi namespace Models
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Để dùng hàm .Include()
using System.Diagnostics;

namespace MenFashionProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly MenFashionContext _context;

        // Inject Database vào Controller
        public HomeController(MenFashionContext context)
        {
            _context = context;
        }

        // 1. Action Index nhận thêm các tham số tìm kiếm
        public IActionResult Index(string searchString, decimal? minPrice, decimal? maxPrice, int? categoryId)
        {
            // Bắt đầu truy vấn (chưa chạy ngay)
            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive == true); // Chỉ lấy sp đang hoạt động

            // 2. Lọc theo tên (Nếu có nhập)
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.Contains(searchString));
            }

            // 3. Lọc theo giá thấp nhất (Min)
            if (minPrice.HasValue)
            {
                // Logic: Nếu có giá Sale (>0) thì dùng giá Sale, ngược lại dùng giá Gốc
                products = products.Where(p => (p.PriceSale > 0 ? p.PriceSale : p.Price) >= minPrice.Value);
            }

            // 4. Lọc theo giá cao nhất (Max)
            if (maxPrice.HasValue)
            {
                products = products.Where(p => (p.PriceSale > 0 ? p.PriceSale : p.Price) <= maxPrice.Value);
            }

            // 5. Lọc theo danh mục
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // Lấy danh sách danh mục để hiển thị vào Dropdown tìm kiếm
            ViewBag.Categories = _context.Categories.ToList();

            // Gửi lại các giá trị đã tìm để hiện lại trên form (UX)
            ViewBag.SearchString = searchString;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.CategoryId = categoryId;

            return View(products.OrderByDescending(p => p.CreatedDate).ToList());
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
}