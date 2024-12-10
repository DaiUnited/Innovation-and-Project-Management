using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DoAnChuyenNganh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LinhVucController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LinhVucController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult LinhVuc()
        {
            return View("~/Areas/Admin/Views/Home/LinhVuc.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var linhVucList = await _context.LinhVucs.ToListAsync();
            return Json(linhVucList);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var linhVuc = await _context.LinhVucs.FindAsync(id);
            return Json(linhVuc);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string TenLinhVuc)
        {
            // Kiểm tra tên lĩnh vực đã tồn tại
            bool isExisting = await _context.LinhVucs.AnyAsync(lv => lv.TenLinhVuc == TenLinhVuc);
            if (isExisting)
                return BadRequest("Tên lĩnh vực đã tồn tại.");

            var linhVuc = new LinhVuc { TenLinhVuc = TenLinhVuc };
            _context.LinhVucs.Add(linhVuc);
            await _context.SaveChangesAsync();
            return Ok();
        }


        // Cập nhật lĩnh vực
        [HttpPut]
        public async Task<IActionResult> Update(int id, string TenLinhVuc)
        {
            // Tìm lĩnh vực cần cập nhật
            var linhVuc = await _context.LinhVucs.FindAsync(id);
            if (linhVuc == null)
                return NotFound("Lĩnh vực không tồn tại.");

            // Kiểm tra nếu tên lĩnh vực mới bị trùng
            bool isDuplicate = await _context.LinhVucs.AnyAsync(lv => lv.TenLinhVuc == TenLinhVuc && lv.MaLinhVuc != id);
            if (isDuplicate)
            {
                return BadRequest("Tên lĩnh vực đã tồn tại.");
            }

            // Kiểm tra nếu lĩnh vực đang được sử dụng trong đề tài
            bool isLinkedToDeTai = await _context.DeTais.AnyAsync(dt => dt.MaLinhVuc == id);
            if (isLinkedToDeTai)
            {
                return BadRequest("Lĩnh vực này đang được sử dụng trong các đề tài, không thể cập nhật.");
            }

            // Kiểm tra nếu lĩnh vực đang liên kết với các đề tài trong quá trình thi
            var ongoingDeTai = await _context.DeTais
                .Include(dt => dt.CuocThi)
                .Where(dt => dt.MaLinhVuc == id &&
                             _context.VongThis.Any(vt => vt.MaCuocThi == dt.MaCuocThi &&
                                                         vt.NgayBatDau <= DateTime.Now &&
                                                         vt.NgayKetThuc >= DateTime.Now))
                .FirstOrDefaultAsync();
            if (ongoingDeTai != null)
            {
                return BadRequest("Lĩnh vực này đang được sử dụng trong các đề tài đang thi, không thể cập nhật.");
            }

            // Cập nhật tên lĩnh vực
            linhVuc.TenLinhVuc = TenLinhVuc;
            _context.Entry(linhVuc).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Cập nhật lĩnh vực thành công.");
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Kiểm tra nếu có đề tài thuộc lĩnh vực
            var linhVucCoDeTai = await _context.DeTais.AnyAsync(dt => dt.MaLinhVuc == id);
            if (linhVucCoDeTai)
            {
                return BadRequest("Không thể xóa lĩnh vực vì có đề tài đang thuộc lĩnh vực này.");
            }

            var linhVuc = await _context.LinhVucs.FindAsync(id);
            if (linhVuc == null)
                return NotFound();

            _context.LinhVucs.Remove(linhVuc);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
