using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DoAnChuyenNganh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DeTaiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeTaiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult DeTai()
        {
            return View("~/Areas/Admin/Views/Home/DeTai.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var deTaiList = await _context.DeTais
                .Include(dt => dt.LinhVuc)
                .Include(dt => dt.CuocThi)
                .Include(dt => dt.Nhom)
                .Include(dt => dt.GiangVienHuongDan)
                .Select(dt => new
                {
                    dt.MaDeTai,
                    dt.TenDeTai,
                    LinhVuc = dt.LinhVuc.TenLinhVuc,
                    CuocThi = dt.CuocThi.TenCuocThi,
                    Nhom = dt.Nhom.TenNhom,
                    GVHD = dt.GiangVienHuongDan.HoTen
                })
                .ToListAsync();

            return Json(deTaiList);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDeTai(int? maCuocThi)
        {
            var deTaiList = await _context.DeTais
                .Include(dt => dt.LinhVuc)
                .Include(dt => dt.CuocThi)
                .Include(dt => dt.Nhom)
                .Include(dt => dt.GiangVienHuongDan)
                .Where(dt => !maCuocThi.HasValue || dt.MaCuocThi == maCuocThi)
                .Select(dt => new
                {
                    dt.MaDeTai,
                    dt.TenDeTai,
                    LinhVuc = dt.LinhVuc.TenLinhVuc,
                    CuocThi = dt.CuocThi.TenCuocThi,
                    Nhom = dt.Nhom.TenNhom,
                    GVHD = dt.GiangVienHuongDan.HoTen
                })
                .ToListAsync();

            return Json(deTaiList);
        }

        private async Task<int?> GetMaVongThiAsync(int thuTu, int maCuocThi)
        {
            return await _context.VongThis
                .Where(vt => vt.ThuTu == thuTu && vt.MaCuocThi == maCuocThi)
                .Select(vt => vt.MaVongThi)
                .FirstOrDefaultAsync();
        }


        [HttpGet]
        public async Task<IActionResult> GetAllSoKhao(int maCuocThi)
        {
            var maVongThi = await GetMaVongThiAsync(1, maCuocThi); // ThuTu = 1 cho Sơ Khảo

            if (maVongThi == null)
            {
                return NotFound("Không tìm thấy vòng Sơ Khảo cho cuộc thi này.");
            }

            var deTaiList = await _context.KetQuaVongThis
                .Where(kq => kq.MaVongThi == maVongThi)
                .Include(kq => kq.DeTai)
                .Select(kq => new
                {
                    kq.DeTai.MaDeTai,
                    kq.DeTai.TenDeTai
                })
                .Distinct()
                .ToListAsync();

            return Json(deTaiList);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBanKet(int maCuocThi)
        {
            var maVongThi = await GetMaVongThiAsync(2, maCuocThi); // ThuTu = 2 cho Bán Kết

            if (maVongThi == null)
            {
                return NotFound("Không tìm thấy vòng Bán Kết cho cuộc thi này.");
            }

            var deTaiList = await _context.KetQuaVongThis
                .Where(kq => kq.MaVongThi == maVongThi)
                .Include(kq => kq.DeTai)
                .Select(kq => new
                {
                    kq.DeTai.MaDeTai,
                    kq.DeTai.TenDeTai
                })
                .Distinct()
                .ToListAsync();

            return Json(deTaiList);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllChungKet(int maCuocThi)
        {
            var maVongThi = await GetMaVongThiAsync(3, maCuocThi); // ThuTu = 3 cho Chung Kết

            if (maVongThi == null)
            {
                return NotFound("Không tìm thấy vòng Chung Kết cho cuộc thi này.");
            }

            var deTaiList = await _context.KetQuaVongThis
                .Where(kq => kq.MaVongThi == maVongThi)
                .Include(kq => kq.DeTai)
                .Select(kq => new
                {
                    kq.DeTai.MaDeTai,
                    kq.DeTai.TenDeTai
                })
                .Distinct()
                .ToListAsync();

            return Json(deTaiList);
        }



        [HttpPost]
        public async Task<IActionResult> Create()
        {
            int maNhom = int.Parse(Request.Form["MaNhom"]);
            int maGVHD = int.Parse(Request.Form["MaGVHD"]);
            string tenDeTai = Request.Form["TenDeTai"];

            // Kiểm tra tên đề tài đã tồn tại
            bool isExisting = await _context.DeTais.AnyAsync(dt => dt.TenDeTai == tenDeTai);
            if (isExisting)
                return BadRequest("Tên đề tài đã tồn tại.");

            // Kiểm tra nếu nhóm đã có đề tài
            if (_context.DeTais.Any(dt => dt.MaNhom == maNhom))
                return BadRequest("Nhóm này đã có đề tài.");

            // Kiểm tra nếu giảng viên đã hướng dẫn đề tài
            if (_context.DeTais.Any(dt => dt.MaGVHD == maGVHD))
                return BadRequest("Giảng viên này đã hướng dẫn một đề tài.");

            // Kiểm tra số lượng thành viên của nhóm
            int soThanhVien = await _context.ThanhVienNhoms.CountAsync(tv => tv.MaNhom == maNhom);
            if (soThanhVien < 3)
                return BadRequest("Nhóm phải có ít nhất 3 thành viên.");

            // Kiểm tra nếu nhóm có trưởng nhóm
            bool coTruongNhom = await _context.ThanhVienNhoms.AnyAsync(tv => tv.MaNhom == maNhom && tv.TruongNhom);
            if (!coTruongNhom)
                return BadRequest("Nhóm phải có 1 trưởng nhóm.");

            var deTai = new DeTai
            {
                TenDeTai = tenDeTai,
                MoTa = Request.Form["MoTa"],
                MaLinhVuc = int.Parse(Request.Form["MaLinhVuc"]),
                MaCuocThi = int.Parse(Request.Form["MaCuocThi"]),
                MaNhom = maNhom,
                MaGVHD = maGVHD
            };

            if (ModelState.IsValid)
            {
                _context.DeTais.Add(deTai);
                await _context.SaveChangesAsync();
                return Ok();
            }

            return BadRequest("Dữ liệu không hợp lệ.");
        }



        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var deTai = await _context.DeTais.FindAsync(id);
            if (deTai == null)
                return NotFound();

            return Json(deTai);
        }

        [HttpPut]
        public async Task<IActionResult> Update(int id, string tenDeTai, string moTa, int maLinhVuc, int maCuocThi, int maNhom, int maGVHD)
        {
            // Tìm đề tài cần cập nhật
            var deTai = await _context.DeTais.FindAsync(id);
            if (deTai == null)
                return NotFound("Đề tài không tồn tại.");

            // Kiểm tra nếu tên đề tài mới bị trùng
            bool isDuplicate = await _context.DeTais.AnyAsync(dt => dt.TenDeTai == tenDeTai && dt.MaDeTai != id);
            if (isDuplicate)
                return BadRequest("Tên đề tài đã tồn tại.");

            // Cập nhật thông tin đề tài
            deTai.TenDeTai = tenDeTai;
            deTai.MoTa = moTa;
            deTai.MaLinhVuc = maLinhVuc;
            deTai.MaCuocThi = maCuocThi;
            deTai.MaNhom = maNhom;
            deTai.MaGVHD = maGVHD;

            // Lưu thay đổi
            _context.Entry(deTai).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Cập nhật đề tài thành công.");
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {

            var deTaiCoKetQua = await _context.KetQuaVongThis.AnyAsync(kq => kq.MaDeTai == id);
            if (deTaiCoKetQua)
                return BadRequest("Không thể xóa đề tài vì đã có kết quả chấm điểm.");

            var deTai = await _context.DeTais.FindAsync(id);
            if (deTai == null)
                return NotFound();

            _context.DeTais.Remove(deTai);
            await _context.SaveChangesAsync();
            return Ok();
        }


    }
}
