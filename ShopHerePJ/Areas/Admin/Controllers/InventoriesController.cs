using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Areas.Admin.Models;
using ShopHerePJ.Data.Entities;

namespace ShopHerePJ.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InventoriesController : Controller
    {
        private readonly ShopHereContext _context;
        public InventoriesController(ShopHereContext context) => _context = context;

        // GET: /Admin/Inventories?variantId=10
        public async Task<IActionResult> Index(int? variantId)
        {
            ViewBag.VariantId = variantId;

            IQueryable<inventory> q = _context.inventories
                .Include(i => i.variant).ThenInclude(v => v.product)
                .AsNoTracking();

            if (variantId.HasValue)
                q = q.Where(x => x.variant_id == variantId.Value);

            var data = await q.OrderByDescending(x => x.updated_at).ToListAsync();
            return View(data);
        }

        // POST: /Admin/Inventories/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string search, int? variantId)
        {
            ViewBag.Search = search;
            ViewBag.VariantId = variantId;

            IQueryable<inventory> q = _context.inventories
                .Include(i => i.variant).ThenInclude(v => v.product)
                .AsNoTracking();

            if (variantId.HasValue)
                q = q.Where(x => x.variant_id == variantId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                q = q.Where(i =>
                    i.variant.sku.Contains(search) ||
                    i.variant.size.Contains(search) ||
                    i.variant.color.Contains(search) ||
                    i.variant.product.name.Contains(search) ||
                    i.variant.product.sku.Contains(search)
                );
            }

            var data = await q.OrderByDescending(x => x.updated_at).ToListAsync();
            return View("Index", data);
        }

        // GET: /Admin/Inventories/Create
        public async Task<IActionResult> Create(int? variantId)
        {
            await LoadVariantDropdown(variantId);

            return View(new InventoryUpsertVM
            {
                VariantId = variantId ?? 0,
                QtyOnHand = 0,
                QtyReserved = 0
            });
        }

        // POST: /Admin/Inventories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryUpsertVM vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadVariantDropdown(vm.VariantId);
                return View(vm);
            }

            // 1 variant chỉ nên có 1 inventory (theo model variant.inventory)
            var existed = await _context.inventories.FirstOrDefaultAsync(x => x.variant_id == vm.VariantId);
            if (existed != null)
            {
                // nếu đã có -> chuyển qua Edit
                return RedirectToAction(nameof(Edit), new { id = existed.id });
            }

            var entity = new inventory
            {
                variant_id = vm.VariantId,
                qty_on_hand = vm.QtyOnHand,
                qty_reserved = vm.QtyReserved,
                updated_at = DateTime.Now
            };

            _context.inventories.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Inventories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.inventories
                .Include(i => i.variant).ThenInclude(v => v.product)
                .FirstOrDefaultAsync(i => i.id == id);

            if (entity == null) return NotFound();

            await LoadVariantDropdown(entity.variant_id);

            var vm = new InventoryUpsertVM
            {
                Id = entity.id,
                VariantId = entity.variant_id,
                QtyOnHand = entity.qty_on_hand,
                QtyReserved = entity.qty_reserved
            };

            return View(vm);
        }

        // POST: /Admin/Inventories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventoryUpsertVM vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadVariantDropdown(vm.VariantId);
                return View(vm);
            }

            var entity = await _context.inventories.FirstOrDefaultAsync(i => i.id == id);
            if (entity == null) return NotFound();

            // optional: chặn đổi VariantId sang variant đã có inventory khác
            if (entity.variant_id != vm.VariantId)
            {
                var existed = await _context.inventories
                    .AnyAsync(x => x.variant_id == vm.VariantId && x.id != id);
                if (existed) return BadRequest("Variant này đã có inventory.");
            }

            entity.variant_id = vm.VariantId;
            entity.qty_on_hand = vm.QtyOnHand;
            entity.qty_reserved = vm.QtyReserved;
            entity.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Inventories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.inventories
                .Include(i => i.variant).ThenInclude(v => v.product)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.id == id);

            if (entity == null) return NotFound();
            return View(entity);
        }

        // POST: /Admin/Inventories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.inventories.FirstOrDefaultAsync(i => i.id == id);
            if (entity == null) return NotFound();

            _context.inventories.Remove(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadVariantDropdown(int? selectedVariantId = null)
        {
            var variants = await _context.product_variants
                .Include(v => v.product)
                .AsNoTracking()
                .OrderBy(v => v.product.name)
                .ThenBy(v => v.sku)
                .Select(v => new
                {
                    v.id,
                    Label = v.product.name + " | " + v.sku + " | " + v.size + " / " + v.color
                })
                .ToListAsync();

            ViewBag.VariantSelect = new SelectList(variants, "id", "Label", selectedVariantId);
        }
    }
}
