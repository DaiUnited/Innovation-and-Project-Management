using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DoAnChuyenNganh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GiaiThuongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GiaiThuongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Load trang chính
        public IActionResult GiaiThuong()
        {
            return View("~/Areas/Admin/Views/Home/GiaiThuong.cshtml");
        }

        // Lấy danh sách tất cả giải thưởng
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var giaiThuongList = await _context.GiaiThuongs.ToListAsync();
            return Json(giaiThuongList);
        }

        // Lấy thông tin giải thưởng theo ID
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var giaiThuong = await _context.GiaiThuongs.FindAsync(id);
            if (giaiThuong == null)
                return NotFound();
            return Json(giaiThuong);
        }

        // Thêm giải thưởng
        [HttpPost]
        public async Task<IActionResult> Create(string tenGiaiThuong, decimal soTienThuong, string moTa)
        {
            if (await _context.GiaiThuongs.AnyAsync(gt => gt.TenGiaiThuong == tenGiaiThuong))
                return BadRequest("Tên giải thưởng đã tồn tại.");

            var giaiThuong = new GiaiThuong
            {
                TenGiaiThuong = tenGiaiThuong,
                SoTienThuong = soTienThuong,
                MoTa = moTa
            };

            _context.GiaiThuongs.Add(giaiThuong);
            await _context.SaveChangesAsync();
            return Ok("Thêm giải thưởng thành công.");
        }

        // Cập nhật giải thưởng
        [HttpPut]
        public async Task<IActionResult> Update(int id, string tenGiaiThuong, decimal soTienThuong, string moTa)
        {
            var giaiThuong = await _context.GiaiThuongs.FindAsync(id);
            if (giaiThuong == null)
                return NotFound("Không tìm thấy giải thưởng.");

            if (await _context.GiaiThuongs.AnyAsync(gt => gt.TenGiaiThuong == tenGiaiThuong && gt.MaGiaiThuong != id))
                return BadRequest("Tên giải thưởng đã tồn tại.");

            giaiThuong.TenGiaiThuong = tenGiaiThuong;
            giaiThuong.SoTienThuong = soTienThuong;
            giaiThuong.MoTa = moTa;

            _context.Entry(giaiThuong).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok("Cập nhật giải thưởng thành công.");
        }

        // Xóa giải thưởng
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var giaiThuong = await _context.GiaiThuongs.FindAsync(id);
            if (giaiThuong == null)
                return NotFound("Không tìm thấy giải thưởng.");

            _context.GiaiThuongs.Remove(giaiThuong);
            await _context.SaveChangesAsync();
            return Ok("Xóa giải thưởng thành công.");
        }
    }
}
