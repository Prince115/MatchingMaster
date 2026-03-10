using Inventory.Domain.Entities;
using Inventory.Domain.ViewModels;
using Inventory.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Web.Controllers
{
    public class DesignController : Controller
    {
        #region DI
        private readonly ApplicationDbContext _db;
        public DesignController(ApplicationDbContext db)
        {
            _db = db;
        }
        #endregion


        #region Index
        public async Task<IActionResult> Index(int? PartyId, int page = 1, int pageSize = 10)
        {
            var query = _db.Designs.Select(x => new MatchingVM
            {
                DesignId = x.DesignId,
                DesignNo = x.DesignNo,
                Quality = x.Quality,
                Date = x.Date,
                PartyId = x.PartyId,
                PartyName = _db.Party.Where(p => p.PartyId == x.PartyId).Select(p => p.PartyName).FirstOrDefault(),
            });

            if (PartyId != null)
            {
                query = query.Where(x => x.PartyId == PartyId);
            }

            var total = await query.CountAsync();

            var items = await query
            .OrderByDescending(x => x.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            var model = new PaginatedList<MatchingVM>(items, total, page, pageSize);

            ViewBag.PageSize = pageSize;
            ViewBag.PartyList = new SelectList(
                   _db.Party, "PartyId", "PartyName", PartyId
                );

            return View(model);
        }
        #endregion


        #region AddEdit
        public async Task<IActionResult> AddEdit(int DesignId = 0)
        {
            ViewBag.PartyList = new SelectList(_db.Party, "PartyId", "PartyName");

            // ADD Mode 
            if (DesignId == 0)
            {
                return View("AddEdit", new MatchingVM());
            }
            else
            {
                // EDIT Mode
                var vDesign = await _db.Designs
                .Include(x => x.DesignPlates)
                    .ThenInclude(x => x.DesignMatchings)
                .FirstOrDefaultAsync(x => x.DesignId == DesignId);

                if (vDesign == null)
                    return NotFound();

                // Re-build party list with selected value for edit
                ViewBag.PartyList = new SelectList(_db.Party, "PartyId", "PartyName", vDesign.PartyId);

                var vModel = new MatchingVM
                {
                    DesignId = vDesign.DesignId,
                    DesignNo = vDesign.DesignNo,
                    Quality = vDesign.Quality,
                    Date = vDesign.Date,
                    PartyId = vDesign.PartyId,
                    Plates = vDesign.DesignPlates.Count(),
                    Matching = vDesign.DesignPlates.FirstOrDefault()?.DesignMatchings.Count() ?? 0,

                    Rows = vDesign.DesignPlates
                    .Select(p => new PlateRow
                    {
                        DesignPlateId = p.DesignPlateId,
                        PlateName = p.PlateName,
                        PlateNo = p.PlateNo,
                        Matchings = p.DesignMatchings
                            .OrderBy(m => m.MatchingNo)
                            .Select(m => new MatchingCell
                            {
                                DesignMatchingId = m.DesignMatchingId,
                                MatchingNo = m.MatchingNo,
                                Colour = m.Colour,
                                Quantity = m.Quantity
                            }).ToList()
                    }).ToList()
                };

                return View("AddEdit", vModel);
            }
        }
        #endregion


        #region Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(MatchingVM vModel)
        {
            // ── Remove model state errors for dynamic plate/matching fields not posted by user
            var ignoredKeys = ModelState.Keys.Where(k => k.Contains("DesignPlateId")
                 || k.Contains("DesignMatchingId")
                 || k.Contains("MatchingNo")
                 || k.Contains("PlateNo")).ToList();

            foreach (var key in ignoredKeys)
                ModelState.Remove(key);

            if (!ModelState.IsValid)
            {
                ViewBag.PartyList = new SelectList(_db.Party, "PartyId", "PartyName", vModel.PartyId);
                return View("AddEdit", vModel);
            }

            
            if (vModel.DesignId == 0)
            {
                var vDesign = new Design
                {
                    DesignNo = vModel.DesignNo,
                    Quality = vModel.Quality,
                    Date = vModel.Date,
                    PartyId = vModel.PartyId
                };

                foreach (var row in vModel.Rows)
                {
                    var plate = new DesignPlate
                    {
                        PlateName = row.PlateName,
                        PlateNo = row.PlateNo
                    };

                    int matchNo = 1;
                    foreach (var cell in row.Matchings)
                    {
                        plate.DesignMatchings.Add(new DesignMatching
                        {
                            MatchingNo = matchNo++,
                            Colour = cell.Colour,
                            Quantity = cell.Quantity
                        });
                    }

                    vDesign.DesignPlates.Add(plate);
                }

                await _db.Designs.AddAsync(vDesign);
            }
            else
            {
                var vDesign = await _db.Designs
                    .Include(x => x.DesignPlates)
                        .ThenInclude(x => x.DesignMatchings)
                    .FirstOrDefaultAsync(x => x.DesignId == vModel.DesignId);

                if (vDesign == null)
                    return NotFound();

                vDesign.DesignNo = vModel.DesignNo;
                vDesign.Quality = vModel.Quality;
                vDesign.Date = vModel.Date;
                vDesign.PartyId = vModel.PartyId;

                foreach (var row in vModel.Rows)
                {
                    var plate = vDesign.DesignPlates
                        .FirstOrDefault(p => p.DesignPlateId == row.DesignPlateId);

                    if (plate == null) continue; // safety check

                    plate.PlateName = row.PlateName;
                    plate.PlateNo = row.PlateNo;
                   
                    foreach (var cell in row.Matchings)
                    {
                        var matching = plate.DesignMatchings
                            .FirstOrDefault(m => m.DesignMatchingId == cell.DesignMatchingId);

                        if (matching == null) continue; // safety check

                        matching.Colour = cell.Colour;
                        matching.Quantity = cell.Quantity;
                    }
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion
    }
}
