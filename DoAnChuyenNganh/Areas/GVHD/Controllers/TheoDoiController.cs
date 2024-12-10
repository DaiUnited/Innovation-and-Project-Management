using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace DoAnChuyenNganh.Areas.GVHD.Controllers
{
    [Area("GVHD")]
    [Authorize(Roles = "Giảng viên hướng dẫn")]
    public class TheoDoiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TheoDoiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang xem thông tin đề tài
        public async Task<IActionResult> ThongTinDeTai()
        {
            int maNguoiDung = int.Parse(User.FindFirstValue("UserId"));

            // Tìm giảng viên hướng dẫn dựa trên MaNguoiDung
            var giangVien = await _context.GiangVienHuongDans.FirstOrDefaultAsync(gv => gv.MaNguoiDung == maNguoiDung);
            if (giangVien == null)
            {
                ViewBag.Message = "Không tìm thấy giảng viên hướng dẫn.";
                return View("~/Areas/GVHD/Views/Home/ThongTinDeTai.cshtml");
            }


            int maGVHD = giangVien.MaGVHD;

            // Lấy danh sách đề tài do giảng viên hướng dẫn
            var deTaiList = await _context.DeTais
                .Where(dt => dt.MaGVHD == maGVHD)
                .Include(dt => dt.CuocThi)
                .Include(dt => dt.LinhVuc)
                .Include(dt => dt.Nhom)
                .Include(dt => dt.BaiNop)
                .Select(dt => new
                {
                    dt.MaDeTai,
                    dt.TenDeTai,
                    LinhVuc = dt.LinhVuc.TenLinhVuc,
                    CuocThi = dt.CuocThi.TenCuocThi,
                    Nhom = dt.Nhom.TenNhom,
                    KetQuas = _context.KetQuaVongThis
                        .Where(kq => kq.MaDeTai == dt.MaDeTai)
                        .Include(kq => kq.VongThi)
                        .Include(kq => kq.GiamKhao)
                        .Select(kq => new
                        {
                            kq.MaKetQua,
                            kq.VongThi.TenVongThi,
                            kq.GiamKhao.HoTen,
                            kq.DiemSangTao,
                            kq.DiemKhaThi,
                            kq.DiemTacDongXaHoi,
                            kq.DiemTiemNangThiTruong,
                            kq.TongDiem,
                            kq.XepHang,
                            kq.NhanXet,
                            kq.NgayNhanXet,
                            kq.TinhTrang
                        }).ToList(),
                    BaiNop = _context.BaiNops
                        .Where(bn => bn.MaDeTai == dt.MaDeTai)
                        .Select(bn => new
                        {
                            bn.FileDinhKem,
                            bn.TrangThai,
                            bn.NgayNop,
                            bn.NgayChinhSua
                        }).FirstOrDefault()
                })
                .ToListAsync();
            if (!deTaiList.Any())
            {
                ViewBag.Message = "Không có đề tài nào để hiển thị.";
            }

            return View("~/Areas/GVHD/Views/Home/ThongTinDeTai.cshtml", deTaiList);
        }

        // Phương thức để tải file bài nộp
        [HttpGet]
        public IActionResult DownloadFile(int maDeTai)
        {
            var baiNop = _context.BaiNops.FirstOrDefault(bn => bn.MaDeTai == maDeTai);
            if (baiNop == null || string.IsNullOrEmpty(baiNop.FileDinhKem))
                return NotFound("Không tìm thấy file đính kèm.");

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", baiNop.FileDinhKem);
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", baiNop.FileDinhKem);
        }

    }
}
