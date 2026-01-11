using Microsoft.AspNetCore.Mvc;
using MenFashionProject.Models;

namespace MenFashionProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NewsController : Controller
    {
        private readonly MenFashionContext _context;

        public NewsController(MenFashionContext context)
        {
            _context = context;
        }

        // 1. Danh sách tin tức
        public IActionResult Index()
        {
            var list = _context.News.OrderByDescending(n => n.CreatedDate).ToList();
            return View(list);
        }

        // 2. Giao diện Thêm mới
        public IActionResult Create()
        {
            return View();
        }

        // 3. Xử lý Thêm mới (Có Upload ảnh)
        [HttpPost]
        public async Task<IActionResult> Create(News model, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Lấy tên file
                    var fileName = Path.GetFileName(imageFile.FileName);
                    // Đường dẫn lưu ảnh: wwwroot/images/
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    // Copy ảnh vào thư mục
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Lưu tên ảnh vào database
                    model.Image = fileName;
                }

                model.CreatedDate = DateTime.Now; // Lấy ngày giờ hiện tại
                _context.News.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View(model);
        }

        // 4. Xóa tin tức
        [HttpGet] // Dùng GET cho đơn giản, hoặc dùng POST + Form như sản phẩm
        public IActionResult Delete(int id)
        {
            var news = _context.News.Find(id);
            if (news != null)
            {
                _context.News.Remove(news);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}