using Microsoft.AspNetCore.Mvc;
using MenFashionProject.Models;
using MenFashionProject.Helpers;

namespace MenFashionProject.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly MenFashionContext _context;
        const string CART_KEY = "MYCART";

        public CheckoutController(MenFashionContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ FORM THANH TOÁN
        public IActionResult Index(int? productId, int? quantity, string? size, string? color)
        {
            List<CartItem> cart = new();

            // MUA NGAY
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

                        // 🔥 FIX decimal?
                        Price = product.PriceSale.HasValue && product.PriceSale.Value > 0
                                ? product.PriceSale.Value
                                : product.Price.GetValueOrDefault()
                    });

                    ViewBag.IsBuyNow = true;
                }
            }
            else
            {
                // LẤY TỪ GIỎ HÀNG
                cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY)
                       ?? new List<CartItem>();
            }

            if (cart.Count == 0)
                return RedirectToAction("Index", "Home");

            ViewBag.Cart = cart;
            ViewBag.GrandTotal = cart.Sum(x => x.Total);

            return View();
        }

        // 2. XỬ LÝ ĐẶT HÀNG
        [HttpPost]
        public IActionResult ProcessCheckout(
            string customerName,
            string phone,
            string address,
            string email,
            int? productId,
            int? quantity,
            string? size,
            string? color)
        {
            List<CartItem> cart = new();

            // MUA NGAY
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

                       
                        Price = product.PriceSale.HasValue && product.PriceSale.Value > 0
                                ? product.PriceSale.Value
                                : product.Price.GetValueOrDefault()
                    });
                }
            }
            else
            {
                cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY)
                       ?? new List<CartItem>();
            }

            if (cart.Count == 0)
                return RedirectToAction("Index", "Home");

            // TẠO ĐƠN HÀNG
            var order = new Order
            {
                Code = "DH" + DateTime.Now.Ticks,
                CustomerName = customerName ?? "",
                Phone = phone ?? "",
                Address = address ?? "",
                TotalAmount = cart.Sum(x => x.Total),
                CreatedDate = DateTime.Now,
                Status = 1,
                PaymentMethod = 1
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // CHI TIẾT ĐƠN HÀNG
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
            }

            _context.SaveChanges();

            // XÓA GIỎ HÀNG NẾU KHÔNG PHẢI MUA NGAY
            if (!productId.HasValue)
            {
                HttpContext.Session.Remove(CART_KEY);
            }

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
