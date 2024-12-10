using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnChuyenNganh.Areas.GVHD.Controllers
{
    [Area("GVHD")]
    [Authorize(Roles = "Giảng viên hướng dẫn")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ThongTinCaNhan()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null) return Unauthorized();

            var giangVien = await _context.GiangVienHuongDans
                .Include(gv => gv.Khoa)
                .FirstOrDefaultAsync(gv => gv.MaNguoiDung == int.Parse(userId));

            if (giangVien == null) return NotFound();

            return View(giangVien);
        }

        [HttpPost]
        public async Task<IActionResult> ThongTinCaNhan(GiangVienHuongDan model)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null) return Unauthorized();

            var giangVien = await _context.GiangVienHuongDans
                .FirstOrDefaultAsync(gv => gv.MaNguoiDung == int.Parse(userId));

            if (giangVien == null)
            {
                TempData["Error"] = "Không tìm thấy giảng viên.";
                return RedirectToAction("ThongTinCaNhan");
            }

            // Cập nhật thông tin
            giangVien.Email = model.Email;
            giangVien.SoDienThoai = model.SoDienThoai;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("ThongTinCaNhan");
        }

        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DoiMatKhau(string matKhauCu, string matKhauMoi, string xacNhanMatKhauMoi)
        {
            if (matKhauMoi != xacNhanMatKhauMoi)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới và xác nhận không khớp.");
                return View();
            }

            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null) return Unauthorized();

            var nguoiDung = await _context.NguoiDungs.FindAsync(int.Parse(userId));
            if (nguoiDung == null) return NotFound();

            // Kiểm tra mật khẩu cũ
            if (!BCrypt.Net.BCrypt.Verify(matKhauCu, nguoiDung.MatKhau))
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng.");
                return View();
            }

            // Mã hóa mật khẩu mới
            nguoiDung.MatKhau = BCrypt.Net.BCrypt.HashPassword(matKhauMoi);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công. Hệ thống sẽ tự động đăng xuất.";
            return RedirectToAction("LogoutAndRedirect", "Account", new { area = "" });
        }
    }
}
