using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.IO;

namespace DoAnChuyenNganh.Areas.BGK.Controllers
{
    [Area("BGK")]
    [Authorize(Roles = "Ban giám khảo")]
    public class ChamDiemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChamDiemController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách đề tài cần chấm điểm
        public async Task<IActionResult> ChamDiem()
        {
            int maNguoiDung = int.Parse(User.FindFirstValue("UserId"));

            // Lấy MaGiamKhao dựa trên MaNguoiDung
            var giamKhao = await _context.GiamKhaos.FirstOrDefaultAsync(gk => gk.MaNguoiDung == maNguoiDung);
            if (giamKhao == null)
                return NotFound("Không tìm thấy giám khảo");

            int maGiamKhao = giamKhao.MaGiamKhao;

            // Lấy danh sách kết quả chấm điểm của giám khảo
            var ketQuaList = await _context.KetQuaVongThis
                .Include(kq => kq.DeTai)
                .ThenInclude(dt => dt.BaiNop) // Lấy thông tin bài nộp
                .Include(kq => kq.VongThi)
                .Where(kq => kq.MaGiamKhao == maGiamKhao)
                .ToListAsync();

            return View("~/Areas/BGK/Views/Home/ChamDiem.cshtml", ketQuaList);
        }

        // Phương thức POST để lưu điểm và nhận xét
        [HttpPost]
        public async Task<IActionResult> LuuDiem(int maKetQua, float diemSangTao, float diemKhaThi, float diemTacDongXaHoi, float diemTiemNangThiTruong, string nhanXet)
        {
            var ketQua = await _context.KetQuaVongThis.FindAsync(maKetQua);
            if (ketQua == null)
                return NotFound("Không tìm thấy kết quả.");

            // Cập nhật điểm và nhận xét
            ketQua.DiemSangTao = diemSangTao;
            ketQua.DiemKhaThi = diemKhaThi;
            ketQua.DiemTacDongXaHoi = diemTacDongXaHoi;
            ketQua.DiemTiemNangThiTruong = diemTiemNangThiTruong;
            ketQua.NhanXet = nhanXet;
            ketQua.NgayNhanXet = DateTime.Now;
            ketQua.TinhTrang = "Đã chấm";

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Lưu điểm và nhận xét thành công!";
            return RedirectToAction("ChamDiem");
        }

        // Phương thức để tải file bài nộp
        [HttpGet]
        public IActionResult DownloadFile(int maDeTai)
        {
            var baiNop = _context.BaiNops.FirstOrDefault(bn => bn.MaDeTai == maDeTai);
            if (baiNop == null || string.IsNullOrEmpty(baiNop.FileDinhKem))
                return NotFound("Không tìm thấy file đính kèm.");

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", baiNop.FileDinhKem);
            if (!System.IO.File.Exists(filePath))
                return NotFound("File không tồn tại.");

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", baiNop.FileDinhKem);
        }
    }
}
