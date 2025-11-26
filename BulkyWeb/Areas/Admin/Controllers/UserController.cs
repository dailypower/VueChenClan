using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.DataAcess.Data;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    //public class SessionController : Controller
    //{

    //    private readonly IUnitOfWork _unitOfWork;
    //    public SessionController(IUnitOfWork unitOfWork )
    //    {
    //        _unitOfWork = unitOfWork;

    //    }

    //    [HttpPost("Start")]
    //    public IActionResult StartSession()
    //    {
    //        var userId = User.Identity.Name ?? "Anonymous";
    //        var startTime = DateTime.UtcNow;

    //        // Save to DB: (pseudo-code)
    //        string result=SaveStartTime(userId, startTime, _unitOfWork);

    //        return Ok(new { startTime });
    //    }
    //    public string SaveStartTime(string userId, DateTime startTime, IUnitOfWork _unitOfWork)
    //    {            
    //        try
    //        {               
    //            ApplicationUser user = _unitOfWork.ApplicationUser.Get(u => u.UserName == userId);
    //            //已存在
    //            if (user != null)
    //            {
    //               user.StartTime= startTime;
    //                //do db_change:
    //                _unitOfWork.ApplicationUser.Update(user);
    //                string strResult = _unitOfWork.Save();
    //                return "OK,"+ strResult;
    //            }
    //            else
    //            {
    //                //不存在，新增
    //                ApplicationUser newUser = new ApplicationUser
    //                {
    //                    UserName = userId,
    //                    StartTime = startTime
    //                };
    //                _unitOfWork.ApplicationUser.Add(newUser);
    //                string strResult = _unitOfWork.Save();
    //                return "OK," + strResult;
    //            }

    //        }
    //        catch (Exception ex)
    //        {
    //            // 可記錄 log
    //            return "Fail,"+ex.Message;
    //        }
    //    }
    //}


    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Customer)]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private static DateTime? SystemStartTime { get; set; }
        public UserController(UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager, IConfiguration configuration) {
            // System start time 2025 07 15 22:17
            if (SystemStartTime == null)
            {
                SystemStartTime = DateTime.Now;
            }
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
        }
        public IActionResult Index() 
        {         
              //2025.03.28 21:29 取得權限
            if (User.IsInRole(SD.Role_Admin))
            {
                TempData["Role"] = SD.Role_Admin;
            }
            else if (User.IsInRole(SD.Role_Company))
            {
                TempData["Role"] = SD.Role_Company;
            }
            else if (User.IsInRole(SD.Role_Customer))
            {
                TempData["Role"] = SD.Role_Customer;
            }
            else if (User.IsInRole(SD.Role_Employee))
            {
                TempData["Role"] = SD.Role_Employee;
            }
            else
            {
                TempData["Role"] = "尚未登入";
            }
            return View();
        }

        public IActionResult RoleManagment(string userId) {

            RoleManagmentVM RoleVM = new RoleManagmentVM() {
                ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeProperties:"Company"),
                RoleList = _roleManager.Roles.Select(i => new SelectListItem {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u=>u.Id==userId))
                    .GetAwaiter().GetResult().FirstOrDefault();
            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagment(RoleManagmentVM roleManagmentVM) {

            string oldRole  = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id))
                    .GetAwaiter().GetResult().FirstOrDefault();

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id);


            if (!(roleManagmentVM.ApplicationUser.Role == oldRole)) {
                //a role was updated
                if (roleManagmentVM.ApplicationUser.Role == SD.Role_Company) {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                }
                if (oldRole == SD.Role_Company) {
                    applicationUser.CompanyId = null;
                }
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagmentVM.ApplicationUser.Role).GetAwaiter().GetResult();

            }
            else {
                if(oldRole==SD.Role_Company && applicationUser.CompanyId != roleManagmentVM.ApplicationUser.CompanyId) {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            return RedirectToAction("Index");
        }


        // API endpoints moved to `UserApiController` (api/admin/user)
    }
}
