using System.Collections.Generic;
using System.IO;
using System.Linq;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/product")]
    public class ProductApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public ProductApiController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search)
        {
            IEnumerable<Product> list;
            if (!string.IsNullOrWhiteSpace(search))
                list = _unitOfWork.Product.GetAll(filter: p => p.Title != null && p.Title.Contains(search), includeProperties: "Category,Company").ToList();
            else
                list = _unitOfWork.Product.GetAll(includeProperties: "Category,Company").ToList();

            return Ok(new { data = list });
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var item = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Category,Company,ProductImages");
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Product obj)
        {
            if (obj == null) return BadRequest();
            _unitOfWork.Product.Add(obj);
            _unitOfWork.Save();
            return Ok(new { success = true, data = obj });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Product obj)
        {
            var existing = _unitOfWork.Product.Get(u => u.Id == id);
            if (existing == null) return NotFound();
            existing.Title = obj.Title;
            existing.Description = obj.Description;
            existing.ListPrice = obj.ListPrice;
            existing.CategoryId = obj.CategoryId;
            existing.CompanyId = obj.CompanyId;
            _unitOfWork.Product.Update(existing);
            _unitOfWork.Save();
            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return BadRequest(new { success = false, message = "Error while deleting" });
            }

            string productPath = Path.Combine(_webHostEnvironment.WebRootPath ?? string.Empty, "images", "products", "product-" + id);

            if (Directory.Exists(productPath))
            {
                var filePaths = Directory.GetFiles(productPath);
                foreach (var filePath in filePaths) System.IO.File.Delete(filePath);
                Directory.Delete(productPath);
            }

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Ok(new { success = true, message = "Product deleted" });
        }
    }
}
