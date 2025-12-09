    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using ShopHerePJ.Data.Entities;

    namespace ShopHerePJ.Areas.Admin.Controllers
    {
    [Area("Admin")]
    public class CategoriesController : Controller
    {

    private readonly ShopHereContext _context;
    public CategoriesController(ShopHereContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index()
    {
        var categories = await _context.categories
            .OrderBy(p => p.id)
            .ToListAsync();

        return View(categories);
    }

    [HttpPost]
    public IActionResult Search()
    {
        string search = Request.Form["search"].ToString();
        List<category> categories = _context.categories.Where(x => x.name.Contains(search)).ToList();
        return View(categories);
    }

    // =========================================
    // CREATE CATEGORY (HIỂN THỊ FORM)
    // GET: /Admin/Categories/Create
    // =========================================
    public async Task<IActionResult> Create()
    {
        // Load danh sách category cha
        ViewBag.parent_id = new SelectList(
            await _context.categories.OrderBy(c => c.name).ToListAsync(),
            "id",
            "name"
        );

        return View(new category
        {
            is_active = true
        });
    }

    // =========================================
    // CREATE CATEGORY (SUBMIT FORM)
    // POST: /Admin/Categories/Create
    // =========================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("name,slug,parent_id,is_active")] category model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.parent_id = new SelectList(_context.categories, "id", "name");
            return View(model);
        }

        model.created_at = DateTime.UtcNow;
        model.updated_at = DateTime.UtcNow;

        _context.categories.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

        // =========================================
        // DELETE CATEGORY - HIỂN THỊ MÀN CONFIRM
        // GET: /Admin/Categories/Delete/5
        // =========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Category không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // Lấy category kèm parent (để hiện thông tin trên view confirm)
            var category = await _context.categories
                .Include(c => c.parent)
                .FirstOrDefaultAsync(c => c.id == id.Value);

            if (category == null)
            {
                TempData["Error"] = "Category không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // CHỈ hiển thị view confirm, KHÔNG xóa ở đây
            return View(category);
        }

        // =========================================
        // DELETE CATEGORY - THỰC HIỆN XÓA
        // POST: /Admin/Categories/Delete/5
        // =========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.categories.FindAsync(id);

            if (category == null)
            {
                TempData["Error"] = "Category không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra category con
            bool hasChild = await _context.categories.AnyAsync(c => c.parent_id == id);
            if (hasChild)
            {
                TempData["Error"] = "Không thể xóa: Category đang có Category con.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra product dùng category này
            bool hasProducts = await _context.products.AnyAsync(p => p.category_id == id);
            if (hasProducts)
            {
                TempData["Error"] = "Không thể xóa: Category đang được dùng bởi sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            _context.categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa Category thành công!";
            return RedirectToAction(nameof(Index));
        }


        // EDIT - HIỂN THỊ FORM
        // GET: /Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.categories.FindAsync(id.Value);
            if (category == null) return NotFound();

            await PopulateParentDropDownList(category.parent_id, category.id);
            return View(category);
        }

        // EDIT - SUBMIT FORM
        // POST: /Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            // Không cho sửa updated_at trực tiếp, chỉ bind những field cần
            [Bind("id,name,slug,parent_id,is_active,created_at")] category model)
        {
            if (id != model.id) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateParentDropDownList(model.parent_id, model.id);
                return View(model);
            }

            try
            {
                // giữ nguyên created_at, chỉ update updated_at
                model.updated_at = DateTime.UtcNow;

                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.categories.Any(e => e.id == model.id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // Đổ dropdown Parent Category (tránh cho phép chọn chính nó làm parent)
        private async Task PopulateParentDropDownList(int? selectedParentId = null, int? excludeId = null)
        {
            var query = _context.categories.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.id != excludeId.Value);
            }

            var parents = await query
                .OrderBy(c => c.name)
                .ToListAsync();

            ViewBag.parent_id = new SelectList(parents, "id", "name", selectedParentId);
        }
    }
    }



