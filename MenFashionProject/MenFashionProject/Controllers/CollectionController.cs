using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenFashionProject.Models;

namespace MenFashionProject.Controllers
{
    public class CollectionController : Controller
    {
        private readonly MenFashionContext _context;

        public CollectionController(MenFashionContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy danh sách danh mục để làm bộ sưu tập
            var collections = _context.Categories.ToList();
            return View(collections);
        }
    }
}