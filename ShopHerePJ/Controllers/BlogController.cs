using Microsoft.AspNetCore.Mvc;

namespace ShopHerePJ.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index() => View();

        // demo, sau này lấy DB theo id
        public IActionResult Details(int id)
        {
            ViewBag.Id = id;
            return View();
        }
    }
}
