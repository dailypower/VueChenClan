using System.Collections.Generic;
using System.Linq;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/category")]
    public class CategoryApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public CategoryApiController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search)
        {
            IEnumerable<Category> list;
            if (!string.IsNullOrWhiteSpace(search))
                list = _unitOfWork.Category.GetAll(filter: c => c.Name != null && c.Name.Contains(search)).ToList();
            else
                list = _unitOfWork.Category.GetAll().ToList();

            return Ok(new { data = list });
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var item = _unitOfWork.Category.Get(u => u.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Category obj)
        {
            if (obj == null) return BadRequest();
            _unitOfWork.Category.Add(obj);
            _unitOfWork.Save();
            return Ok(new { success = true, data = obj });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Category obj)
        {
            var existing = _unitOfWork.Category.Get(u => u.Id == id);
            if (existing == null) return NotFound();
            existing.Name = obj.Name;
            existing.DisplayOrder = obj.DisplayOrder;
            _unitOfWork.Category.Update(existing);
            _unitOfWork.Save();
            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.Category.Get(u => u.Id == id);
            if (obj == null) return BadRequest(new { success = false, message = "刪除失敗" });
            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();
            return Ok(new { success = true });
        }

        [HttpPost("deleterange")]
        public IActionResult DeleteRange([FromBody] List<int> ids)
        {
            foreach (var id in ids)
            {
                var entity = _unitOfWork.Category.Get(u => u.Id == id);
                if (entity != null) _unitOfWork.Category.Remove(entity);
            }
            _unitOfWork.Save();
            return Ok(new { success = true });
        }

        [HttpPost("deleteall")]
        public IActionResult DeleteAll()
        {
            var all = _unitOfWork.Category.GetAll().ToList();
            foreach (var e in all) _unitOfWork.Category.Remove(e);
            _unitOfWork.Save();
            return Ok(new { success = true });
        }
    }
}
