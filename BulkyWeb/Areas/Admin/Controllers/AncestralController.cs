using System;
using System.Collections.Generic;
using System.Linq;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AncestralController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private static DateTime? SystemStartTime { get; set; }

        public AncestralController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            if (SystemStartTime == null) SystemStartTime = DateTime.Now;
            ViewBag.SystemStartTime = SystemStartTime;
        }

        public IActionResult Index(int? KindnessPositionId)
        {
            ReadAncestralSetting(KindnessPositionId);
            var objAncestralList = _unitOfWork.Ancestral.GetAll().ToList();
            return View(objAncestralList);
        }

        [HttpGet]
        public IActionResult Application(int? AncestralPositionId)
        {
            ReadAncestralSetting(AncestralPositionId);
            return DisplayAncestralObj(AncestralPositionId);
        }

        [HttpGet]
        public IActionResult DisplayPosition(int? AncestralPositionId = 0)
        {
            ReadAncestralSetting(AncestralPositionId);
            return DisplayAncestralObj(AncestralPositionId);
        }

        private IActionResult DisplayAncestralObj(int? AncestralPositionId)
        {
            ViewBag.SystemStartTime = SystemStartTime;
            var objPositionList = _unitOfWork.Ancestral.GetAll().ToList();
            var PositionIdlist = objPositionList.Where(u => u.PositionId != null && u.PositionId != "0側-0區-0層:000").Select(u => u.PositionId).ToList();
            var Namelist = objPositionList.Select(u => u.Name ?? string.Empty).ToList();
            var result = new List<string>();
            for (int i = 0; i < PositionIdlist.Count && i < Namelist.Count; i++)
            {
                result.Add(PositionIdlist[i] + "," + Namelist[i]);
            }

            ViewBag.ListPositionId = result;
            ViewBag.OccupiedCount = result.Count;

            AncestralPosition PositionObj = null;
            string positionId = "0側-0區-0層:000";

            if (AncestralPositionId != null && AncestralPositionId > 0)
            {
                PositionObj = _unitOfWork.Ancestral.Get(u => u.AncestralPositionId == AncestralPositionId);
            }

            if (PositionObj == null)
            {
                PositionObj = new AncestralPosition
                {
                    AncestralPositionId = 0,
                    PositionId = positionId,
                    Name = string.Empty,
                    Side = "左側",
                    Section = "甲區",
                    Level = "1層",
                    Position = "000"
                };
                ViewBag.PositionId = PositionObj.PositionId;
                return View(PositionObj);
            }

            positionId = PositionObj.PositionId ?? positionId;
            ViewBag.PositionId = positionId;

            try
            {
                var splitter_colon = ":";
                var splitter_side = "側";
                var splitter_section = "區";
                var splitter_level = "層";

                var colon_Index = positionId.IndexOf(splitter_colon);
                var side_Index = positionId.IndexOf(splitter_side);
                var section_Index = positionId.IndexOf(splitter_section);
                var level_Index = positionId.IndexOf(splitter_level);

                if (colon_Index > 0 && side_Index > 0 && section_Index > 0 && level_Index > 0)
                {
                    var side = positionId.Substring(side_Index - 1, 1);
                    var section = positionId.Substring(section_Index - 1, 1);
                    var level = positionId.Substring(level_Index - 1, 1);
                    var position = positionId.Substring(colon_Index + 1);

                    ViewBag.side = side;
                    ViewBag.section = section;
                    ViewBag.level = level;
                    ViewBag.position = position;
                }
                else
                {
                    ViewBag.side = "左";
                    ViewBag.section = "甲";
                    ViewBag.level = "1";
                    ViewBag.position = "000";
                }
            }
            catch
            {
                ViewBag.side = "左";
                ViewBag.section = "甲";
                ViewBag.level = "1";
                ViewBag.position = "000";
            }
            
            // Initialize layout ViewBag properties for serialization in the view
            // These are used for the 3D display layout but may be stubbed/empty if functionality not implemented
            InitializeAncestralViewBag();

            return View(PositionObj);
        }

        private void ReadAncestralSetting(int? AncestralPositionId)
        {
            ViewBag.SystemStartTime = SystemStartTime;
            ViewBag.AncestralSide = _configuration.GetValue<int>("Ancestral:Side", 2);
            ViewBag.AncestralSection = _configuration.GetValue<int>("Ancestral:Section", 4);
            ViewBag.AncestralLevel = _configuration.GetValue<int>("Ancestral:Level", 10);
            ViewBag.AncestralPosition = _configuration.GetValue<int>("Ancestral:Position", 10);

            ViewBag.AncestralLayout_L = _configuration.GetValue<string>("Ancestral:Layout_L", string.Empty);
            ViewBag.AncestralLayout_R = _configuration.GetValue<string>("Ancestral:Layout_R", string.Empty);
            ViewBag.AncestralLayout = _configuration.GetValue<string>("Ancestral:Layout", string.Empty);

            ViewBag.lar = _configuration.GetValue<int>("Ancestral:la:row", 0);
            ViewBag.lac = _configuration.GetValue<int>("Ancestral:la:col", 0);
            ViewBag.lbr = _configuration.GetValue<int>("Ancestral:lb:row", 0);
            ViewBag.lbc = _configuration.GetValue<int>("Ancestral:lb:col", 0);
            ViewBag.lcr = _configuration.GetValue<int>("Ancestral:lc:row", 0);
            ViewBag.lcc = _configuration.GetValue<int>("Ancestral:lc:col", 0);
            ViewBag.ldr = _configuration.GetValue<int>("Ancestral:ld:row", 0);
            ViewBag.ldc = _configuration.GetValue<int>("Ancestral:ld:col", 0);
            ViewBag.lmr = _configuration.GetValue<int>("Ancestral:lm:row", 0);
            ViewBag.lmc = _configuration.GetValue<int>("Ancestral:lm:col", 0);

            ViewBag.rar = _configuration.GetValue<int>("Ancestral:ra:row", 0);
            ViewBag.rac = _configuration.GetValue<int>("Ancestral:ra:col", 0);
            ViewBag.rbr = _configuration.GetValue<int>("Ancestral:rb:row", 0);
            ViewBag.rbc = _configuration.GetValue<int>("Ancestral:rb:col", 0);
            ViewBag.rcr = _configuration.GetValue<int>("Ancestral:rc:row", 0);
            ViewBag.rcc = _configuration.GetValue<int>("Ancestral:rc:col", 0);
            ViewBag.rdr = _configuration.GetValue<int>("Ancestral:rd:row", 0);
            ViewBag.rdc = _configuration.GetValue<int>("Ancestral:rd:col", 0);
            ViewBag.rmr = _configuration.GetValue<int>("Ancestral:rm:row", 0);
            ViewBag.rmc = _configuration.GetValue<int>("Ancestral:rm:col", 0);

            ViewBag.PublishDate = _configuration.GetValue<string>("PublishDate", string.Empty);
            ViewBag.connectionString = _configuration.GetConnectionString("DefaultConnection");

            ViewBag.AUTO_LOGOUT_MINUTE = _configuration.GetValue<float?>("Logout_Duration:AUTO_LOGOUT_MINUTE") ?? 30f;
            ViewBag.WARNING_BEFORE_LOGOUT_SECOND = _configuration.GetValue<int?>("Logout_Duration:WARNING_BEFORE_LOGOUT_SECOND") ?? 10;
            ViewBag.Work_Duration = _configuration.GetValue<float?>("Work_Duration") ?? 1.0f;
            ViewBag.Import_Duration = _configuration.GetValue<float?>("Import_Duration") ?? 3.0f;
            ViewBag.WORK_WARNING_SECONDS = _configuration.GetValue<int?>("WORK_WARNING_SECONDS") ?? 60;

            var leftPrefixes = new[] { "ala", "alb", "alc", "ald", "alm" };
            var rightPrefixes = new[] { "ara", "arb", "arc", "ard", "arm" };
            foreach (var p in leftPrefixes.Concat(rightPrefixes))
            {
                for (int i = 1; i <= 10; i++)
                {
                    var key = $"{p}:row{i}";
                    ViewData[$"{p}row{i}"] = _configuration.GetValue<string>(key, string.Empty);
                }
            }
        }

        public IActionResult PositionQuery()
        {
            return View();
        }

        public IActionResult Upsert(int? AncestralPositionId)
        {
            if (AncestralPositionId == null || AncestralPositionId == 0)
            {
                return View(new AncestralPosition());
            }

            var AncestralObj = _unitOfWork.Ancestral.Get(u => u.AncestralPositionId == AncestralPositionId);
            return View(AncestralObj);
        }

        [HttpPost]
        public IActionResult Upsert(AncestralPosition AncestralObj)
        {
            if (ModelState.IsValid)
            {
                if (AncestralObj.AncestralPositionId == 0)
                {
                    _unitOfWork.Ancestral.Add(AncestralObj);
                }
                else
                {
                    _unitOfWork.Ancestral.Update(AncestralObj);
                }

                _unitOfWork.Save();
                TempData["success"] = "新增/更新 成功";
                return RedirectToAction("Index");
            }

            return View(AncestralObj);
        }

        private void InitializeAncestralViewBag()
        {
            // Initialize all layout ViewBag properties to empty lists if not already set
            // This allows the view to serialize them without null reference errors
            dynamic bag = ViewBag;
            bag.AncestralLayout_R = bag.AncestralLayout_R ?? new List<string>();
            bag.AncestralLayout_L = bag.AncestralLayout_L ?? new List<string>();
            bag.AncestralLayout = bag.AncestralLayout ?? new List<string>();
            bag.ararow1 = bag.ararow1 ?? new List<string>();
            bag.ararow2 = bag.ararow2 ?? new List<string>();
            bag.ararow3 = bag.ararow3 ?? new List<string>();
            bag.ararow4 = bag.ararow4 ?? new List<string>();
            bag.ararow5 = bag.ararow5 ?? new List<string>();
            bag.ararow6 = bag.ararow6 ?? new List<string>();
            bag.ararow7 = bag.ararow7 ?? new List<string>();
            bag.ararow8 = bag.ararow8 ?? new List<string>();
            bag.ararow9 = bag.ararow9 ?? new List<string>();
            bag.ararow10 = bag.ararow10 ?? new List<string>();
            bag.arbrow1 = bag.arbrow1 ?? new List<string>();
            bag.arbrow2 = bag.arbrow2 ?? new List<string>();
            bag.arbrow3 = bag.arbrow3 ?? new List<string>();
            bag.arbrow4 = bag.arbrow4 ?? new List<string>();
            bag.arbrow5 = bag.arbrow5 ?? new List<string>();
            bag.arbrow6 = bag.arbrow6 ?? new List<string>();
            bag.arbrow7 = bag.arbrow7 ?? new List<string>();
            bag.arbrow8 = bag.arbrow8 ?? new List<string>();
        }
    }
}
