using System;
using System.Collections.Generic;
using System.Linq;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BulkyBook.Models.ViewModels;


namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/kindness")]
    public class KindnessApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public KindnessApiController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        /// <summary>
        /// GET /api/admin/kindness
        /// Search and return all kindness positions with encrypted applicant names
        /// </summary>
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search)
        {
            IEnumerable<KindnessPosition> objKindnessPositionList;
            if (!string.IsNullOrWhiteSpace(search))
            {
                objKindnessPositionList = _unitOfWork.Kindness.GetAll(
                    filter: x => x.Name != null && x.Name.Contains(search)
                ).ToList();
            }
            else
            {
                objKindnessPositionList = _unitOfWork.Kindness.GetAll().ToList();
            }

            // Encrypt applicant names for privacy
            foreach (var item in objKindnessPositionList)
            {
                try
                {
                    if (item.Applicant == null)
                    {
                        continue;
                    }
                    else if (item.Applicant.Length < 2)
                    {
                        continue;
                    }
                    else if (item.Applicant.Length > 2)
                    {
                        string encrpted_applicant = item.Applicant.Substring(0, 1) + "*" + item.Applicant.Substring(2, item.Applicant.Length - 2) ?? "";
                        item.Applicant = encrpted_applicant;
                    }
                    else if (item.Applicant.Length == 2)
                    {
                        string encrpted_applicant = item.Applicant.Substring(0, 2) + "*";
                        item.Applicant = encrpted_applicant;
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = "加密申請人姓名失敗!!" });
                    }
                }
                catch
                {
                    continue;
                }
            }

            return Ok(new { data = objKindnessPositionList });
        }

        /// <summary>
        /// DELETE /api/admin/kindness/{id}
        /// Delete a single kindness position by ID
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(int? id)
        {
            var KindnessToBeDeleted = _unitOfWork.Kindness.Get(u => u.KindnessPositionId == id);
            if (KindnessToBeDeleted == null)
            {
                return BadRequest(new { success = false, message = "刪除失敗" });
            }

            _unitOfWork.Kindness.Remove(KindnessToBeDeleted);
            string strResult = _unitOfWork.Save();

            return Ok(new { success = true, message = "刪除成功" });
        }

        /// <summary>
        /// POST /api/admin/kindness/deleterange
        /// Delete multiple kindness positions by list of IDs
        /// </summary>
        [HttpPost("deleterange")]
        public IActionResult DeleteRange([FromBody] List<int> ids)
        {
            foreach (var id in ids)
            {
                var entity = _unitOfWork.Kindness.Get(x => x.KindnessPositionId == id);
                if (entity != null)
                    _unitOfWork.Kindness.Remove(entity);
            }
            _unitOfWork.Save();
            return Ok(new { success = true });
        }

        /// <summary>
        /// POST /api/admin/kindness/deleteall
        /// Delete all kindness positions
        /// </summary>
        [HttpPost("deleteall")]
        public IActionResult DeleteAll()
        {
            var all = _unitOfWork.Kindness.GetAll().ToList();
            foreach (var entity in all)
                _unitOfWork.Kindness.Remove(entity);
            _unitOfWork.Save();
            return Ok(new { success = true });
        }

        [HttpPost("import")]
        public IActionResult Import([FromBody] List<KindnessPositionViewModel> rows)
        {
            var errors = new List<string>();
            var valid = new List<KindnessPosition>();
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                int rowNum = i + 2;
                if (string.IsNullOrWhiteSpace(r.Name)) errors.Add($"第{rowNum}行: 姓名為必填");
                if (r.Floor != "1?" && r.Floor != "2?" && r.Floor != "3?") errors.Add($"第{rowNum}行: 樓層必須為'1?'或'2?'或'3?'");
                if (string.IsNullOrWhiteSpace(r.Section)) errors.Add($"第{rowNum}行: 區為必填");
                if (string.IsNullOrWhiteSpace(r.Level)) errors.Add($"第{rowNum}行: 層為必填");
                if (string.IsNullOrWhiteSpace(r.Position)) errors.Add($"第{rowNum}行: 編號為必填");
                if (string.IsNullOrWhiteSpace(r.PositionId)) errors.Add($"第{rowNum}行: 牌位為必填");
                if (_unitOfWork.Kindness.Get(a => a.PositionId == r.PositionId) != null) errors.Add($"第{rowNum}行: 牌位編號 [{r.PositionId}] 已存在於資料庫");

                if (!errors.Any(e => e.StartsWith($"第{rowNum}行")))
                {
                    valid.Add(new KindnessPosition
                    {
                        Name = r.Name,
                        Floor = r.Floor,
                        Section = r.Section,
                        Level = r.Level,
                        Position = r.Position,
                        PositionId = r.PositionId,
                        Applicant = r.Applicant,
                        Relation = r.Relation,
                        Mobile_Tel = r.Mobile_Tel,
                        Note = r.Note
                    });
                }
            }

            if (errors.Count > 0) return BadRequest(new { success = false, errors });
            foreach (var e in valid) _unitOfWork.Kindness.Add(e);
            _unitOfWork.Save();
            return Ok(new { success = true });
        }

        [HttpPost("saveposition")]
        public IActionResult SavePosition([FromBody] SavePositionRequest dto)
        {
            try
            {
                var displaytext = dto.DisplayText ?? string.Empty;
                var colon_Index = displaytext.IndexOf(":");
                var splitter_floor = "?";
                var splitter_section = "?";
                var splitter_level = "?";

                var floor_Index = displaytext.IndexOf(splitter_floor);
                var section_Index = displaytext.IndexOf(splitter_section);
                var level_Index = displaytext.IndexOf(splitter_level);

                if (colon_Index < 0 || floor_Index < 0 || section_Index < 0 || level_Index < 0)
                    return BadRequest(new { success = false, message = "displaytext format invalid" });

                var floor = displaytext.Substring(Math.Max(0, floor_Index - 1), Math.Min(2, floor_Index));
                var section = displaytext.Substring(Math.Max(0, section_Index - 1), Math.Min(2, section_Index));
                var level = displaytext.Substring(Math.Max(0, level_Index - 1), Math.Min(2, level_Index));
                var position = displaytext.Substring(colon_Index + 1);

                var existing = _unitOfWork.Kindness.Get(u => u.PositionId == displaytext);
                if (existing != null && existing.KindnessPositionId != dto.SelectedId)
                    return BadRequest(new { success = false, message = "選取的牌位已被使用" });

                var target = _unitOfWork.Kindness.Get(u => u.KindnessPositionId == dto.SelectedId);
                if (target == null) return NotFound(new { success = false, message = "選取的牌位不存在" });

                target.PositionId = displaytext;
                target.Floor = floor;
                target.Section = section;
                target.Level = level;
                target.Position = position;

                _unitOfWork.Kindness.Update(target);
                _unitOfWork.Save();
                return Ok(new { success = true });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public class SavePositionRequest
        {
            public string? DisplayText { get; set; }
            public int? SelectedId { get; set; }
        }
    }
}
