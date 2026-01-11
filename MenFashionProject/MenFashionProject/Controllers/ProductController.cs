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

        // 1. Trang danh sách sản phẩm (Có tìm kiếm & Lọc)
        public IActionResult Index(string searchString, decimal? minPrice, decimal? maxPrice, int? categoryId, int? page)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive == true);

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.Contains(searchString));
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => (p.PriceSale > 0 ? p.PriceSale : p.Price) >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => (p.PriceSale > 0 ? p.PriceSale : p.Price) <= maxPrice.Value);
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            ViewBag.Categories = _context.Categories.Where(c => c.ParentId != null).ToList();

            ViewBag.SearchString = searchString;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.CategoryId = categoryId;

            int pageSize = 12;
            int pageNumber = page ?? 1;
            var totalItems = products.Count();

            var data = products
                .OrderByDescending(p => p.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(data);
        }

        // Trang chi tiết: /Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            ViewBag.Sizes = _context.ProductAttributes
                .Where(p => p.ProductId == id)
                .Select(x => x.Size).Distinct().OrderBy(x => x).ToList();

            ViewBag.Colors = _context.ProductAttributes
                .Where(p => p.ProductId == id)
                .Select(x => x.Color).Distinct().ToList();

            ViewBag.Gallery = _context.ProductImages
                .Where(p => p.ProductId == id)
                .ToList();

            ViewBag.RelatedProducts = _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id)
                .Take(4).ToList();

            ViewBag.Attributes = _context.ProductAttributes
                .Where(p => p.ProductId == id)
                .ToList();

            // --- PHẦN ĐÁNH GIÁ ---
            ViewBag.Reviews = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            var ratings = _context.Reviews.Where(r => r.ProductId == id).Select(r => r.Rating).ToList();
            ViewBag.RatingCount = ratings.Count;
            ViewBag.RatingAverage = ratings.Count > 0 ? ratings.Average() : 0;

            // [MỚI] KIỂM TRA QUYỀN ĐÁNH GIÁ (Để ẩn/hiện form bên View)
            bool canReview = false;
            if (User.Identity.IsAuthenticated)
            {
                var userEmail = User.Identity.Name;
                var user = _context.Users.FirstOrDefault(u => u.UserName == userEmail);
                if (user != null)
                {
                    // 1. Đếm số đơn hàng ĐÃ MUA & HOÀN THÀNH chứa sản phẩm này
                    int countBought = _context.Orders
                        .Count(o => o.UserId == user.UserId
                                    && o.Status == 4
                                    && o.OrderDetails.Any(d => d.ProductId == id));

                    // 2. Đếm số lần ĐÃ REVIEW sản phẩm này
                    int countReviewed = _context.Reviews
                        .Count(r => r.UserId == user.UserId && r.ProductId == id);

                    // 3. Nếu số lần mua > số lần review => Được phép đánh giá tiếp
                    canReview = countBought > countReviewed;
                }
            }
            // Truyền biến này sang View để quyết định hiển thị Form
            ViewBag.CanReview = canReview;

            return View(product);
        }

        // 2. Xử lý đánh giá (POST) - Đã cập nhật logic đếm lượt
        [HttpPost]
        public IActionResult AddReview(int ProductId, int Rating, string Comment)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Product/Details/{ProductId}" });
            }

            var userEmail = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.UserName == userEmail);

            if (user == null) return RedirectToAction("Login", "Account");

            // 1. Đếm số lần mua (đơn hoàn thành)
            int countBought = _context.Orders
                .Count(o => o.UserId == user.UserId
                            && o.Status == 4
                            && o.OrderDetails.Any(d => d.ProductId == ProductId));

            if (countBought == 0)
            {
                TempData["ErrorReview"] = "Bạn cần mua và hoàn thành đơn hàng để đánh giá sản phẩm này.";
                return RedirectToAction("Details", new { id = ProductId });
            }

            // 2. Đếm số lần đã review
            int countReviewed = _context.Reviews
                .Count(r => r.UserId == user.UserId && r.ProductId == ProductId);

            // 3. Kiểm tra: Hết lượt chưa?
            if (countReviewed >= countBought)
            {
                TempData["ErrorReview"] = $"Bạn đã dùng hết {countReviewed} lượt đánh giá cho {countBought} lần mua. Hãy mua thêm để tiếp tục đánh giá.";
                return RedirectToAction("Details", new { id = ProductId });
            }

            // 4. Lưu đánh giá
            var review = new Review
            {
                ProductId = ProductId,
                UserId = user.UserId,
                Rating = Rating,
                Comment = Comment,
                CreatedDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            TempData["SuccessReview"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            return RedirectToAction("Details", new { id = ProductId });
        }

        public IActionResult Discount()
        {
            var saleProducts = _context.Products
                .Include(p => p.Category)
                .Where(p => p.PriceSale > 0 && p.PriceSale < p.Price)
                .OrderByDescending(p => p.PriceSale)
                .ToList();

            return View(saleProducts);
        }
    }
}