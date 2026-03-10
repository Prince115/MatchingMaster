namespace Inventory.Web.Controllers;
using Inventory.Domain.Entities;
using Inventory.Domain.ViewModels;
using Inventory.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class CatalogueController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public CatalogueController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    #region Index
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        // build base query
        var query = _db.Catalogues
            .Select(c => new CatalogueStockVM
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Packing = c.Packing,
                Remark = c.Remark,

                Stock = _db.InventoryTransactions
                    .Where(t => t.CatalogueId == c.Id)
                    .Sum(t => t.TransactionType == TransactionType.Stock
                            ? t.Quantity
                            : -t.Quantity)
            });

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Code.Contains(search, StringComparison.CurrentCultureIgnoreCase) || x.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new PaginatedList<CatalogueStockVM>(items, total, page, pageSize);

        ViewBag.Search = search;
        ViewBag.PageSize = pageSize;

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
            var item = await _db.Catalogues.FindAsync(id);

            if (item == null)
                return NotFound();

            return View("AddEdit", item);
        }

        return View("AddEdit", new Catalogue());
    }
    #endregion


    #region Save
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Catalogue model, IFormFile? photo, IFormFile? pdf)
    {
        if (!ModelState.IsValid)
            return View("Edit", model);

        #region Upload Photo & PDF
        var uploads = Path.Combine(_env.WebRootPath, "uploads", "catalogues");
        Directory.CreateDirectory(uploads);

        // PHOTO
        if (photo != null && photo.Length > 0)
        {
            var fileName = $"photo_{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using var fs = System.IO.File.Create(filePath);
            await photo.CopyToAsync(fs);

            model.PhotoFileName = fileName;
        }

        // PDF
        if (pdf != null && pdf.Length > 0)
        {
            var fileName = $"doc_{Guid.NewGuid()}{Path.GetExtension(pdf.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using var fs = System.IO.File.Create(filePath);
            await pdf.CopyToAsync(fs);

            model.PdfFileName = fileName;
        }
        #endregion

        if (model.Id == 0)
        {
            _db.Catalogues.Add(model);
            TempData["Success"] = "Product created successfully.";
        }
        else
        {
            var item = await _db.Catalogues.FindAsync(model.Id);
            if (item == null) return NotFound();

            item.Code = model.Code;
            item.Name = model.Name;
            item.Packing = model.Packing;
            item.Remark = model.Remark;

            if (!string.IsNullOrEmpty(model.PhotoFileName))
                item.PhotoFileName = model.PhotoFileName;

            if (!string.IsNullOrEmpty(model.PdfFileName))
                item.PdfFileName = model.PdfFileName;

            TempData["Success"] = "Product updated successfully.";
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    #endregion


    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Catalogue model, IFormFile? photo, IFormFile? pdf)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (photo != null && photo.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "catalogues");
            Directory.CreateDirectory(uploads);
            var ext = Path.GetExtension(photo.FileName);
            var fileName = $"photo_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, fileName);
            using (var fs = System.IO.File.Create(filePath))
            {
                await photo.CopyToAsync(fs);
            }
            model.PhotoFileName = fileName;
        }

        if (pdf != null && pdf.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "catalogues");
            Directory.CreateDirectory(uploads);
            var ext = Path.GetExtension(pdf.FileName);
            var fileName = $"doc_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, fileName);
            using (var fs = System.IO.File.Create(filePath))
            {
                await pdf.CopyToAsync(fs);
            }
            model.PdfFileName = fileName;
        }

        _db.Catalogues.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Product created successfully.";
        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Catalogues.FindAsync(id);
        if (item == null)
            return NotFound();

        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Catalogue model, IFormFile? photo, IFormFile? pdf)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var item = await _db.Catalogues.FindAsync(id);
        if (item == null) return NotFound();

        // update scalar properties
        item.Code = model.Code;
        item.Name = model.Name;
        item.Packing = model.Packing;
        item.Remark = model.Remark;

        if (photo != null && photo.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "catalogues");
            Directory.CreateDirectory(uploads);
            var ext = Path.GetExtension(photo.FileName);
            var fileName = $"photo_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, fileName);
            using (var fs = System.IO.File.Create(filePath))
            {
                await photo.CopyToAsync(fs);
            }
            item.PhotoFileName = fileName;
        }

        if (pdf != null && pdf.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "catalogues");
            Directory.CreateDirectory(uploads);
            var ext = Path.GetExtension(pdf.FileName);
            var fileName = $"doc_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, fileName);
            using (var fs = System.IO.File.Create(filePath))
            {
                await pdf.CopyToAsync(fs);
            }
            item.PdfFileName = fileName;
        }

        _db.Catalogues.Update(item);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Product updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Catalogues.FindAsync(id);
        if (item == null) return NotFound();

        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _db.Catalogues.FindAsync(id);

        if (item != null)
        {
            _db.Catalogues.Remove(item);
            await _db.SaveChangesAsync();
        }

        TempData["Success"] = "Product deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
