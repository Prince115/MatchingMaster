using Inventory.Domain.Entities;
using Inventory.Domain.ViewModels;
using Inventory.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Web.Controllers
{
    [Authorize]
    public class ProgramController : Controller
    {
        #region DI
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        public ProgramController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }
        #endregion


        #region Index
        public async Task<IActionResult> Index(int? PartyId, int page = 1, int pageSize = 20)
        {
            // Base query (no projection) to allow efficient count and paging
            var baseQuery = _db.Program.AsNoTracking();
            if (PartyId != null)
                baseQuery = baseQuery.Where(x => x.PartyId == PartyId);

            var total = await baseQuery.CountAsync();

            // Fetch a page of programs (lightweight projection)
            var programs = await baseQuery
                .OrderByDescending(x => x.ProgramId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.ProgramId,
                    x.ProgramNo,
                    x.PartyId,
                    x.Quality,
                    x.Date,
                    x.MainCut,
                    x.Fold,
                    x.Finishing,
                    x.Quantity,
                    x.Remarks,
                    x.Round,
                    x.Rate
                })
                .ToListAsync();

            var programIds = programs.Select(p => p.ProgramId).ToList();

            // Load related data in a few targeted queries to avoid per-row subqueries
            var partyIds = programs.Select(p => p.PartyId).Distinct().ToList();
            var party = _db.Party.AsNoTracking();
            var parties = await party
                .Where(p => partyIds.Contains(p.PartyId))
                .ToDictionaryAsync(p => p.PartyId, p => p.PartyName);

            var programMatchings = await _db.ProgramMatchings.AsNoTracking()
                .Where(pm => programIds.Contains(pm.ProgramId))
                .ToListAsync();

            var totalMatchingsMap = programMatchings
                .GroupBy(pm => pm.ProgramId)
                .ToDictionary(g => g.Key, g => g.Select(pm => pm.MatchingNo).Distinct().Count());

            var designIdsPerProgram = programMatchings
                .GroupBy(pm => pm.ProgramId)
                .ToDictionary(g => g.Key, g => g.Select(pm => pm.DesignId).Distinct().ToList());

            var allDesignIds = designIdsPerProgram.Values.SelectMany(x => x).Distinct().ToList();
            var designs = new Dictionary<int, int>();
            if (allDesignIds.Any())
            {
                designs = await _db.Designs.AsNoTracking()
                    .Where(d => allDesignIds.Contains(d.DesignId))
                    .ToDictionaryAsync(d => d.DesignId, d => d.DesignNo ?? 0);
            }

            var items = programs.Select(p => new ProgramVM
            {
                ProgramId = p.ProgramId,
                ProgramNo = p.ProgramNo,
                PartyId = p.PartyId,
                PartyName = parties.TryGetValue(p.PartyId, out var pn) ? pn : string.Empty,
                DesignNoCSV = designIdsPerProgram.TryGetValue(p.ProgramId, out var dids) ? string.Join(", ", dids.Select(id => designs.TryGetValue(id, out var dn) ? dn.ToString() : id.ToString())) : string.Empty,
                TotalMatchings = totalMatchingsMap.TryGetValue(p.ProgramId, out var tm) ? tm : 0,
                Quality = p.Quality,
                Date = p.Date,
                MainCut = p.MainCut,
                Fold = p.Fold,
                Finishing = p.Finishing,
                Quantity = p.Quantity,
                Remarks = p.Remarks,
                Round = p.Round,
                Rate = p.Rate,
            }).ToList();

            var model = new PaginatedList<ProgramVM>(items, total, page, pageSize);

            ViewBag.PageSize = pageSize;
            // Load party list once (no tracking)
            var partyList = await party.OrderBy(p => p.PartyName).ToListAsync();
            ViewBag.PartyList = new SelectList(partyList, "PartyId", "PartyName", PartyId);

            return View(model);
        }
        #endregion


        #region AddEdit
        public async Task<IActionResult> AddEdit(int? id)
        {
            ViewBag.Action = "Add";
            // Load lookup lists with no tracking (read-only)
            var partyList = await _db.Party.AsNoTracking().OrderBy(p => p.PartyName).ToListAsync();
            ViewBag.PartyList = new SelectList(partyList, "PartyId", "PartyName");
            var designList = await _db.Designs.AsNoTracking().OrderBy(o => o.DesignNo).ToListAsync();
            ViewBag.DesignList = new SelectList(designList, "DesignId", "DesignNo");

            var vPreviousProgram = await _db.Program.AsNoTracking().OrderByDescending(x => x.ProgramId).FirstOrDefaultAsync();
            int vNextProgramNo = 1;

            if (vPreviousProgram != null)
            {
                var vPreviousProgramNo = int.Parse(vPreviousProgram.ProgramNo);
                vNextProgramNo = vPreviousProgramNo + 1;
            }

            var vModel = new ProgramVM
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                ProgramNo = $"{vNextProgramNo}"
            };

            if (id != null)
            {
                ViewBag.Action = "Edit";

                var program = await _db.Program.AsNoTracking().Include(x => x.ProgramMatchings).FirstOrDefaultAsync(x => x.ProgramId == id);

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
                    PhotoFileName = program.PhotoFileName,
                    SelectedDesignIds = program.ProgramMatchings.Select(x => x.DesignId).Distinct().ToList(),
                    SelectedMatchings = program.ProgramMatchings.Select(x => $"{x.DesignId}|{x.MatchingNo}").Distinct().ToList()
                };

                // Reuse the already loaded lists and set selected values
                ViewBag.PartyList = new SelectList(partyList, "PartyId", "PartyName", program.PartyId);
                ViewBag.DesignList = new SelectList(designList, "DesignId", "DesignNo");

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
            if (model.SelectedDesignIds == null || !model.SelectedDesignIds.Any())
                ModelState.AddModelError(nameof(model.SelectedDesignIds), "Please select at least one design.");

            if (model.SelectedMatchings == null || !model.SelectedMatchings.Any())
                ModelState.AddModelError(nameof(model.SelectedMatchings), "Please select at least one matching.");

            if (!ModelState.IsValid)
            {
                ViewBag.PartyList = new SelectList(_db.Party, "PartyId", "PartyName", model.PartyId);
                ViewBag.DesignList = new SelectList(_db.Designs, "DesignId", "DesignNo");
                return View("AddEdit", model);
            }

            #region Upload Photo
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "programs");
            Directory.CreateDirectory(uploads);

            if (model.Photo != null && model.Photo.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(model.Photo.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Photo), "Only jpg, jpeg, png files allowed.");
                    return View("AddEdit", model);
                }

                if (model.Photo.Length > 3 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(model.Photo), "File size must be under 3MB.");
                    return View("AddEdit", model);
                }

                var fileName = $"photo_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploads, fileName);

                using var fs = System.IO.File.Create(filePath);
                await model.Photo.CopyToAsync(fs);

                model.PhotoFileName = fileName;
            }
            #endregion

            // Parse
            var vMatchingList = model.SelectedMatchings
                .Select(x => new
                {
                    DesignId = int.Parse(x.Split('|')[0]),
                    MatchingNo = int.Parse(x.Split('|')[1])
                })
                .ToList();

            // HashSet
            var vMatchingSet = vMatchingList
                .Select(x => (x.DesignId, x.MatchingNo))
                .ToHashSet();

            var vDesignIds = vMatchingList.Select(x => x.DesignId).Distinct().ToList();

            var vDBMatchings = await _db.DesignMatchings
                .AsNoTracking()
                .Include(x => x.DesignPlate)
                .Where(x => vDesignIds.Contains(x.DesignPlate.DesignId))
                .ToListAsync();

            var vMatchings = vDBMatchings
                .Where(x => vMatchingSet.Contains(
                    (x.DesignPlate.DesignId, x.MatchingNo)))
                .ToList();


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
                    PhotoFileName = model.PhotoFileName
                };

                await _db.Program.AddAsync(program);
                await _db.SaveChangesAsync();

                foreach (var item in vMatchings)
                {
                    program.ProgramMatchings.Add(new ProgramMatching
                    {
                        ProgramId = program.ProgramId,
                        DesignId = item.DesignPlate.DesignId,
                        DesignMatchingId = item.DesignMatchingId,
                        PlateId = item.DesignPlateId,
                        MatchingNo = item.MatchingNo,
                        Colour = item.Colour
                    });
                }
            }
            else
            {
                var program = await _db.Program
                    .Include(x => x.ProgramMatchings)
                    .FirstOrDefaultAsync(x => x.ProgramId == model.ProgramId);

                if (program == null)
                    return NotFound();

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

                if (!string.IsNullOrEmpty(model.PhotoFileName))
                    program.PhotoFileName = model.PhotoFileName;

                _db.ProgramMatchings.RemoveRange(program.ProgramMatchings);

                foreach (var item in vMatchings)
                {
                    program.ProgramMatchings.Add(new ProgramMatching
                    {
                        ProgramId = program.ProgramId,
                        DesignId = item.DesignPlate.DesignId,
                        DesignMatchingId = item.DesignMatchingId,
                        PlateId = item.DesignPlateId,
                        MatchingNo = item.MatchingNo,
                        Colour = item.Colour
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
        public async Task<IActionResult> Print(int ProgramId, string PrintView)
        {
            // Load program header using no-tracking
            var vModel = await _db.Program.AsNoTracking()
                .Where(x => x.ProgramId == ProgramId)
                .Select(x => new ProgramVM
                {
                    ProgramId = x.ProgramId,
                    ProgramNo = x.ProgramNo,
                    PartyName = _db.Party.Where(p => p.PartyId == x.PartyId).Select(p => p.PartyName).FirstOrDefault(),
                    DesignNo = 0,
                    Quality = x.Quality,
                    Date = x.Date,
                    MainCut = x.MainCut,
                    Fold = x.Fold,
                    Finishing = x.Finishing,
                    Quantity = x.Quantity,
                    Remarks = x.Remarks,
                    Round = x.Round,
                    Rate = x.Rate,
                    PhotoFileName = x.PhotoFileName
                }).FirstOrDefaultAsync();

            // Load matching list with no-tracking and batch lookups for plate and design names
            var matchings = await _db.ProgramMatchings.AsNoTracking()
                .Where(m => m.ProgramId == ProgramId)
                .ToListAsync();

            var plateIds = matchings.Select(m => m.PlateId).Distinct().ToList();
            var designIds = matchings.Select(m => m.DesignId).Distinct().ToList();

            var plateMap = await _db.DesignPlates.AsNoTracking().Where(p => plateIds.Contains(p.DesignPlateId)).ToDictionaryAsync(p => p.DesignPlateId, p => p.PlateName);
            var designMap = await _db.Designs.AsNoTracking().Where(d => designIds.Contains(d.DesignId)).ToDictionaryAsync(d => d.DesignId, d => d.DesignNo);

            ViewBag.MatchingList = matchings.Select(m => new ProgramMatchingVM
            {
                MatchingNo = m.MatchingNo,
                Colour = m.Colour,
                DesignMatchingId = m.DesignMatchingId,
                PlateId = m.PlateId,
                PlateName = plateMap.TryGetValue(m.PlateId, out var pn) ? pn : string.Empty,
                DesignId = m.DesignId,
                DesignNo = designMap.TryGetValue(m.DesignId, out var dn) ? dn : 0
            }).ToList();


            if (PrintView == "DesignMatching")
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
        public async Task<IActionResult> GetMatchingByDesignIDCSV(string DesignIDCSV)
        {
            var vDesignIDList = DesignIDCSV
                .Split(',')
                .Select(int.Parse)
                .ToList();

            var data = await _db.DesignMatchings
                .Where(x => vDesignIDList.Contains(x.DesignPlate.DesignId))
                .Select(x => new
                {
                    DesignId = x.DesignPlate.DesignId,
                    DesignNo = x.DesignPlate.Design.DesignNo,
                    MatchingNo = x.MatchingNo
                })
                .Distinct()
                .OrderBy(x => x.DesignNo)
                .ThenBy(x => x.MatchingNo)
                .ToListAsync();

            return Json(data);
        }
        #endregion
    }
}
