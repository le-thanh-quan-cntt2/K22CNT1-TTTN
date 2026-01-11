using Microsoft.AspNetCore.Mvc;

namespace MenFashionProject.Controllers
{
    public class PolicyController : Controller
    {
        // 1. Chính sách đổi trả
        public IActionResult Return()
        {
            return View();
        }

        // 2. Chính sách bảo mật
        public IActionResult Privacy()
        {
            return View();
        }

        // 3. Hướng dẫn mua hàng
        public IActionResult Guide()
        {
            return View();
        }

        // 4. Kiểm tra đơn hàng (Hướng dẫn)
        public IActionResult CheckOrder()
        {
            return View();
        }

        // 5. Câu hỏi thường gặp (FAQ)
        public IActionResult FAQ()
        {
            return View();
        }

        // 6. Chính sách vận chuyển
        public IActionResult Shipping()
        {
            return View();
        }

        // 7. Thành viên VIP
        public IActionResult Member()
        {
            return View();
        }
    }
}