using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoAnChuyenNganh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GiangVienHuongDanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GiangVienHuongDanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang chính
        public IActionResult GiangVienHuongDan()
        {
            return View("~/Areas/Admin/Views/Home/GiangVienHuongDan.cshtml");
        }

        // Lấy tất cả giảng viên hướng dẫn
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var giangVienList = await _context.GiangVienHuongDans
                .Include(gv => gv.Khoa)
                .Include(gv => gv.NguoiDung)
                .Select(gv => new
                {
                    gv.MaGVHD,
                    gv.HoTen,
                    gv.Email,
                    gv.SoDienThoai,
                    Khoa = gv.Khoa != null ? gv.Khoa.TenKhoa : "Không có",
                    NguoiDung = gv.NguoiDung != null ? gv.NguoiDung.TenDangNhap : "Không có"
                }).ToListAsync();

            return Json(giangVienList);
        }

        // Lấy giảng viên hướng dẫn theo Id
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var giangVien = await _context.GiangVienHuongDans
                .Include(gv => gv.Khoa)
                .Include(gv => gv.NguoiDung)
                .FirstOrDefaultAsync(gv => gv.MaGVHD == id);

            if (giangVien == null)
                return NotFound();

            return Json(giangVien);
        }

        // Thêm mới giảng viên hướng dẫn
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            string email = Request.Form["Email"];
            string soDienThoai = Request.Form["SoDienThoai"];

            if (await _context.GiangVienHuongDans.AnyAsync(gv => gv.Email == email))
                return BadRequest("Email này đã được sử dụng.");

            if (await _context.GiangVienHuongDans.AnyAsync(gv => gv.SoDienThoai == soDienThoai))
                return BadRequest("Số điện thoại này đã được sử dụng.");

            var giangVien = new GiangVienHuongDan
            {
                HoTen = Request.Form["HoTen"],
                Email = Request.Form["Email"],
                SoDienThoai = Request.Form["SoDienThoai"],
                MaKhoa = int.Parse(Request.Form["MaKhoa"]),
                MaNguoiDung = int.Parse(Request.Form["MaNguoiDung"])
            };

            if (ModelState.IsValid)
            {
                _context.GiangVienHuongDans.Add(giangVien);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Dữ liệu không hợp lệ.");
        }

        // Cập nhật giảng viên hướng dẫn
        [HttpPut]
        public async Task<IActionResult> Update(int id)
        {
            var giangVien = await _context.GiangVienHuongDans.FindAsync(id);
            if (giangVien == null)
                return NotFound("Giảng viên không tồn tại.");

            string email = Request.Form["Email"];
            string soDienThoai = Request.Form["SoDienThoai"];

            if (await _context.GiangVienHuongDans.AnyAsync(gv => gv.Email == email && gv.MaGVHD != id))
                return BadRequest("Email này đã được sử dụng.");

            if (await _context.GiangVienHuongDans.AnyAsync(gv => gv.SoDienThoai == soDienThoai && gv.MaGVHD != id))
                return BadRequest("Số điện thoại này đã được sử dụng.");

            giangVien.HoTen = Request.Form["HoTen"];
            giangVien.Email = Request.Form["Email"];
            giangVien.SoDienThoai = Request.Form["SoDienThoai"];
            giangVien.MaKhoa = int.Parse(Request.Form["MaKhoa"]);
            giangVien.MaNguoiDung = int.Parse(Request.Form["MaNguoiDung"]);

            if (ModelState.IsValid)
            {
                _context.Entry(giangVien).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Dữ liệu không hợp lệ.");
        }

        // Xóa giảng viên hướng dẫn
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Kiểm tra nếu giảng viên đang hướng dẫn đề tài
            var gvCoDeTai = await _context.DeTais.AnyAsync(dt => dt.MaGVHD == id);
            if (gvCoDeTai)
            {
                return BadRequest("Không thể xóa giảng viên vì đang hướng dẫn đề tài.");
            }

            var giangVien = await _context.GiangVienHuongDans.FindAsync(id);
            if (giangVien == null)
                return NotFound();

            _context.GiangVienHuongDans.Remove(giangVien);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
