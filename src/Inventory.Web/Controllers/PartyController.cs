using Inventory.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory.Domain.Entities;

namespace Inventory.Web.Controllers
{
    public class PartyController : Controller
    {
        #region DI
        private readonly ApplicationDbContext _db;
        public PartyController(ApplicationDbContext db)
        {
            _db = db;
        }
        #endregion


        #region Index
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var query = _db.Party.AsQueryable();
            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new Domain.ViewModels.PaginatedList<Party>(items, total, page, pageSize);

            return View(model);
        }
        #endregion


        #region AddEdit
        public async Task<IActionResult> AddEdit(int? id)
        {
            ViewBag.Action = "Create";

            if (id != null)
            {
                ViewBag.Action = "Edit";
                var item = await _db.Party.FindAsync(id);

                if (item == null)
                    return NotFound();

                return View("AddEdit", item);
            }

            return View("AddEdit", new Party());
        }
        #endregion


        #region Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Party model)
        {
            if (!ModelState.IsValid)
            {
                return View("AddEdit", model);
            }

            if (model.PartyId == 0)
            {
                _db.Party.Add(model);
                TempData["Success"] = "Party created successfully.";
            }
            else
            {
                var item = await _db.Party.FindAsync(model.PartyId);
                if (item == null) return NotFound();

                item.PartyName = model.PartyName;
                item.City = model.City;
                item.State = model.State;
                item.Remarks = model.Remarks;

                TempData["Success"] = "Party updated successfully.";
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        #endregion


        #region Delete
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Party.FindAsync(id);
            if (item == null) return NotFound();

            _db.Party.Remove(item);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Party deleted successfully.";
            return RedirectToAction("Index");
        }
        #endregion
    }
}
