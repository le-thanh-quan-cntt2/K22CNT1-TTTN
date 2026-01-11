using MenFashionProject.Helpers;
using MenFashionProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MenFashionProject.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly MenFashionContext _context;
        const string CART_KEY = "MYCART";

        public CheckoutController(MenFashionContext context)
        {
            _context = context;
        }

        // --- HÀM TÍNH % GIẢM GIÁ ---
        private int GetVipDiscount(int? userId)
        {
            if (userId == null) return 0;

            // Tính tổng tiền các đơn đã hoàn thành (Status = 4)
            var totalSpent = _context.Orders
                .Where(o => o.UserId == userId && o.Status == 4)
                .Sum(o => o.TotalAmount ?? 0);

            if (totalSpent >= 10000000) return 15; // Diamond
            if (totalSpent >= 5000000) return 10;  // Gold
            if (totalSpent >= 2000000) return 5;   // Silver
            return 0; // Member
        }

        // 1. HIỂN THỊ FORM THANH TOÁN
        public IActionResult Index(int? productId, int? quantity, string? size, string? color)
        {
            List<CartItem> cart = new();

            // TRƯỜNG HỢP: MUA NGAY (Không qua giỏ)
            if (productId.HasValue && quantity.HasValue)
            {
                var product = _context.Products.Find(productId.Value);
                if (product != null)
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName ?? "",
                        Image = product.Image ?? "",
                        Quantity = quantity.Value,
                        Size = size ?? "",
                        Color = color ?? "",

                        // 🔥 ĐÃ SỬA: Xóa GetValueOrDefault()
                        Price = product.PriceSale.HasValue && product.PriceSale.Value > 0
                                ? product.PriceSale.Value
                                : product.Price
                    });
                    ViewBag.IsBuyNow = true;
                }
            }
            else
            {
                // TRƯỜNG HỢP: LẤY TỪ GIỎ HÀNG SESSION
                cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            }

            if (cart.Count == 0) return RedirectToAction("Index", "Home");

            // TÍNH TOÁN TIỀN HÀNG
            decimal subTotal = cart.Sum(x => x.Total); // Tổng tiền hàng chưa giảm
            int discountPercent = 0;

            // LẤY THÔNG TIN KHÁCH HÀNG & TÍNH GIẢM GIÁ
            if (User.Identity.IsAuthenticated)
            {
                var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user != null)
                {
                    ViewBag.User = user;
                    discountPercent = GetVipDiscount(user.UserId);
                }
            }

            // TÍNH RA SỐ TIỀN GIẢM VÀ TỔNG CẦN TRẢ
            decimal discountAmount = subTotal * discountPercent / 100;
            decimal grandTotal = subTotal - discountAmount;

            ViewBag.Cart = cart;
            ViewBag.SubTotal = subTotal;         // Tạm tính
            ViewBag.DiscountPercent = discountPercent; // % Giảm
            ViewBag.DiscountAmount = discountAmount;   // Tiền giảm
            ViewBag.GrandTotal = grandTotal;     // Phải trả

            return View();
        }

        // 2. XỬ LÝ ĐẶT HÀNG (POST)
        [HttpPost]
        public IActionResult ProcessCheckout(
            string customerName, string phone, string address, string email,
            int? productId, int? quantity, string? size, string? color)
        {
            List<CartItem> cart = new();

            // TÁI TẠO GIỎ HÀNG (Để tính lại tiền ở Server - Bảo mật)
            if (productId.HasValue && quantity.HasValue)
            {
                var product = _context.Products.Find(productId.Value);
                if (product != null)
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName ?? "",
                        Image = product.Image ?? "",
                        Quantity = quantity.Value,
                        Size = size ?? "",
                        Color = color ?? "",

                        // 🔥 ĐÃ SỬA: Xóa GetValueOrDefault()
                        Price = product.PriceSale.HasValue && product.PriceSale.Value > 0
                                ? product.PriceSale.Value
                                : product.Price
                    });
                }
            }
            else
            {
                cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            }

            if (cart.Count == 0) return RedirectToAction("Index", "Home");

            // XỬ LÝ USER & TÍNH LẠI GIÁ
            int? userId = null;
            decimal subTotal = cart.Sum(x => x.Total);
            int discountPercent = 0;

            if (User.Identity.IsAuthenticated)
            {
                var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user != null)
                {
                    userId = user.UserId;

                    // Cập nhật thông tin mới nhất của khách
                    user.Address = address;
                    user.Phone = phone;
                    user.Email = email; // Cập nhật cả email nếu có
                    _context.Users.Update(user);
                    _context.SaveChanges();

                    discountPercent = GetVipDiscount(userId);
                }
            }

            decimal discountAmount = subTotal * discountPercent / 100;
            decimal finalTotal = subTotal - discountAmount;

            // TẠO ĐƠN HÀNG (ORDER)
            var order = new Order
            {
                Code = "DH" + DateTime.Now.Ticks,
                UserId = userId,
                CustomerName = customerName ?? "",
                Phone = phone ?? "",
                Address = address ?? "",
                TotalAmount = finalTotal, // Lưu số tiền sau khi đã trừ khuyến mãi
                CreatedDate = DateTime.Now,
                Status = 1, // 1: Mới đặt
                PaymentMethod = 1 // COD
            };

            _context.Orders.Add(order);
            _context.SaveChanges(); // Lưu để lấy OrderId

            // TẠO CHI TIẾT ĐƠN HÀNG (ORDER DETAILS) & TRỪ TỒN KHO
            foreach (var item in cart)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Size = item.Size,
                    Color = item.Color
                });

                // Trừ tồn kho trong bảng ProductAttributes
                var attr = _context.ProductAttributes.FirstOrDefault(p =>
                    p.ProductId == item.ProductId &&
                    p.Size == item.Size &&
                    p.Color == item.Color
                );

                // Kiểm tra null và số lượng trước khi trừ
                if (attr != null && attr.Quantity >= item.Quantity)
                {
                    attr.Quantity -= item.Quantity;
                }
            }
            _context.SaveChanges();

            // Nếu mua từ giỏ hàng thì xóa giỏ sau khi đặt xong
            if (!productId.HasValue) HttpContext.Session.Remove(CART_KEY);

            return RedirectToAction("Success");
        }

        public IActionResult Success() => View();
    }
}