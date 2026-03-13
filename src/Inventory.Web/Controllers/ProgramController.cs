using Inventory.Domain.Entities;
using Inventory.Domain.ViewModels;
using Inventory.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Design;

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
                DesignNo = _db.Designs.Where(d => d.DesignId == x.DesignId).Select(d => d.DesignNo).FirstOrDefault()
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
            ViewBag.DesignList = new SelectList(_db.Designs, "DesignId", "DesignNo");

            var vPreviousProgram = await _db.Program.OrderByDescending(x => x.ProgramId).FirstOrDefaultAsync();
            int vNextProgramNo = 1;

            if (vPreviousProgram != null)
            {
                var vPreviousProgramNo = int.Parse(vPreviousProgram.ProgramNo.Substring(1));
                vNextProgramNo = vPreviousProgramNo + 1;
            }

            var vModel = new ProgramVM
            {
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

                    Matchings = program.ProgramMatchings.Select(m => new ProgramMatchingVM
                    {
                        ProgramMatchingId = m.ProgramMatchingId,
                        ProgramId = m.ProgramId,
                        DesignId = m.DesignId,
                        DesignMatchingId = m.DesignMatchingId,
                        PlateId = m.PlateId,
                        MatchingNo = m.MatchingNo,
                        Colour = m.Colour,
                    }).ToList()
                };

                ViewBag.PartyList = new SelectList(_db.Party, "PartyId", "PartyName", program.PartyId);
                ViewBag.DesignList = new SelectList(_db.Designs, "DesignId", "DesignNo", program.DesignId);

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

            if (!ModelState.IsValid)
            {
                ViewBag.PartyList = new SelectList(_db.Party, "PartyId", "PartyName");
                ViewBag.DesignList = new SelectList(_db.Designs, "DesignId", "DesignNo");
                return View("AddEdit", model);
            }

            if(model.ProgramId == 0)
            {
                var program = new ProgramEntry
                {
                    ProgramNo = model.ProgramNo,
                    PartyId = model.PartyId,
                    Quality = model.Quality,
                    Date = DateOnly.FromDateTime(DateTime.Today),
                    MainCut = model.MainCut,
                    Fold = model.Fold,
                    Finishing = model.Finishing,
                    Quantity = model.Quantity,
                    Remarks = model.Remarks,
                    Round = model.Round,
                    Rate = model.Rate,
                    DesignId = model.DesignId,
                    ProgramMatchings = model.SelectedMatchingIds.Select(vMatchingId => new ProgramMatching
                    {
                        ProgramId = model.ProgramId,
                        DesignMatchingId = vMatchingId,
                        DesignId = model.DesignId,
                        PlateId = _db.DesignMatchings.Where(dm => dm.DesignMatchingId == vMatchingId).Select(dm => dm.DesignPlateId).FirstOrDefault(),
                        MatchingNo = _db.DesignMatchings.Where(dm => dm.DesignMatchingId == vMatchingId).Select(dm => dm.MatchingNo).FirstOrDefault(),
                        Colour = _db.DesignMatchings.Where(dm => dm.DesignMatchingId == vMatchingId).Select(dm => dm.Colour).FirstOrDefault(),
                    }).ToList()

                };

                await _db.Program.AddAsync(program);
            }
            else
            {
                var program = await _db.Program.Include(x => x.ProgramMatchings).FirstOrDefaultAsync(x => x.ProgramId == model.ProgramId);
                
                if (program == null)
                {
                    return NotFound();
                }
                program.ProgramNo = model.ProgramNo;
                program.PartyId = model.PartyId;
                program.Quality = model.Quality;
                program.Date = model.Date;
                program.MainCut = model.MainCut;
                program.Fold = model.Fold;
                program.Finishing = model.Finishing;
                program.Quantity = model.Quantity;
                program.Remarks = model.Remarks;
                program.Round = model.Round;
                program.Rate = model.Rate;
                program.DesignId = model.DesignId;
            }


            return RedirectToAction("Index");

        }
        #endregion



        #region GetMatchingByDesign
        [HttpGet]
        public async Task<IActionResult> GetMatchingByDesign(int designId)
        {
            var data = await _db.DesignPlates
                .Where(p => p.DesignId == designId)
                .SelectMany(p => p.DesignMatchings.Select(m => new
                {
                    m.DesignMatchingId,
                    m.MatchingNo,
                    PlateName = p.PlateName
                })).ToListAsync();

            return Json(data);
        }
        #endregion
    }
}
