using Inventory.Domain.Entities;
using Inventory.Domain.ViewModels;
using Inventory.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Web.Controllers
{
    public class ProgramController : Controller
    {
        #region DI
        private readonly ApplicationDbContext _db;
        public ProgramController(ApplicationDbContext db)
        {
            _db = db;
        }
        #endregion


        #region Index
        public async Task<IActionResult> Index(int? PartyId, int page = 1, int pageSize = 10)
        {
            var query = _db.Program.Select(x => new ProgramVM
            {
                ProgramId = x.ProgramId,
                ProgramNo = x.ProgramNo,
                PartyId = x.PartyId,
                PartyName = _db.Party.Where(p => p.PartyId == x.PartyId).Select(p => p.PartyName).FirstOrDefault(),
                DesignNo = _db.Designs.Where(d => d.DesignId == x.DesignId).Select(d => d.DesignNo).FirstOrDefault(),
                TotalMatchings = _db.ProgramMatchings.Where(m => m.ProgramId == x.ProgramId).GroupBy(x => x.MatchingNo).Count(),
                Quality = x.Quality,
                Date = x.Date,
                MainCut = x.MainCut,
                Fold = x.Fold,
                Finishing = x.Finishing,
                Quantity = x.Quantity,
                Remarks = x.Remarks,
                Round = x.Round,
                Rate = x.Rate,
                DesignId = x.DesignId,
            });

            if (PartyId != null)
            {
                query = query.Where(x => x.PartyId == PartyId);
            }

            var total = await query.CountAsync();

            var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            var model = new PaginatedList<ProgramVM>(items, total, page, pageSize);

            ViewBag.PageSize = pageSize;

            ViewBag.PartyList = new SelectList(
                   _db.Party, "PartyId", "PartyName", PartyId
                );

            return View(model);
        }
        #endregion


        #region AddEdit
        public async Task<IActionResult> AddEdit(int? id)
        {
            ViewBag.Action = "Add";
            ViewBag.PartyList = new SelectList(_db.Party, "PartyId", "PartyName");
            ViewBag.DesignList = _db.Designs;

            var vPreviousProgram = await _db.Program.OrderByDescending(x => x.ProgramId).FirstOrDefaultAsync();
            int vNextProgramNo = 1;

            if (vPreviousProgram != null)
            {
                var vPreviousProgramNo = int.Parse(vPreviousProgram.ProgramNo.Substring(1));
                vNextProgramNo = vPreviousProgramNo + 1;
            }

            var vModel = new ProgramVM
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                ProgramNo = $"A{vNextProgramNo}"
            };

            if (id != null)
            {
                ViewBag.Action = "Edit";

                var program = await _db.Program.Include(x => x.ProgramMatchings).FirstOrDefaultAsync(x => x.ProgramId == id);

                var item = new ProgramVM
                {
                    ProgramId = program.ProgramId,
                    ProgramNo = program.ProgramNo,
                    PartyId = program.PartyId,
                    Quality = program.Quality,
                    Date = program.Date,
                    MainCut = program.MainCut,
                    Fold = program.Fold,
                    Finishing = program.Finishing,
                    Quantity = program.Quantity,
                    Remarks = program.Remarks,
                    Round = program.Round,
                    Rate = program.Rate,
                    DesignId = program.DesignId,
                    SelectedMatchingNo = program.ProgramMatchings.Select(x => x.MatchingNo).Distinct().ToList()
                };

                ViewBag.PartyList = new SelectList(_db.Party, "PartyId", "PartyName", program.PartyId);
                //ViewBag.DesignList = new SelectList(_db.Designs, "DesignId", "DesignNo", program.DesignId);

                if (item == null)
                    return NotFound();

                return View("AddEdit", item);
            }

            return View("AddEdit", vModel);
        }
        #endregion


        #region Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ProgramVM model)
        {
            // Validate matchings
            if (model.SelectedMatchingNo == null || !model.SelectedMatchingNo.Any())
                ModelState.AddModelError(nameof(model.SelectedMatchingNo), "Please select at least one matching.");

            if (!ModelState.IsValid)
            {
                ViewBag.PartyList = new SelectList(_db.Party, "PartyId", "PartyName", model.PartyId);
                ViewBag.DesignList = new SelectList(_db.Designs, "DesignId", "DesignNo", model.DesignId);
                return View("AddEdit", model);
            }

            //var vDesignMatchingList = await _db.DesignMatchings
            //    .Where(m => model.SelectedMatchingNo!.Contains(m.MatchingNo))
            //    .ToListAsync();

            if (model.ProgramId == 0)
            {
                var program = new ProgramEntry
                {
                    ProgramNo = model.ProgramNo,
                    PartyId = model.PartyId,
                    Quality = model.Quality,
                    Date = model.Date,
                    MainCut = model.MainCut,
                    Fold = model.Fold,
                    Finishing = model.Finishing,
                    Quantity = model.Quantity,
                    Remarks = model.Remarks,
                    Round = model.Round,
                    Rate = model.Rate,
                    DesignId = model.DesignId
                };

                var vDesignMatchingList = await _db.DesignPlates.Where(x => x.DesignId == model.DesignId)
                        .SelectMany(x => x.DesignMatchings).Where(m => model.SelectedMatchingNo!.Contains(m.MatchingNo))
                        .ToListAsync();

                await _db.Program.AddAsync(program);
                await _db.SaveChangesAsync();

                foreach (var vMatching in vDesignMatchingList)
                {
                    program.ProgramMatchings.Add(new ProgramMatching
                    {
                        ProgramId = program.ProgramId,
                        DesignId = model.DesignId,
                        DesignMatchingId = vMatching.DesignMatchingId,
                        PlateId = vMatching.DesignPlateId,
                        MatchingNo = vMatching.MatchingNo,
                        Colour = vMatching.Colour
                    });
                }
            }
            else
            {
                var vProgram = await _db.Program
                    .Include(x => x.ProgramMatchings)
                    .FirstOrDefaultAsync(x => x.ProgramId == model.ProgramId);

                if (vProgram == null)
                    return NotFound();

                vProgram.ProgramNo = model.ProgramNo;
                vProgram.PartyId = model.PartyId;
                vProgram.Quality = model.Quality;
                vProgram.Date = model.Date;
                vProgram.MainCut = model.MainCut;
                vProgram.Fold = model.Fold;
                vProgram.Finishing = model.Finishing;
                vProgram.Quantity = model.Quantity;
                vProgram.Remarks = model.Remarks;
                vProgram.Round = model.Round;
                vProgram.Rate = model.Rate;
                vProgram.DesignId = model.DesignId;

                var vDesignMatchingList = await _db.DesignPlates.Where(x => x.DesignId == model.DesignId)
                        .SelectMany(x => x.DesignMatchings).Where(m => model.SelectedMatchingNo!.Contains(m.MatchingNo))
                        .ToListAsync();

                _db.ProgramMatchings.RemoveRange(vProgram.ProgramMatchings);

                foreach (var vMatching in vDesignMatchingList)
                {
                    vProgram.ProgramMatchings.Add(new ProgramMatching
                    {
                        ProgramId = vProgram.ProgramId,
                        DesignId = model.DesignId,
                        DesignMatchingId = vMatching.DesignMatchingId,
                        PlateId = vMatching.DesignPlateId,
                        MatchingNo = vMatching.MatchingNo,
                        Colour = vMatching.Colour
                    });
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        #endregion


        #region Delete
        public async Task<IActionResult> Delete(int id)
        {
            var program = await _db.Program.FindAsync(id);

            if (program == null)
                return NotFound();

            foreach (var matching in program.ProgramMatchings)
            {
                _db.ProgramMatchings.Remove(matching);
            }

            _db.Program.Remove(program);

            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        #endregion


        #region Print
        public async Task<IActionResult> Print(int ProgramId,string PrintView)
        {
            var vModel = await _db.Program.Where(x => x.ProgramId == ProgramId)
                .Select(x => new ProgramVM
                {
                    ProgramId = x.ProgramId,
                    ProgramNo = x.ProgramNo,
                    PartyName = _db.Party.Where(p => p.PartyId == x.PartyId).Select(p => p.PartyName).FirstOrDefault(),
                    DesignNo = _db.Designs.Where(d => d.DesignId == x.DesignId).Select(d => d.DesignNo).FirstOrDefault(),
                    Quality = x.Quality,
                    Date = x.Date,
                    MainCut = x.MainCut,
                    Fold = x.Fold,
                    Finishing = x.Finishing,
                    Quantity = x.Quantity,
                    Remarks = x.Remarks,
                    Round = x.Round,
                    Rate = x.Rate,
                }).FirstOrDefaultAsync();

            ViewBag.MatchingList = await _db.ProgramMatchings.Where(m => m.ProgramId == ProgramId)
                .Select(m => new ProgramMatchingVM
                {
                    MatchingNo = m.MatchingNo,
                    Colour = m.Colour,
                    DesignMatchingId = m.DesignMatchingId,
                    PlateId = m.PlateId,
                    PlateName = _db.DesignPlates.Where(p => p.DesignPlateId == m.PlateId).Select(p => p.PlateName).FirstOrDefault(),
                    DesignId = m.DesignId
                }).ToListAsync();


            if(PrintView == "DesignMatching")
            {
                return View("Print_DesignMatching", vModel);
            }
            else
            {
                return View("Print_Program", vModel);
            }
        }
        #endregion


        #region GetMatchingByDesign
        [HttpGet]
        public async Task<IActionResult> GetMatchingByDesign(int designId)
        {
            var data = _db.DesignPlates.Where(x => x.DesignId == designId)
                    .SelectMany(x => x.DesignMatchings).GroupBy(x => x.MatchingNo).Select(x => x.First()).ToList();

            return Json(data);
        }
        #endregion
    }
}
