using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Data.Entities;

namespace ShopHerePJ.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ImagesController : Controller
    {
        private readonly ShopHereContext _context;
        public ImagesController(ShopHereContext context) => _context = context;

        // GET: /Admin/Images/File?id=123
        [HttpGet]
        public async Task<IActionResult> File(int id)
        {
            var img = await _context.images.AsNoTracking().FirstOrDefaultAsync(x => x.id == id);
            if (img == null) return NotFound();
            return File(img.data, img.content_type);
        }

        // POST: /Admin/Images/UploadUserAvatar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadUserAvatar(int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return RedirectToAction("Edit", "Users", new { area = "Admin", id = userId });

            if (!file.ContentType.StartsWith("image/"))
                return BadRequest("Only image files are allowed.");

            // giới hạn size (tuỳ bạn)
            if (file.Length > 2 * 1024 * 1024)
                return BadRequest("Max 2MB.");

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            // nếu chỉ muốn 1 avatar chính: clear primary cũ
            var olds = await _context.images
                .Where(x => x.object_type == "user" && x.object_id == userId && x.is_primary)
                .ToListAsync();

            foreach (var o in olds) o.is_primary = false;

            var img = new image
            {
                object_type = "user",
                object_id = userId,
                file_name = file.FileName,
                content_type = file.ContentType,
                file_size = (int)file.Length,
                data = bytes,
                is_primary = true,
                created_at = DateTime.Now
            };

            _context.images.Add(img);
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", "Users", new { area = "Admin", id = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProductPrimary(int productId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return RedirectToAction("Edit", "Products", new { area = "Admin", id = productId });

            if (!file.ContentType.StartsWith("image/"))
                return BadRequest("Only image files are allowed.");

            if (file.Length > 2 * 1024 * 1024)
                return BadRequest("Max 2MB.");

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            // clear primary cũ
            var olds = await _context.images
                .Where(x => x.object_type == "product" && x.object_id == productId && x.is_primary)
                .ToListAsync();
            foreach (var o in olds) o.is_primary = false;

            var img = new image
            {
                object_type = "product",
                object_id = productId,
                file_name = file.FileName,
                content_type = file.ContentType,
                file_size = (int)file.Length,
                data = bytes,
                is_primary = true,
                created_at = DateTime.Now
            };

            _context.images.Add(img);
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", "Products", new { area = "Admin", id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadVariantPrimary(int variantId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return RedirectToAction("Edit", "ProductVariants", new { area = "Admin", id = variantId });

            if (!file.ContentType.StartsWith("image/"))
                return BadRequest("Only image files are allowed.");

            if (file.Length > 2 * 1024 * 1024)
                return BadRequest("Max 2MB.");

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            var olds = await _context.images
                .Where(x => x.object_type == "product_variant" && x.object_id == variantId && x.is_primary)
                .ToListAsync();
            foreach (var o in olds) o.is_primary = false;

            var img = new image
            {
                object_type = "product_variant",
                object_id = variantId,
                file_name = file.FileName,
                content_type = file.ContentType,
                file_size = (int)file.Length,
                data = bytes,
                is_primary = true,
                created_at = DateTime.Now
            };

            _context.images.Add(img);
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", "ProductVariants", new { area = "Admin", id = variantId });
        }

    }
}
