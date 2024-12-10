using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DoAnChuyenNganh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class KhoaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KhoaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang Khoa
        public IActionResult Khoa()
        {
            return View("~/Areas/Admin/Views/Home/Khoa.cshtml");
        }

        // Lấy tất cả các khoa
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var khoaList = await _context.Khoas.ToListAsync();
            return Json(khoaList);
        }

        // Lấy khoa theo Id
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var khoa = await _context.Khoas.FindAsync(id);
            if (khoa == null)
                return NotFound();
            return Json(khoa);
        }

        // Thêm mới khoa
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            string tenKhoa = Request.Form["TenKhoa"];

            // Kiểm tra tên khoa đã tồn tại
            bool isExisting = await _context.Khoas.AnyAsync(k => k.TenKhoa == tenKhoa);
            if (isExisting)
                return BadRequest("Tên khoa đã tồn tại.");

            var khoa = new Khoa { TenKhoa = tenKhoa };
            _context.Khoas.Add(khoa);
            await _context.SaveChangesAsync();
            return Ok();
        }


        // Cập nhật khoa
        [HttpPut]
        public async Task<IActionResult> Update(int id)
        {
            var khoa = await _context.Khoas.FindAsync(id);
            if (khoa == null)
            {
                return NotFound("Khoa không tồn tại.");
            }

            khoa.TenKhoa = Request.Form["TenKhoa"];

            if (ModelState.IsValid)
            {
                _context.Entry(khoa).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Dữ liệu không hợp lệ.");
        }

        // Xóa khoa
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var khoa = await _context.Khoas.FindAsync(id);
            if (khoa == null)
                return NotFound();

            bool isLinkedToSinhVien = await _context.SinhViens.AnyAsync(sv => sv.MaKhoa == id);
            bool isLinkedToGiangVien = await _context.GiangVienHuongDans.AnyAsync(gv => gv.MaKhoa == id);

            if (isLinkedToSinhVien)
                return BadRequest("Không thể xóa khoa vì có sinh viên thuộc khoa này.");
            if (isLinkedToGiangVien)
                return BadRequest("Không thể xóa khoa vì có giảng viên hướng dẫn thuộc khoa này.");

            _context.Khoas.Remove(khoa);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
