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
    public class NguoiDungController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NguoiDungController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action để load trang NguoiDung.cshtml
        public IActionResult NguoiDung()
        {
            return View("~/Areas/Admin/Views/Home/NguoiDung.cshtml");
        }

        // GET: Lấy tất cả người dùng
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var nguoiDungList = await _context.NguoiDungs.Include(nd => nd.VaiTro).ToListAsync();
            return Json(nguoiDungList);
        }

        // GET: Lấy người dùng theo Id
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            return Json(nguoiDung);
        }

        // POST: Thêm mới người dùng
        [HttpPost]
        public async Task<IActionResult> Create(string TenDangNhap, string MatKhau, int MaVaiTro)
        {
            // Kiểm tra nếu tên đăng nhập đã tồn tại
            bool isUsernameTaken = await _context.NguoiDungs.AnyAsync(nd => nd.TenDangNhap == TenDangNhap);
            if (isUsernameTaken)
            {
                return BadRequest("Tên đăng nhập đã tồn tại.");
            }

            // Mã hóa mật khẩu
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(MatKhau);

            var nguoiDung = new NguoiDung
            {
                TenDangNhap = TenDangNhap,
                MatKhau = hashedPassword, // Lưu mật khẩu đã mã hóa
                MaVaiTro = MaVaiTro
            };

            _context.NguoiDungs.Add(nguoiDung);
            await _context.SaveChangesAsync();

            return Ok();
        }


        // GET: Lấy người dùng theo vai trò
        [HttpGet]
        public async Task<IActionResult> GetByRole(string roleName)
        {
            var users = await _context.NguoiDungs
                .Include(nd => nd.VaiTro)
                .Where(nd => nd.VaiTro.TenVaiTro == roleName)
                .Select(nd => new
                {
                    nd.MaNguoiDung,
                    nd.TenDangNhap
                })
                .ToListAsync();

            return Json(users);
        }


        //// PUT: Cập nhật người dùng
        //[HttpPut]
        //public async Task<IActionResult> Update(int id, string TenDangNhap, string MatKhau, int MaVaiTro)
        //{
        //    var nguoiDung = await _context.NguoiDungs.FindAsync(id);
        //    if (nguoiDung == null)
        //        return NotFound();

        //    // Kiểm tra nếu tên đăng nhập đã tồn tại cho người dùng khác
        //    bool isUsernameTaken = await _context.NguoiDungs.AnyAsync(nd => nd.TenDangNhap == TenDangNhap && nd.MaNguoiDung != id);
        //    if (isUsernameTaken)
        //    {
        //        return BadRequest("Tên đăng nhập đã tồn tại cho người dùng khác.");
        //    }

        //    nguoiDung.TenDangNhap = TenDangNhap;
        //    nguoiDung.MatKhau = MatKhau;
        //    nguoiDung.MaVaiTro = MaVaiTro;
        //    _context.Entry(nguoiDung).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();
        //    return Ok();
        //}

        [HttpPost]
        public async Task<IActionResult> RecoverPassword(int id)
        {
            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            // Mã hóa mật khẩu mặc định
            nguoiDung.MatKhau = BCrypt.Net.BCrypt.HashPassword("pass123");

            _context.Entry(nguoiDung).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Mật khẩu đã được khôi phục về mặc định.");
        }


        // DELETE: Xóa người dùng
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null)
                return NotFound();

            // Kiểm tra nếu người dùng đang liên kết với Sinh viên
            bool isLinkedToSinhVien = await _context.SinhViens.AnyAsync(sv => sv.MaNguoiDung == id);
            if (isLinkedToSinhVien)
            {
                return BadRequest("Không thể xóa người dùng này vì đang được liên kết với Sinh viên.");
            }

            // Kiểm tra nếu người dùng đang liên kết với Giảng viên hướng dẫn
            bool isLinkedToGiangVien = await _context.GiangVienHuongDans.AnyAsync(gv => gv.MaNguoiDung == id);
            if (isLinkedToGiangVien)
            {
                return BadRequest("Không thể xóa người dùng này vì đang được liên kết với Giảng viên hướng dẫn.");
            }

            // Kiểm tra nếu người dùng đang liên kết với Giám khảo
            bool isLinkedToGiamKhao = await _context.GiamKhaos.AnyAsync(gk => gk.MaNguoiDung == id);
            if (isLinkedToGiamKhao)
            {
                return BadRequest("Không thể xóa người dùng này vì đang được liên kết với Giám khảo.");
            }

            // Xóa người dùng nếu không liên kết với bất kỳ ai
            _context.NguoiDungs.Remove(nguoiDung);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
