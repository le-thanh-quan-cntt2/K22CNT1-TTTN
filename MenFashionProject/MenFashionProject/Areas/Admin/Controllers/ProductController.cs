using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MenFashionProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace MenFashionProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // <--- Dòng này khóa cửa, chỉ Admin mới được vào
    public class ProductController : Controller
    {
        private readonly MenFashionContext _context;
        private readonly IWebHostEnvironment _environment; // Dùng để lấy đường dẫn lưu ảnh

        public ProductController(MenFashionContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // 1. Danh sách sản phẩm
        public IActionResult Index()
        {
            // Include thêm ProductAttributes để tính tổng tồn kho
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductAttributes) 
                .OrderByDescending(p => p.ProductId)
                .ToList();
            return View(products);
        }
        // 2. Tạo mới - Hiển thị Form
        public IActionResult Create()
        {
            // Load danh sách Category vào Dropdown
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // 3. Tạo mới - Xử lý lưu dữ liệu
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Tạo tên file độc nhất để tránh trùng (VD: anh-sp_23923923.jpg)
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var filePath = Path.Combine(_environment.WebRootPath, "images", fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    product.Image = fileName; // Lưu tên ảnh vào DB
                }

                product.CreatedDate = DateTime.Now;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // 4. Xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 1. Trang quản lý thuộc tính (Size/Màu) của 1 sản phẩm
        public IActionResult Attributes(int id)
        {
            var product = _context.Products
                .Include(p => p.ProductAttributes)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null) return NotFound();

            return View(product); // Truyền model là Product (đã kèm Attributes)
        }

        // 2. Xử lý thêm thuộc tính mới
        [HttpPost]
        public IActionResult AddAttribute(int ProductId, string Size, string Color, int Quantity)
        {
            var attribute = new ProductAttribute
            {
                ProductId = ProductId,
                Size = Size,
                Color = Color,
                Quantity = Quantity
            };

            _context.ProductAttributes.Add(attribute);
            _context.SaveChanges();

            // Quay lại trang quản lý thuộc tính của sản phẩm đó
            return RedirectToAction("Attributes", new { id = ProductId });
        }

        // 3. Xóa thuộc tính
        [HttpPost]
        public IActionResult DeleteAttribute(int id)
        {
            var attr = _context.ProductAttributes.Find(id);
            if (attr != null)
            {
                var productId = attr.ProductId;
                _context.ProductAttributes.Remove(attr);
                _context.SaveChanges();
                return RedirectToAction("Attributes", new { id = productId });
            }
            return RedirectToAction("Index");
        }
    }
}