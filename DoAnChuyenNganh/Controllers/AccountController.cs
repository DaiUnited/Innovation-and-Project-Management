using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

namespace DoAnChuyenNganh.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Tìm người dùng theo tên đăng nhập
            var user = await _context.NguoiDungs
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u => u.TenDangNhap == username);

            // Kiểm tra xem người dùng có tồn tại và mật khẩu có hợp lệ không
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.MatKhau))
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View();
            }

            if (user.VaiTro.TenVaiTro != "Admin")
            {
                // Kiểm tra liên kết với Sinh viên, Giảng viên hướng dẫn hoặc Ban giám khảo
                bool isLinked = await _context.SinhViens.AnyAsync(sv => sv.MaNguoiDung == user.MaNguoiDung)
                    || await _context.GiangVienHuongDans.AnyAsync(gv => gv.MaNguoiDung == user.MaNguoiDung)
                    || await _context.GiamKhaos.AnyAsync(gk => gk.MaNguoiDung == user.MaNguoiDung);

                if (!isLinked)
                {
                    ViewBag.Error = "Tài khoản chưa được liên kết với người dùng nào!";
                    return View();
                }
            }

            // Tạo claims cho người dùng
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim("UserId", user.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Role, user.VaiTro.TenVaiTro)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }




        [HttpGet]
        public IActionResult LogoutAndRedirect()
        {
            return View("LogoutAndRedirect");
        }

        [HttpPost]
        public async Task<IActionResult> LogoutAndRedirectConfirm()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            // Kiểm tra email trong bảng SinhVien
            var sinhVien = await _context.SinhViens
                .Include(sv => sv.NguoiDung)
                .FirstOrDefaultAsync(sv => sv.Email == email);

            if (sinhVien != null)
            {
                // Gửi mã xác nhận cho SinhVien
                return await HandleForgotPassword(sinhVien.NguoiDung, email);
            }

            // Kiểm tra email trong bảng GiangVienHuongDan
            var giangVien = await _context.GiangVienHuongDans
                .Include(gv => gv.NguoiDung)
                .FirstOrDefaultAsync(gv => gv.Email == email);

            if (giangVien != null)
            {
                // Gửi mã xác nhận cho GiangVienHuongDan
                return await HandleForgotPassword(giangVien.NguoiDung, email);
            }

            // Kiểm tra email trong bảng GiamKhao
            var giamKhao = await _context.GiamKhaos
                .Include(gk => gk.NguoiDung)
                .FirstOrDefaultAsync(gk => gk.Email == email);

            if (giamKhao != null)
            {
                // Gửi mã xác nhận cho GiamKhao
                return await HandleForgotPassword(giamKhao.NguoiDung, email);
            }

            ViewBag.Error = "Email không tồn tại trong hệ thống!";
            return View();
        }

        // Phương thức xử lý gửi mã xác nhận
        private async Task<IActionResult> HandleForgotPassword(NguoiDung nguoiDung, string email)
        {
            // Tạo mã xác nhận ngẫu nhiên
            var verificationCode = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            // Lưu mã xác nhận vào bộ nhớ tạm hoặc cơ sở dữ liệu
            TempData["VerificationCode"] = verificationCode;
            TempData["UserId"] = nguoiDung.MaNguoiDung;

            // Gửi mã xác nhận qua email
            await SendVerificationEmail(email, verificationCode);

            ViewBag.Success = "Mã xác nhận đã được gửi tới email của bạn.";
            return RedirectToAction("VerifyCode");
        }

        private async Task SendVerificationEmail(string email, string code)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("daiunited1505@gmail.com", "roon ssdy pmtm twxm"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("daiunited1505@gmail.com"),
                Subject = "Mã xác nhận đặt lại mật khẩu",
                Body = $"Mã xác nhận của bạn là: <b>{code}</b>",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        [HttpGet]
        public IActionResult VerifyCode()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyCode(string code)
        {
            var savedCode = TempData["VerificationCode"]?.ToString();

            if (savedCode == null || code != savedCode)
            {
                ViewBag.Error = "Mã xác nhận không đúng!";
                return View();
            }

            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            var userId = TempData["UserId"];
            if (userId == null) return Unauthorized();

            var user = await _context.NguoiDungs.FindAsync(int.Parse(userId.ToString()));
            if (user == null) return NotFound();

            // Mã hóa mật khẩu mới trước khi lưu
            user.MatKhau = BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _context.SaveChangesAsync();

            ViewBag.Success = "Đặt lại mật khẩu thành công!";
            return RedirectToAction("LogoutAndRedirect");
        }


    }
}