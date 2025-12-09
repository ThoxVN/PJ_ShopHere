using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Data.Entities;

namespace ShopHerePJ.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {

        public async Task<IActionResult> Index()
        {
            return View();
        }

    
    }
}
