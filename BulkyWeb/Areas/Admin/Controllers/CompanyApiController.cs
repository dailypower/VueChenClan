using System.Collections.Generic;
using System.Linq;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/company")]
    public class CompanyApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public CompanyApiController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search)
        {
            IEnumerable<Company> list;
            if (!string.IsNullOrWhiteSpace(search))
                list = _unitOfWork.Company.GetAll(filter: c => c.Name != null && c.Name.Contains(search)).ToList();
            else
                list = _unitOfWork.Company.GetAll().ToList();

            return Ok(new { data = list });
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var item = _unitOfWork.Company.Get(u => u.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Company obj)
        {
            if (obj == null) return BadRequest();
            _unitOfWork.Company.Add(obj);
            _unitOfWork.Save();
            return Ok(new { success = true, data = obj });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Company obj)
        {
            var existing = _unitOfWork.Company.Get(u => u.Id == id);
            if (existing == null) return NotFound();
            existing.Name = obj.Name;
            existing.StreetAddress = obj.StreetAddress;
            existing.City = obj.City;
            existing.State = obj.State;
            existing.PostalCode = obj.PostalCode;
            existing.PhoneNumber = obj.PhoneNumber;
            _unitOfWork.Company.Update(existing);
            _unitOfWork.Save();
            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.Company.Get(u => u.Id == id);
            if (obj == null) return BadRequest(new { success = false, message = "刪除失敗" });
            _unitOfWork.Company.Remove(obj);
            _unitOfWork.Save();
            return Ok(new { success = true });
        }
    }
}
