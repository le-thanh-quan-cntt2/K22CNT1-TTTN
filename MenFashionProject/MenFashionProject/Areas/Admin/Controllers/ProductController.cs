using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MenFashionProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace MenFashionProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly MenFashionContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductController(MenFashionContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // 1. Danh sách sản phẩm
        public IActionResult Index(int? categoryId)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductAttributes)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.CategoryId = categoryId;

            return View(products.ToList());
        }

        // 2. Tạo mới - Hiển thị Form
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // 3. Tạo mới - Xử lý lưu (Đã nâng cấp thêm ảnh phụ)
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile, List<IFormFile> extraImages)
        {
            if (ModelState.IsValid)
            {
                // A. Xử lý ảnh đại diện (Main Image)
                if (imageFile != null && imageFile.Length > 0)
                {
                    product.Image = await SaveFile(imageFile);
                }

                product.CreatedDate = DateTime.Now;
                _context.Add(product);
                await _context.SaveChangesAsync(); // Lưu để có ProductId trước

                // B. Xử lý ảnh phụ (Extra Images)
                if (extraImages != null && extraImages.Count > 0)
                {
                    foreach (var file in extraImages)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = await SaveFile(file);
                            var productImage = new ProductImage
                            {
                                ProductId = product.ProductId,
                                Url = fileName
                            };
                            _context.ProductImages.Add(productImage);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // 4. Sửa - Hiển thị Form (Mới)
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages) // Lấy kèm ảnh phụ
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // 5. Sửa - Lưu dữ liệu (Mới)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile, List<IFormFile> extraImages)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null) return NotFound();

                    // Cập nhật thông tin cơ bản
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.PriceSale = product.PriceSale;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.IsActive = product.IsActive;
                    existingProduct.IsHome = product.IsHome;

                    // Nếu có chọn ảnh đại diện mới thì thay thế
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        existingProduct.Image = await SaveFile(imageFile);
                    }

                    // Xử lý thêm ảnh phụ mới (nếu có)
                    if (extraImages != null && extraImages.Count > 0)
                    {
                        foreach (var file in extraImages)
                        {
                            if (file.Length > 0)
                            {
                                var fileName = await SaveFile(file);
                                var productImage = new ProductImage
                                {
                                    ProductId = existingProduct.ProductId,
                                    Url = fileName
                                };
                                _context.ProductImages.Add(productImage);
                            }
                        }
                    }

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.ProductId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // 6. Xóa ảnh phụ (Dùng trong trang Edit)
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var img = await _context.ProductImages.FindAsync(imageId);
            if (img != null)
            {
                _context.ProductImages.Remove(img);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // Hàm phụ trợ: Lưu file và trả về tên file
        private async Task<string> SaveFile(IFormFile file)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_environment.WebRootPath, "images", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }

        // 7. Xóa sản phẩm
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

        // 8. Quản lý thuộc tính (Giữ nguyên)
        public IActionResult Attributes(int id)
        {
            var product = _context.Products.Include(p => p.ProductAttributes).FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public IActionResult AddAttribute(int ProductId, string Size, string Color, int Quantity)
        {
            var attribute = new ProductAttribute { ProductId = ProductId, Size = Size, Color = Color, Quantity = Quantity };
            _context.ProductAttributes.Add(attribute);
            _context.SaveChanges();
            return RedirectToAction("Attributes", new { id = ProductId });
        }

        [HttpPost]
        public IActionResult DeleteAttribute(int id)
        {
            var attr = _context.ProductAttributes.Find(id);
            if (attr != null)
            {
                // Vì ProductId trong DB là INT NULL nên attr.ProductId là int?
                // Ta dùng ?? 0 để lấy giá trị 0 nếu nó bị null
                int pid = attr.ProductId ?? 0;

                _context.ProductAttributes.Remove(attr);
                _context.SaveChanges();

                // Quay lại trang danh sách
                return RedirectToAction("Attributes", new { id = pid });
            }
            return RedirectToAction("Index");
        }
    }
}