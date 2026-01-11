using Microsoft.AspNetCore.Mvc;
using MenFashionProject.Models;

namespace MenFashionProject.Controllers
{
    public class ContactController : Controller
    {
        private readonly MenFashionContext _context;

        public ContactController(MenFashionContext context)
        {
            _context = context;
        }

        // 1. Hiển thị Form Liên hệ
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // 2. Xử lý khi khách bấm Gửi
        [HttpPost]
        public async Task<IActionResult> Send(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.CreatedDate = DateTime.Now;
                contact.IsRead = false;

                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất.";
                return RedirectToAction("Index");
            }

            return View("Index", contact);
        }
    }
}