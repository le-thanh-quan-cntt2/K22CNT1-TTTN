using MenFashionProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MenFashionProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly MenFashionContext _context;

        public HomeController(MenFashionContext context)
        {
            _context = context;
        }

        // Action Index (Trang chủ & Tìm kiếm)
        public IActionResult Index(string searchString, decimal? minPrice, decimal? maxPrice, int? categoryId, int? page)
        {
            // 1. Tạo query cơ bản (chưa thực thi)
            // Lưu ý: Chỉ lấy sản phẩm Active và không bị xóa (nếu có cờ IsDeleted)
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages) // Include thêm ảnh phụ để hiển thị hover
                .Where(p => p.IsActive == true);

            // 2. Xử lý tìm kiếm (Nếu có)
            if (!string.IsNullOrEmpty(searchString))
            {
                // Tìm theo tên sản phẩm (không phân biệt hoa thường)
                products = products.Where(p => p.ProductName.Contains(searchString));
            }

            // 3. Lọc theo giá (Min - Max)
            if (minPrice.HasValue)
            {
                // Ưu tiên so sánh với giá Sale nếu có, không thì giá gốc
                products = products.Where(p => (p.PriceSale > 0 ? p.PriceSale : p.Price) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => (p.PriceSale > 0 ? p.PriceSale : p.Price) <= maxPrice.Value);
            }

            // 4. Lọc theo danh mục
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // 5. Chuẩn bị dữ liệu hiển thị (ViewBag)
            // Lấy danh sách danh mục để đổ vào Dropdown lọc
            ViewBag.Categories = _context.Categories.ToList();

            // Lưu lại các tham số tìm kiếm để giữ giá trị trên Form khi reload lại trang
            ViewBag.SearchString = searchString;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.CategoryId = categoryId;

            // 6. Phân trang (Pagination)
            int pageSize = 12; // Số sản phẩm trên 1 trang (Nên chia hết cho 2, 3, 4 để đẹp trên mọi màn hình)
            int pageNumber = page ?? 1; // Nếu page null thì mặc định là trang 1

            int totalItems = products.Count(); // Tổng số sản phẩm tìm được
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Lấy dữ liệu trang hiện tại
            var data = products
                .OrderByDescending(p => p.CreatedDate) // Mặc định sắp xếp mới nhất
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Truyền thông tin phân trang sang View
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(data);
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