using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace DoAnChuyenNganh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SinhVienController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SinhVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang chính
        public IActionResult SinhVien()
        {
            return View("~/Areas/Admin/Views/Home/SinhVien.cshtml");
        }

        // Lấy tất cả SinhVien
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sinhVienList = await _context.SinhViens
                .Include(sv => sv.Khoa)
                .Include(sv => sv.NguoiDung)
                .Select(sv => new
                {
                    sv.MaSV,
                    sv.HoTen,
                    sv.Lop,
                    sv.Email,
                    sv.SoDienThoai,
                    Khoa = sv.Khoa != null ? sv.Khoa.TenKhoa : "Không có",
                    NguoiDung = sv.NguoiDung != null ? sv.NguoiDung.TenDangNhap : "Không có"
                })
                .ToListAsync();

            return Json(sinhVienList);
        }

        // Lấy SinhVien theo Id
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var sinhVien = await _context.SinhViens.FindAsync(id);
            if (sinhVien == null)
                return NotFound();

            return Json(sinhVien);
        }

        // Thêm mới SinhVien
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            try
            {
                var maSV = Request.Form["MaSV"];
                if (await _context.SinhViens.AnyAsync(sv => sv.MaSV == int.Parse(maSV)))
                {
                    return BadRequest("Mã sinh viên đã tồn tại.");
                }

                string email = Request.Form["Email"];
                string soDienThoai = Request.Form["SoDienThoai"];

                if (await _context.SinhViens.AnyAsync(sv => sv.Email == email))
                    return BadRequest("Email này đã được sử dụng.");

                if (await _context.SinhViens.AnyAsync(sv => sv.SoDienThoai == soDienThoai))
                    return BadRequest("Số điện thoại này đã được sử dụng.");

                var sinhVien = new SinhVien
                {
                    MaSV = int.Parse(maSV),
                    HoTen = Request.Form["HoTen"],
                    Lop = Request.Form["Lop"],
                    Email = Request.Form["Email"],
                    SoDienThoai = Request.Form["SoDienThoai"],
                    MaKhoa = int.Parse(Request.Form["MaKhoa"]),
                    MaNguoiDung = int.Parse(Request.Form["MaNguoiDung"])
                };

                if (ModelState.IsValid)
                {
                    _context.SinhViens.Add(sinhVien);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                return BadRequest("Dữ liệu không hợp lệ.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi thêm SinhVien: " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }




        // Cập nhật SinhVien
        [HttpPut]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                var sinhVien = await _context.SinhViens.FindAsync(id);
                if (sinhVien == null)
                {
                    return NotFound("Không tìm thấy sinh viên.");
                }

                string email = Request.Form["Email"];
                string soDienThoai = Request.Form["SoDienThoai"];

                if (await _context.SinhViens.AnyAsync(sv => sv.Email == email && sv.MaSV != id))
                    return BadRequest("Email này đã được sử dụng.");

                if (await _context.SinhViens.AnyAsync(sv => sv.SoDienThoai == soDienThoai && sv.MaSV != id))
                    return BadRequest("Số điện thoại này đã được sử dụng.");

                sinhVien.HoTen = Request.Form["HoTen"];
                sinhVien.Lop = Request.Form["Lop"];
                sinhVien.Email = Request.Form["Email"];
                sinhVien.SoDienThoai = Request.Form["SoDienThoai"];
                sinhVien.MaKhoa = int.Parse(Request.Form["MaKhoa"]);
                sinhVien.MaNguoiDung = int.Parse(Request.Form["MaNguoiDung"]);

                if (ModelState.IsValid)
                {
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                return BadRequest("Dữ liệu không hợp lệ.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi cập nhật SinhVien: " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }



        // Xóa SinhVien
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var sinhVien = await _context.SinhViens.FindAsync(id);
            if (sinhVien == null)
                return NotFound();

            bool isLinkedToNhom = await _context.ThanhVienNhoms.AnyAsync(tv => tv.MaSV == id);
            if (isLinkedToNhom)
                return BadRequest("Không thể xóa sinh viên vì sinh viên đang thuộc một nhóm.");

            _context.SinhViens.Remove(sinhVien);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
