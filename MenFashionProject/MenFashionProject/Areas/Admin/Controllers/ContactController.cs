using Microsoft.AspNetCore.Mvc;
using MenFashionProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace MenFashionProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được vào xem
    public class ContactController : Controller
    {
        private readonly MenFashionContext _context;

        public ContactController(MenFashionContext context)
        {
            _context = context;
        }

        // 1. Danh sách tin nhắn liên hệ
        public IActionResult Index()
        {
            var items = _context.Contacts
                .OrderByDescending(x => x.CreatedDate) // Tin mới nhất lên đầu
                .ToList();
            return View(items);
        }

        // 2. Xóa tin nhắn (nếu là spam)
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var item = _context.Contacts.Find(id);
            if (item != null)
            {
                _context.Contacts.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}