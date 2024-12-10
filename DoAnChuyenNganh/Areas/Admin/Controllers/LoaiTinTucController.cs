using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DoAnChuyenNganh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LoaiTinTucController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoaiTinTucController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang chính
        public IActionResult LoaiTinTuc()
        {
            return View("~/Areas/Admin/Views/Home/LoaiTinTuc.cshtml");
        }

        // Lấy danh sách tất cả loại tin tức
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var loaiTinTucs = await _context.LoaiTinTucs.ToListAsync();
            return Json(loaiTinTucs);
        }

        // Lấy thông tin loại tin tức theo ID
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var loaiTinTuc = await _context.LoaiTinTucs.FindAsync(id);
            if (loaiTinTuc == null)
                return NotFound();

            return Json(loaiTinTuc);
        }

        // Thêm loại tin tức
        [HttpPost]
        public async Task<IActionResult> Create(string tenLoai, string moTa)
        {
            if (await _context.LoaiTinTucs.AnyAsync(lt => lt.TenLoai == tenLoai))
                return BadRequest("Tên loại tin tức đã tồn tại.");

            var loaiTinTuc = new LoaiTinTuc
            {
                TenLoai = tenLoai,
                MoTa = moTa
            };

            _context.LoaiTinTucs.Add(loaiTinTuc);
            await _context.SaveChangesAsync();
            return Ok("Thêm loại tin tức thành công.");
        }

        // Cập nhật loại tin tức
        [HttpPut]
        public async Task<IActionResult> Update(int id, string tenLoai, string moTa)
        {
            var loaiTinTuc = await _context.LoaiTinTucs.FindAsync(id);
            if (loaiTinTuc == null)
                return NotFound("Không tìm thấy loại tin tức.");

            if (await _context.LoaiTinTucs.AnyAsync(lt => lt.TenLoai == tenLoai && lt.MaLoaiTinTuc != id))
                return BadRequest("Tên loại tin tức đã tồn tại.");

            loaiTinTuc.TenLoai = tenLoai;
            loaiTinTuc.MoTa = moTa;

            _context.Entry(loaiTinTuc).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok("Cập nhật loại tin tức thành công.");
        }

        // Xóa loại tin tức
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var loaiTinTuc = await _context.LoaiTinTucs.FindAsync(id);
            if (loaiTinTuc == null)
                return NotFound("Không tìm thấy loại tin tức.");

            _context.LoaiTinTucs.Remove(loaiTinTuc);
            await _context.SaveChangesAsync();
            return Ok("Xóa loại tin tức thành công.");
        }
    }
}
