using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Data.Entities;

namespace ShopHerePJ.Controllers
{
    public class MediaController : Controller
    {
        private readonly ShopHereContext _context;
        public MediaController(ShopHereContext context) => _context = context;

        // /media/image/123
        [HttpGet("/media/image/{id:int}")]
        public async Task<IActionResult> Image(int id)
        {
            var img = await _context.images.AsNoTracking().FirstOrDefaultAsync(x => x.id == id);
            if (img == null) return NotFound();
            return File(img.data, img.content_type);
        }
    }
}
