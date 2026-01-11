using Microsoft.AspNetCore.Mvc;
using MenFashionProject.Models;
using MenFashionProject.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace MenFashionProject.Controllers
{
    public class CartController : Controller
    {
        private readonly MenFashionContext _context;
        private const string CART_KEY = "MYCART";
        private const string VOUCHER_KEY = "MY_VOUCHER";


        public CartController(MenFashionContext context)
        {
            _context = context;
        }

        // =========================
        // 1. HIỂN THỊ GIỎ HÀNG
        // =========================
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY)
                       ?? new List<CartItem>();

            return View(cart);
        }

        // =========================
        // 2. THÊM VÀO GIỎ
        // =========================
        public IActionResult AddToCart(int id, int quantity, string type, string size, string color)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY)
                       ?? new List<CartItem>();

            // Kiểm tra tồn kho theo biến thể
            var stock = _context.ProductAttributes.FirstOrDefault(p =>
                p.ProductId == id &&
                p.Size == size &&
                p.Color == color
            );

            if (stock == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại biến thể này";
                return RedirectToAction("Details", "Product", new { id });
            }

            int currentStock = stock.Quantity ?? 0;

            // Số lượng đã có trong giỏ
            int existQty = cart
                .Where(p => p.ProductId == id && p.Size == size && p.Color == color)
                .Sum(p => p.Quantity);

            if (existQty + quantity > currentStock)
            {
                TempData["Error"] = $"Chỉ còn {currentStock - existQty} sản phẩm trong kho";
                return RedirectToAction("Details", "Product", new { id });
            }

            // Tìm item trùng
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

        // =========================
        // 3. CẬP NHẬT SỐ LƯỢNG
        // =========================
        public IActionResult UpdateQuantity(int id, string size, string color, int change)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY)
                        ?? new List<CartItem>();

            var item = cart.SingleOrDefault(p =>
                p.ProductId == id &&
                p.Size == size &&
                p.Color == color
            );

            if (item == null)
                return RedirectToAction("Index");

            // Tăng / giảm
            int newQty = item.Quantity + change;

            if (newQty <= 0)
            {
                cart.Remove(item);
            }
            else
            {
                // Kiểm tra tồn kho
                var stock = _context.ProductAttributes.FirstOrDefault(p =>
                    p.ProductId == id &&
                    p.Size == size &&
                    p.Color == color
                );

                int maxQty = stock?.Quantity ?? 0;

                if (newQty > maxQty)
                {
                    TempData["Error"] = $"Chỉ còn {maxQty} sản phẩm trong kho";
                    return RedirectToAction("Index");
                }

                item.Quantity = newQty;
            }

            HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
            return RedirectToAction("Index");
        }


        // =========================
        // 4. XÓA SẢN PHẨM
        // =========================
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

        // =========================
        // 5. TẠO CART ITEM
        // =========================
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
                Price = product.PriceSale.HasValue && product.PriceSale.Value > 0
                        ? product.PriceSale.Value
                        : product.Price
            };
        }

    }
}
