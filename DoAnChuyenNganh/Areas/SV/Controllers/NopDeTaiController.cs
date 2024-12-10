using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace DoAnChuyenNganh.Areas.SV.Controllers
{
    [Area("SV")]
    [Authorize(Roles = "Sinh viên")]
    public class NopDeTaiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NopDeTaiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang đề tài của nhóm
        public async Task<IActionResult> ThongTinDeTai()
        {
            int maNguoiDung = int.Parse(User.FindFirstValue("UserId"));

            // Tìm sinh viên
            var sinhVien = await _context.SinhViens.Include(sv => sv.NguoiDung)
                                                   .FirstOrDefaultAsync(sv => sv.MaNguoiDung == maNguoiDung);
            if (sinhVien == null)
            {
                ViewBag.ErrorMessage = "Không tìm thấy sinh viên.";
                return View("~/Areas/SV/Views/Home/ThongTinDeTai.cshtml"); // Hiển thị View với thông báo
            }
            // Lấy nhóm của sinh viên
            var thanhVien = await _context.ThanhVienNhoms.Include(tv => tv.Nhom)
                                                         .FirstOrDefaultAsync(tv => tv.MaSV == sinhVien.MaSV);
            if (thanhVien == null)
            {
                ViewBag.ErrorMessage = "Sinh viên chưa tham gia nhóm.";
                return View("~/Areas/SV/Views/Home/ThongTinDeTai.cshtml"); // Hiển thị View với thông báo
            }

            int maNhom = thanhVien.MaNhom;

            // Lấy thông tin đề tài và vòng thi hiện tại
            var deTaiList = await _context.DeTais
                                          .Where(dt => dt.MaNhom == maNhom)
                                          .Include(dt => dt.CuocThi)
                                          .Include(dt => dt.LinhVuc)
                                          .Include(dt => dt.GiangVienHuongDan)
                                          .Include(dt => dt.BaiNop)
                                          .Select(dt => new
                                          {
                                              dt.MaDeTai,
                                              dt.TenDeTai,
                                              LinhVuc = dt.LinhVuc.TenLinhVuc,
                                              CuocThi = dt.CuocThi.TenCuocThi,
                                              GVHD = dt.GiangVienHuongDan.HoTen,
                                              KetQuas = _context.KetQuaVongThis
                                                  .Where(kq => kq.MaDeTai == dt.MaDeTai)
                                                  .Include(kq => kq.VongThi)
                                                  .Select(kq => new
                                                  {
                                                      kq.VongThi.TenVongThi,
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
                                                  }).FirstOrDefault(),
                                              VongThiHienTai = _context.VongThis
                                                  .Where(vt => vt.MaCuocThi == dt.MaCuocThi && vt.NgayBatDau <= DateTime.Now && vt.NgayKetThuc >= DateTime.Now)
                                                  .OrderBy(vt => vt.ThuTu)
                                                  .FirstOrDefault()
                                          })
                                          .ToListAsync();

            ViewBag.IsTruongNhom = thanhVien.TruongNhom;
            return View("~/Areas/SV/Views/Home/ThongTinDeTai.cshtml", deTaiList);
        }




        // Trang nộp bài
        [HttpGet]
        public async Task<IActionResult> NopBai(int maDeTai)
        {
            // Lấy thông tin đề tài cùng với các liên kết tới CuocThi, LinhVuc, Nhom, và GiangVienHuongDan
            var deTai = await _context.DeTais
                .Include(dt => dt.CuocThi)
                .Include(dt => dt.LinhVuc)
                .Include(dt => dt.Nhom)
                .Include(dt => dt.GiangVienHuongDan)
                .FirstOrDefaultAsync(dt => dt.MaDeTai == maDeTai);

            if (deTai == null)
                return NotFound("Không tìm thấy đề tài.");

            return View("~/Areas/SV/Views/Home/NopBai.cshtml", deTai);
        }


        [HttpPost]
        public async Task<IActionResult> NopBai(int maDeTai, List<IFormFile> files)
        {
            int maNguoiDung = int.Parse(User.FindFirstValue("UserId"));

            // Tìm sinh viên và nhóm
            var sinhVien = await _context.SinhViens.FirstOrDefaultAsync(sv => sv.MaNguoiDung == maNguoiDung);
            if (sinhVien == null)
            {
                return BadRequest("Không tìm thấy thông tin sinh viên.");
            }

            var thanhVien = await _context.ThanhVienNhoms.FirstOrDefaultAsync(tv => tv.MaSV == sinhVien.MaSV);
            if (thanhVien == null || !thanhVien.TruongNhom)
            {
                return BadRequest("Chỉ nhóm trưởng mới có quyền nộp bài.");
            }

            // Kiểm tra vòng thi hiện tại
            var deTai = await _context.DeTais.Include(dt => dt.CuocThi).FirstOrDefaultAsync(dt => dt.MaDeTai == maDeTai);
            if (deTai == null)
            {
                return NotFound("Không tìm thấy đề tài.");
            }

            //var vongThiHienTai = await _context.VongThis
            //    .Where(vt => vt.MaCuocThi == deTai.MaCuocThi && vt.NgayBatDau <= DateTime.Now && vt.NgayKetThuc >= DateTime.Now)
            //    .OrderBy(vt => vt.ThuTu)
            //    .FirstOrDefaultAsync();

            //if (vongThiHienTai == null)
            //{
            //    TempData["ErrorMessage"] = "Vòng thi chưa diễn ra hoặc đã kết thúc, không thể nộp bài.";
            //    return RedirectToAction("ThongTinDeTai");
            //}

            // Tìm hoặc tạo mới bài nộp
            var baiNop = await _context.BaiNops.FirstOrDefaultAsync(bn => bn.MaDeTai == maDeTai && bn.MaNhom == thanhVien.MaNhom);
            if (baiNop == null)
            {
                baiNop = new BaiNop
                {
                    MaDeTai = maDeTai,
                    MaNhom = thanhVien.MaNhom,
                    NgayNop = DateTime.Now,
                    TrangThai = "Đã nộp"
                };
                _context.BaiNops.Add(baiNop);
            }
            else
            {
                baiNop.NgayChinhSua = DateTime.Now;
                baiNop.TrangThai = "Đã chỉnh sửa";
            }

            // Xử lý lưu file
            if (files != null && files.Count > 0)
            {
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files");
                Directory.CreateDirectory(uploadPath); // Tạo thư mục nếu chưa tồn tại

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Tạo tên file độc nhất để tránh trùng lặp
                        string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                        string filePath = Path.Combine(uploadPath, uniqueFileName);

                        // Lưu file vào thư mục
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Lưu tên file vào cột FileDinhKem (lưu tên cuối cùng nếu upload nhiều file)
                        baiNop.FileDinhKem = uniqueFileName;
                    }
                }
            }
            else
            {
                return BadRequest("Bạn phải tải lên ít nhất một file.");
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Nộp bài thành công!";
            return RedirectToAction("ThongTinDeTai");
        }

    }
}
