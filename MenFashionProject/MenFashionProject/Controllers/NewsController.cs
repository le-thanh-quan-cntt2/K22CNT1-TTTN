using Microsoft.AspNetCore.Mvc;
using MenFashionProject.Models;

namespace MenFashionProject.Controllers
{
    public class NewsController : Controller
    {
        private readonly MenFashionContext _context;

        public NewsController(MenFashionContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var news = _context.News.OrderByDescending(n => n.CreatedDate).ToList();
            return View(news);
        }

        public IActionResult Details(int id)
        {
            var item = _context.News.Find(id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}