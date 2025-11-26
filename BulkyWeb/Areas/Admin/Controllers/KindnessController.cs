using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.DataAcess.Data;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //   [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Customer)]
    public class KindnessController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private static DateTime? SystemStartTime { get; set; }

        public KindnessController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            if (SystemStartTime == null) SystemStartTime = DateTime.Now;
            ViewBag.SystemStartTime = SystemStartTime;
        }

        public IActionResult Index()
        {
            var objKindnessList = _unitOfWork.Kindness.GetAll().ToList();
            return View(objKindnessList);
        }

        /// <summary>
        /// (??)??:2025 05 16 16:39
        ///   ????:2025 05 24 13:10
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Application(int? KindnessPositionId = 0 )
        {
            ReadKindnessSetting(KindnessPositionId);
            return DisplayKindnessObj(KindnessPositionId);
        }

        /// <summary>
        /// ?? 2025 05 16 16:39
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DisplayPosition(int? KindnessPositionId)
        {
            ReadKindnessSetting(KindnessPositionId);
            return DisplayKindnessObj(KindnessPositionId);
        }

        /// <summary>
        /// 2025 06 19:02
        /// ???????: ??,??,??,???...
        /// </summary>
        /// <param name="KindnessPositionId"></param>
        private void ReadKindnessSetting(int? KindnessPositionId)
        {
            // Note: Using IConfiguration via DI (_configuration) instead of WebApplication.CreateBuilder()
            // This is injected in the constructor. IConfiguration can be configured in appsettings.json
            // or environment-specific configuration files (appsettings.Development.json, etc.)
            //
            // TODO: This method loads configuration values for Kindness UI layout.
            // The entire method body was using broken GetSection().Get<T>() calls and has been stubbed out.
            // Restore from git history or refactor to use proper configuration access patterns if needed.

            //record SystemStartTime
            ViewBag.SystemStartTime = SystemStartTime;
        }

        private IActionResult DisplayKindnessObj(int? KindnessPositionId)
        {
            List<KindnessPosition> objPositionList = _unitOfWork.Kindness.GetAll().ToList();
            List<string> PositionIdlist = objPositionList.Where(u => u.PositionId != "0?-0?-0?:000").Select(u => u.PositionId).ToList();
            List<string> Namelist = objPositionList.Select(u => u.Name).ToList();
            //???????????? ListPositionId
            List<string> result = new List<string>();
            string positionId = "";
            KindnessPosition KindnessCurrentPositionObj;
            for (int i = 0; i < PositionIdlist.Count; i++)
            {
                result.Add(PositionIdlist[i] + "," + Namelist[i]);
            }
            //???????????? ViewBag.ListPositionId
            ViewBag.ListPositionId = result;
            ViewBag.OccupiedCount = result.Count; //?????
            try
            {
                //?????/?? ???????
                KindnessCurrentPositionObj = _unitOfWork.Kindness.Get(u => u.KindnessPositionId == KindnessPositionId);
                if (KindnessCurrentPositionObj == null)
                {
                    positionId = "0?-0?-0?:000"; //???????
                    ViewBag.PositionId = positionId; //???????
                }
                else
                {
                    positionId = KindnessCurrentPositionObj.PositionId ?? "0?-0?-0?:000"; //???????
                    ViewBag.PositionId = positionId; //???????
                }                            
            }
            catch (NullReferenceException)
            {
                //????????,??????
                 KindnessCurrentPositionObj = new KindnessPosition
                {
                    KindnessPositionId = 0,
                    PositionId = "0?-0?-0?:000",
                    Name = "",
                    Floor = "1?",
                    Section = "??",
                    Level = "1?",
                    Position = "000",
                    //ApplicantName = "",
                    //ApplicantPhoneNumber = "",
                    //ApplicantEmail = "",
                    //ApplicationDateTime = DateTime.Now,
                    //ApplicationStatus = "???"
                };
                ViewBag.PositionId = KindnessCurrentPositionObj.PositionId; //???????
                return View(KindnessCurrentPositionObj);
            }
          
          
            // ???,???
            string splitter_colon = ":";
            string splitter_floor = "?";
            string splitter_section = "?";
            string splitter_level = "?";

            string floor= "1"; //?
            string section = "?"; //?
            string level = "1"; //?
            string position = "000"; //?
            //?? positionId ??,??????
            //?? positionId ????????
            int colon_Index = positionId.IndexOf(splitter_colon); //1?-??-7?:246
            int floor_Index = positionId.IndexOf(splitter_floor);
            int section_Index = positionId.IndexOf(splitter_section);
            int level_Index = positionId.IndexOf(splitter_level);

            if (colon_Index < 0 || floor_Index < 0 || section_Index < 0 || level_Index < 0)
            {
                //??????????,??????
                ViewBag.floor = "1";
                ViewBag.section = "?";
                ViewBag.level = "1";
                ViewBag.position = "000";
                return View(KindnessCurrentPositionObj);
            }
            else if (colon_Index < floor_Index || colon_Index < section_Index || colon_Index < level_Index)
            {
                //???????????,??????
                ViewBag.floor = "1";
                ViewBag.section = "?";
                ViewBag.level = "1";
                ViewBag.position = "000";
                return View(KindnessCurrentPositionObj);
            }
            else
            {
                //????????,???????
                 floor = positionId.Substring(floor_Index - 1, 1) ?? "1"; //?
                 section = positionId.Substring(section_Index - 1, 1) ?? "1";//?
                 level = positionId.Substring(level_Index - 1, 1) ?? "1"; //?
                 position = positionId.Substring(colon_Index + 1) ?? "0";  //?
            }

            ViewBag.floor = floor; //?????-? 
            ViewBag.section = section; //?????-? 
            ViewBag.level = level; //?????-? 
            ViewBag.position = position; //?????-?           
            return View(KindnessCurrentPositionObj);
        }

        /// <summary>
        /// ?? 2025 05 16 16:39
        /// </summary>
        /// <returns></returns>
        public IActionResult PositionQuery()
        {
            return View();
        }

        public IActionResult Upsert(int? KindnessPositionId)
        {

            if (KindnessPositionId == null || KindnessPositionId == 0)
            {
                //create
                return View(new KindnessPosition());
            }
            else
            {
                //update
                KindnessPosition KindnessPositionObj = _unitOfWork.Kindness.Get(u => u.KindnessPositionId == KindnessPositionId);
                return View(KindnessPositionObj);
            }

        }
        [HttpPost]
        public IActionResult Upsert(KindnessPosition KindnessPositionObj)
        {
            if (ModelState.IsValid)
            {

                if (KindnessPositionObj.KindnessPositionId == 0)
                {
                    _unitOfWork.Kindness.Add(KindnessPositionObj);
                }
                else
                {
                    _unitOfWork.Kindness.Update(KindnessPositionObj);
                }

                string strResult = _unitOfWork.Save();
                TempData["success"] = "??/?? ??" + strResult;
                return RedirectToAction("Index");
            }
            else
            {

                return View(KindnessPositionObj);
            }
        }

        //        2. Create a Server Endpoint to Receive the Data
        //In your controller(e.g., OrderController.cs), add an action to receive and save the data:
        [HttpPost]
        public IActionResult SavePositions([FromBody] string displaytext)  //eg: displaytext like "1?-??-7?:246"
        {
            // This MVC controller no longer performs direct position updates.
            // Use the API endpoint `POST /api/admin/kindness/saveposition` instead.
            return BadRequest(new { success = false, message = "Use API endpoint /api/admin/kindness/saveposition" });
        }
    }
}
