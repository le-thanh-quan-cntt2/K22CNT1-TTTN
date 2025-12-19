using Microsoft.AspNetCore.Mvc;
using MenFashionProject.Models;
using MenFashionProject.Helpers;

namespace MenFashionProject.Controllers
{
    public class CartController : Controller
    {
        private readonly MenFashionContext _context;
        const string CART_KEY = "MYCART";

        public CartController(MenFashionContext context)
        {
            _context = context;
        }

        // 1. Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY)
                       ?? new List<CartItem>();
            return View(cart);
        }

        // 2. Thêm vào giỏ
        public IActionResult AddToCart(int id, int quantity, string type, string size, string color)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY)
                       ?? new List<CartItem>();

            // Tìm sản phẩm trùng ID + SIZE + MÀU
            var item = cart.SingleOrDefault(p =>
                p.ProductId == id &&
                p.Size == size &&
                p.Color == color
            );

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                var product = _context.Products.Find(id);
                if (product == null) return NotFound();

                cart.Add(CreateCartItem(quantity, size, color, product));
            }

            HttpContext.Session.SetObjectAsJson(CART_KEY, cart);

            if (type == "BuyNow")
                return RedirectToAction("Index", "Checkout");

            return RedirectToAction("Index");
        }

        // Hàm tạo CartItem (FIX TRIỆT ĐỂ decimal?)
        private static CartItem CreateCartItem(int quantity, string size, string color, Product product)
        {
            return new CartItem
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName ?? "",
                Image = product.Image ?? "",
                Quantity = quantity,
                Size = size ?? "",
                Color = color ?? "",

                // 🔥 FIX QUAN TRỌNG: decimal? → decimal
                Price = product.PriceSale.HasValue && product.PriceSale.Value > 0
                        ? product.PriceSale.Value
                        : product.Price.GetValueOrDefault()
            };
        }

        // 3. Xóa sản phẩm khỏi giỏ
        public IActionResult Remove(int id, string size, string color)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY)
                       ?? new List<CartItem>();

            var item = cart.SingleOrDefault(p =>
                p.ProductId == id &&
                p.Size == size &&
                p.Color == color
            );

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
            }

            return RedirectToAction("Index");
        }
    }
}
