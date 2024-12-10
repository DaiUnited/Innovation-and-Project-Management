using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;

namespace DoAnChuyenNganh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lớp cho thống kê đề tài đạt giải
        public class ThongKeDeTaiDatGiaiDto
        {
            public string TenCuocThi { get; set; }
            public int SoLuongDeTaiDatGiai { get; set; }
            public decimal TongTienThuong { get; set; }
        }

        // Lớp cho thống kê sinh viên theo cuộc thi
        public class ThongKeSinhVienDto
        {
            public int MaCuocThi { get; set; }
            public string TenCuocThi { get; set; }
            public int SoLuongSinhVien { get; set; }
        }

        // Lớp cho thống kê nhóm theo cuộc thi
        public class ThongKeNhomDto
        {
            public int MaCuocThi { get; set; }
            public string TenCuocThi { get; set; }
            public int SoLuongNhom { get; set; }
        }

        // Trang chính thống kê
        public IActionResult ThongKe()
        {
            return View("~/Areas/Admin/Views/Home/ThongKe.cshtml");
        }

        public async Task<IActionResult> ExportThongKeToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Tải dữ liệu thống kê từ các phương thức đã có
            var tongSoLuong = await GetTongSoLuongData();
            var deTaiDatGiai = await GetThongKeDeTaiDatGiaiData();
            var sinhVienTheoCuocThi = await GetThongKeSinhVienTheoCuocThiData();
            var nhomTheoCuocThi = await GetThongKeNhomTheoCuocThiData();

            using (var package = new ExcelPackage())
            {
                // Tạo worksheet
                var worksheet = package.Workbook.Worksheets.Add("Thống Kê Toàn Hệ Thống");

                // Tạo tiêu đề chính
                worksheet.Cells["A1:E1"].Merge = true;
                worksheet.Cells["A1"].Value = "BÁO CÁO THỐNG KÊ TOÀN HỆ THỐNG";
                worksheet.Cells["A1"].Style.Font.Size = 18;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);

                // Tổng quan
                worksheet.Cells["A3"].Value = "Tổng Quan";
                worksheet.Cells["A3"].Style.Font.Size = 14;
                worksheet.Cells["A3"].Style.Font.Bold = true;

                worksheet.Cells["A4"].Value = "Tổng Số Đề Tài:";
                worksheet.Cells["B4"].Value = tongSoLuong.TongDeTai;
                worksheet.Cells["A5"].Value = "Tổng Số Cuộc Thi:";
                worksheet.Cells["B5"].Value = tongSoLuong.TongCuocThi;
                worksheet.Cells["A6"].Value = "Tổng Số Giải Thưởng:";
                worksheet.Cells["B6"].Value = tongSoLuong.TongGiaiThuong;
                worksheet.Cells["A7"].Value = "Tổng Số Giám Khảo:";
                worksheet.Cells["B7"].Value = tongSoLuong.TongGiamKhao;

                worksheet.Cells["A4:B7"].Style.Font.Size = 12;
                worksheet.Cells["A4:A7"].Style.Font.Bold = true;

                // Đề tài đạt giải
                worksheet.Cells["A9"].Value = "Thống Kê Đề Tài Đạt Giải";
                worksheet.Cells["A9"].Style.Font.Size = 14;
                worksheet.Cells["A9"].Style.Font.Bold = true;

                worksheet.Cells["A10"].Value = "Tên Cuộc Thi";
                worksheet.Cells["B10"].Value = "Số Lượng Đề Tài Đạt Giải";
                worksheet.Cells["C10"].Value = "Tổng Tiền Thưởng";

                worksheet.Cells["A10:C10"].Style.Font.Bold = true;
                worksheet.Cells["A10:C10"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A10:C10"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(237, 125, 49));
                worksheet.Cells["A10:C10"].Style.Font.Color.SetColor(Color.White);
                worksheet.Cells["A10:C10"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                int row = 11;
                foreach (var item in deTaiDatGiai)
                {
                    worksheet.Cells[$"A{row}"].Value = item.TenCuocThi;
                    worksheet.Cells[$"B{row}"].Value = item.SoLuongDeTaiDatGiai;
                    worksheet.Cells[$"C{row}"].Value = item.TongTienThuong;
                    worksheet.Cells[$"C{row}"].Style.Numberformat.Format = "#,##0 VND";
                    row++;
                }

                // Sinh viên và nhóm theo cuộc thi
                worksheet.Cells[$"A{row + 1}"].Value = "Thống Kê Sinh Viên và Nhóm Theo Cuộc Thi";
                worksheet.Cells[$"A{row + 1}"].Style.Font.Size = 14;
                worksheet.Cells[$"A{row + 1}"].Style.Font.Bold = true;

                worksheet.Cells[$"A{row + 2}"].Value = "Tên Cuộc Thi";
                worksheet.Cells[$"B{row + 2}"].Value = "Số Lượng Sinh Viên";
                worksheet.Cells[$"C{row + 2}"].Value = "Số Lượng Nhóm";

                worksheet.Cells[$"A{row + 2}:C{row + 2}"].Style.Font.Bold = true;
                worksheet.Cells[$"A{row + 2}:C{row + 2}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[$"A{row + 2}:C{row + 2}"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(112, 173, 71));
                worksheet.Cells[$"A{row + 2}:C{row + 2}"].Style.Font.Color.SetColor(Color.White);
                worksheet.Cells[$"A{row + 2}:C{row + 2}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                row += 3;
                foreach (var item in sinhVienTheoCuocThi)
                {
                    var nhom = nhomTheoCuocThi.FirstOrDefault(n => n.MaCuocThi == item.MaCuocThi);
                    worksheet.Cells[$"A{row}"].Value = item.TenCuocThi;
                    worksheet.Cells[$"B{row}"].Value = item.SoLuongSinhVien;
                    worksheet.Cells[$"C{row}"].Value = nhom?.SoLuongNhom ?? 0;
                    row++;
                }

                // Viền bảng
                var modelRange = worksheet.Cells[$"A10:C{row - 1}"];
                modelRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                modelRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                modelRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                modelRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                // Auto-fit
                worksheet.Cells.AutoFitColumns();

                // Xuất file Excel
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ThongKeChuyenNghiep.xlsx");
            }
        }

        private async Task<dynamic> GetTongSoLuongData()
        {
            return new
            {
                TongDeTai = await _context.DeTais.CountAsync(),
                TongCuocThi = await _context.CuocThis.CountAsync(),
                TongGiaiThuong = await _context.GiaiThuongs.CountAsync(),
                TongGiamKhao = await _context.GiamKhaos.CountAsync()
            };
        }

        private async Task<List<ThongKeDeTaiDatGiaiDto>> GetThongKeDeTaiDatGiaiData()
        {
            return await _context.DeTaiDatGiais
                .Include(dtg => dtg.DeTai)
                .Include(dtg => dtg.DeTai.CuocThi)
                .Include(dtg => dtg.GiaiThuong)
                .GroupBy(dtg => dtg.DeTai.MaCuocThi)
                .Select(group => new ThongKeDeTaiDatGiaiDto
                {
                    TenCuocThi = group.FirstOrDefault().DeTai.CuocThi.TenCuocThi,
                    SoLuongDeTaiDatGiai = group.Count(),
                    TongTienThuong = group.Sum(dtg => dtg.GiaiThuong.SoTienThuong)
                })
                .ToListAsync();
        }

        private async Task<List<ThongKeSinhVienDto>> GetThongKeSinhVienTheoCuocThiData()
        {
            return await _context.CuocThis.Select(ct => new ThongKeSinhVienDto
            {
                MaCuocThi = ct.MaCuocThi,
                TenCuocThi = ct.TenCuocThi,
                SoLuongSinhVien = _context.ThanhVienNhoms
                    .Count(tv => _context.Nhoms
                        .Any(n => n.MaNhom == tv.MaNhom && _context.DeTais.Any(dt => dt.MaNhom == n.MaNhom && dt.MaCuocThi == ct.MaCuocThi)))
            }).ToListAsync();
        }

        private async Task<List<ThongKeNhomDto>> GetThongKeNhomTheoCuocThiData()
        {
            return await _context.CuocThis.Select(ct => new ThongKeNhomDto
            {
                MaCuocThi = ct.MaCuocThi,
                TenCuocThi = ct.TenCuocThi,
                SoLuongNhom = _context.Nhoms
                    .Count(n => _context.DeTais.Any(dt => dt.MaNhom == n.MaNhom && dt.MaCuocThi == ct.MaCuocThi))
            }).ToListAsync();
        }

        // 1. Tổng số lượng đề tài, giải thưởng, cuộc thi, giám khảo
        [HttpGet]
        public async Task<IActionResult> GetTongSoLuong()
        {
            var tongSoLuong = new
            {
                TongDeTai = await _context.DeTais.CountAsync(),
                TongCuocThi = await _context.CuocThis.CountAsync(),
                TongGiaiThuong = await _context.GiaiThuongs.CountAsync(),
                TongGiamKhao = await _context.GiamKhaos.CountAsync()
            };

            return Json(tongSoLuong);
        }

        // 2. Thống kê số lượng đề tài đạt giải theo cuộc thi
        [HttpGet]
        public async Task<IActionResult> ThongKeDeTaiDatGiai()
        {
            var thongKe = await _context.DeTaiDatGiais
                .Include(dtg => dtg.DeTai)
                .Include(dtg => dtg.DeTai.CuocThi)
                .Include(dtg => dtg.GiaiThuong)
                .GroupBy(dtg => dtg.DeTai.MaCuocThi)
                .Select(group => new
                {
                    MaCuocThi = group.Key,
                    TenCuocThi = group.FirstOrDefault().DeTai.CuocThi.TenCuocThi,
                    SoLuongDeTaiDatGiai = group.Count(),
                    TongTienThuong = group.Sum(dtg => dtg.GiaiThuong.SoTienThuong)
                })
                .ToListAsync();

            return Json(thongKe);
        }

        // 3. Thống kê số lượng giám khảo tham gia chấm điểm theo vòng thi
        [HttpGet]
        public async Task<IActionResult> ThongKeGiamKhaoTheoVongThi()
        {
            var thongKe = await _context.KetQuaVongThis
                .Include(kq => kq.VongThi)
                .Include(kq => kq.GiamKhao)
                .GroupBy(kq => kq.MaVongThi)
                .Select(group => new
                {
                    MaVongThi = group.Key,
                    TenVongThi = group.FirstOrDefault().VongThi.TenVongThi,
                    SoLuongGiamKhao = group.Select(kq => kq.MaGiamKhao).Distinct().Count()
                })
                .ToListAsync();

            return Json(thongKe);
        }

        // 4. Thống kê tổng số tiền thưởng đã trao theo từng cuộc thi
        [HttpGet]
        public async Task<IActionResult> ThongKeTongTienThuongTheoCuocThi()
        {
            var thongKe = await _context.DeTaiDatGiais
                .Include(dtg => dtg.DeTai)
                .Include(dtg => dtg.DeTai.CuocThi)
                .Include(dtg => dtg.GiaiThuong)
                .GroupBy(dtg => dtg.DeTai.MaCuocThi)
                .Select(group => new
                {
                    MaCuocThi = group.Key,
                    TenCuocThi = group.FirstOrDefault().DeTai.CuocThi.TenCuocThi,
                    TongTienThuong = group.Sum(dtg => dtg.GiaiThuong.SoTienThuong)
                })
                .ToListAsync();

            return Json(thongKe);
        }

        // 5. Thống kê số lượng đề tài theo từng lĩnh vực
        [HttpGet]
        public async Task<IActionResult> ThongKeDeTaiTheoLinhVuc()
        {
            var thongKe = await _context.DeTais
                .Include(dt => dt.LinhVuc)
                .GroupBy(dt => dt.MaLinhVuc)
                .Select(group => new
                {
                    MaLinhVuc = group.Key,
                    TenLinhVuc = group.FirstOrDefault().LinhVuc.TenLinhVuc,
                    SoLuongDeTai = group.Count()
                })
                .ToListAsync();

            return Json(thongKe);
        }

        // 6. Thống kê các cuộc thi có số lượng đề tài nhiều nhất
        [HttpGet]
        public async Task<IActionResult> ThongKeCuocThiSoLuongDeTai()
        {
            var thongKe = await _context.DeTais
                .Include(dt => dt.CuocThi)
                .GroupBy(dt => dt.MaCuocThi)
                .Select(group => new
                {
                    MaCuocThi = group.Key,
                    TenCuocThi = group.FirstOrDefault().CuocThi.TenCuocThi,
                    SoLuongDeTai = group.Count()
                })
                .OrderByDescending(group => group.SoLuongDeTai)
                .Take(5) // Top 5 cuộc thi
                .ToListAsync();

            return Json(thongKe);
        }

        // 7. Thống kê số lượng đề tài bị loại qua các vòng thi
        [HttpGet]
        public async Task<IActionResult> ThongKeDeTaiBiLoai()
        {
            var thongKe = await _context.KetQuaVongThis
                .Include(kq => kq.VongThi)
                .Where(kq => kq.TinhTrang.Contains("bị loại"))
                .GroupBy(kq => kq.MaVongThi)
                .Select(group => new
                {
                    MaVongThi = group.Key,
                    TenVongThi = group.FirstOrDefault().VongThi.TenVongThi,
                    SoLuongDeTaiBiLoai = group.Count()
                })
                .ToListAsync();

            return Json(thongKe);
        }

        // 8. Thống kê tỷ lệ đạt giải trên tổng số đề tài theo từng cuộc thi
        [HttpGet]
        public async Task<IActionResult> ThongKeTiLeDatGiai()
        {
            var thongKe = await _context.CuocThis
                .Select(ct => new
                {
                    MaCuocThi = ct.MaCuocThi,
                    TenCuocThi = ct.TenCuocThi,
                    TongDeTai = _context.DeTais.Count(dt => dt.MaCuocThi == ct.MaCuocThi),
                    SoLuongDatGiai = _context.DeTaiDatGiais
                        .Count(dtg => dtg.DeTai.MaCuocThi == ct.MaCuocThi)
                })
                .Select(result => new
                {
                    result.MaCuocThi,
                    result.TenCuocThi,
                    TiLeDatGiai = result.TongDeTai > 0 ? (result.SoLuongDatGiai * 100.0 / result.TongDeTai).ToString("0.00") + "%" : "0.00%"
                })
                .ToListAsync();

            return Json(thongKe);
        }

        // 9. Thống kê số lượng sinh viên tham gia theo từng cuộc thi
        [HttpGet]
        public async Task<IActionResult> ThongKeSoLuongSinhVienTheoCuocThi()
        {
            var thongKe = await _context.CuocThis
                .Select(ct => new
                {
                    MaCuocThi = ct.MaCuocThi,
                    TenCuocThi = ct.TenCuocThi,
                    SoLuongSinhVien = _context.ThanhVienNhoms
                        .Count(tv => _context.Nhoms.Any(n => n.MaNhom == tv.MaNhom && _context.DeTais.Any(dt => dt.MaNhom == n.MaNhom && dt.MaCuocThi == ct.MaCuocThi)))
                })
                .ToListAsync();

            return Json(thongKe);
        }

        // 10. Thống kê số lượng nhóm tham gia theo từng cuộc thi
        [HttpGet]
        public async Task<IActionResult> ThongKeSoLuongNhomTheoCuocThi()
        {
            var thongKe = await _context.CuocThis
                .Select(ct => new
                {
                    MaCuocThi = ct.MaCuocThi,
                    TenCuocThi = ct.TenCuocThi,
                    SoLuongNhom = _context.Nhoms
                        .Count(n => _context.DeTais.Any(dt => dt.MaNhom == n.MaNhom && dt.MaCuocThi == ct.MaCuocThi))
                })
                .ToListAsync();

            return Json(thongKe);
        }

        // 11. Tổng số lượng sinh viên và nhóm tham gia
        [HttpGet]
        public async Task<IActionResult> TongSoLuongSinhVienVaNhom()
        {
            var tongSoLuong = new
            {
                TongSoSinhVien = await _context.ThanhVienNhoms.Select(tv => tv.MaSV).Distinct().CountAsync(),
                TongSoNhom = await _context.Nhoms.CountAsync()
            };

            return Json(tongSoLuong);
        }

        // 12. Thống kê số lượng sinh viên và giảng viên hướng dẫn theo khoa
        [HttpGet]
        public async Task<IActionResult> ThongKeSinhVienVaGiangVienTheoKhoa()
        {
            var thongKe = await _context.Khoas
                .Select(k => new
                {
                    TenKhoa = k.TenKhoa,
                    SoLuongSinhVien = _context.SinhViens.Count(sv => sv.MaKhoa == k.MaKhoa),
                    SoLuongGiangVien = _context.GiangVienHuongDans.Count(gv => gv.MaKhoa == k.MaKhoa)
                })
                .ToListAsync();

            return Json(thongKe);
        }

        // 13. Tổng số lượng khoa và lĩnh vực
        [HttpGet]
        public async Task<IActionResult> GetTongSoLuongKhoaVaLinhVuc()
        {
            var thongKe = new
            {
                SoLuongKhoa = await _context.Khoas.CountAsync(),
                SoLuongLinhVuc = await _context.LinhVucs.CountAsync()
            };

            return Json(thongKe);
        }


    }
}
