using Microsoft.AspNetCore.Mvc;
using MenFashionProject.Models;

namespace MenFashionProject.ViewComponents
{
    public class MenuCategoryViewComponent : ViewComponent
    {
        private readonly MenFashionContext _context;

        public MenuCategoryViewComponent(MenFashionContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            // Lấy tất cả danh mục ra để xử lý ở View
            var items = _context.Categories.OrderBy(x => x.CategoryName).ToList();
            return View(items);
        }
    }
}