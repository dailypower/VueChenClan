using System.Collections.Generic;
using System.Linq;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/user")]
    public class UserApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public UserApiController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();

            foreach (var user in objUserList)
            {
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
                if (user.Company == null) user.Company = new Company { Name = "" };
            }

            return Ok(new { data = objUserList });
        }

        [HttpPost("lockunlock")]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            if (objFromDb == null) return BadRequest(new { success = false, message = "User not found" });

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > System.DateTime.Now)
                objFromDb.LockoutEnd = System.DateTime.Now;
            else
                objFromDb.LockoutEnd = System.DateTime.Now.AddYears(1000);

            _unitOfWork.ApplicationUser.Update(objFromDb);
            _unitOfWork.Save();
            return Ok(new { success = true });
        }
    }
}
