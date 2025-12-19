using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenFashionProject.Models;

namespace MenFashionProject.Controllers
{
    public class ProductController : Controller
    {
        private readonly MenFashionContext _context;

        public ProductController(MenFashionContext context)
        {
            _context = context;
        }

        // Trang danh sách sản phẩm (Mặc định)
        public IActionResult Index()
        {
            return View();
        }

        // Trang chi tiết: /Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0) return NotFound();

            // 1. Lấy thông tin sản phẩm theo ID
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            // 2. Lấy danh sách Size và Màu riêng lẻ để hiển thị button chọn
            // (Dùng ViewBag để truyền sang View cho nhanh gọn)
            ViewBag.Sizes = _context.ProductAttributes
                .Where(p => p.ProductId == id)
                .Select(x => x.Size).Distinct().OrderBy(x => x).ToList();

            ViewBag.Colors = _context.ProductAttributes
                .Where(p => p.ProductId == id)
                .Select(x => x.Color).Distinct().ToList();

            // 3. Lấy danh sách ảnh chi tiết
            ViewBag.Gallery = _context.ProductImages
                .Where(p => p.ProductId == id)
                .ToList();

            // 4. Sản phẩm liên quan (Cùng danh mục)
            ViewBag.RelatedProducts = _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id)
                .Take(4).ToList();

            return View(product);
        }
    }
}