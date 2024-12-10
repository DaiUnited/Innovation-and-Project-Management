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
    public class DeTaiDatGiaiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeTaiDatGiaiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang chính
        public IActionResult DeTaiDatGiai()
        {
            return View("~/Areas/Admin/Views/Home/DeTaiDatGiai.cshtml");
        }

        private async Task<int?> GetMaVongThiAsync(int thuTu, int maCuocThi)
        {
            return await _context.VongThis
                .Where(vt => vt.ThuTu == thuTu && vt.MaCuocThi == maCuocThi)
                .Select(vt => vt.MaVongThi)
                .FirstOrDefaultAsync();
        }

        // GET: Lấy danh sách
        [HttpGet]
        public async Task<IActionResult> GetAll(int? maCuocThi = null)
        {
            var query = _context.DeTaiDatGiais
                .Include(dtg => dtg.DeTai)
                .Include(dtg => dtg.GiaiThuong)
                .AsQueryable();

            // Lọc theo cuộc thi nếu có
            if (maCuocThi.HasValue)
            {
                query = query.Where(dtg => _context.KetQuaVongThis
                    .Any(kq => kq.MaDeTai == dtg.MaDeTai
                               && kq.DeTai.MaCuocThi == maCuocThi
                               && kq.TinhTrang != "Đề tài của nhóm đã bị loại"));
            }

            var deTaiDatGiais = await query
                .Select(dtg => new
                {
                    dtg.MaDeTai,
                    DeTaiTen = dtg.DeTai.TenDeTai,
                    dtg.MaGiaiThuong,
                    GiaiThuongTen = dtg.GiaiThuong.TenGiaiThuong,
                    SoTienThuong = dtg.GiaiThuong.SoTienThuong
                })
                .ToListAsync();

            return Json(deTaiDatGiais);
        }



        // GET: Lấy chi tiết
        [HttpGet]
        public async Task<IActionResult> GetById(int maDeTai, int maGiaiThuong)
        {
            // Kiểm tra dữ liệu có tồn tại trong cơ sở dữ liệu hay không
            var deTaiDatGiai = await _context.DeTaiDatGiais
                .Include(dtg => dtg.DeTai)
                .Include(dtg => dtg.GiaiThuong)
                .FirstOrDefaultAsync(dtg => dtg.MaDeTai == maDeTai && dtg.MaGiaiThuong == maGiaiThuong);

            if (deTaiDatGiai == null)
            {
                return NotFound(new
                {
                    Message = "Không tìm thấy liên kết với các ID được cung cấp.",
                    MaDeTai = maDeTai,
                    MaGiaiThuong = maGiaiThuong
                });
            }

            return Json(new
            {
                deTaiDatGiai.MaDeTai,
                DeTaiTen = deTaiDatGiai.DeTai?.TenDeTai ?? "Không xác định",
                deTaiDatGiai.MaGiaiThuong,
                GiaiThuongTen = deTaiDatGiai.GiaiThuong?.TenGiaiThuong ?? "Không xác định"
            });
        }


        // POST: Thêm liên kết
        [HttpPost]
        public async Task<IActionResult> Create(int maDeTai, int maGiaiThuong, int maCuocThi)
        {
            if (maCuocThi == null)
                return BadRequest("Vui lòng chọn một cuộc thi.");

            // Lấy mã vòng thi chung kết (ThuTu = 3)
            int? maVongChungKet = await GetMaVongThiAsync(3, maCuocThi);
            if (maVongChungKet == null)
                return NotFound("Không tìm thấy vòng chung kết của cuộc thi.");

            // Kiểm tra nếu vòng chung kết đã kết thúc
            //var chungKet = await _context.VongThis.FirstOrDefaultAsync(vt => vt.MaVongThi == maVongChungKet);
            //if (chungKet == null || DateTime.Now <= chungKet.NgayKetThuc)
            //{
            //    return BadRequest("Chỉ có thể trao giải sau khi vòng chung kết kết thúc.");
            //}

            if (!_context.DeTais.Any(dt => dt.MaDeTai == maDeTai))
                return NotFound("Đề tài không tồn tại.");

            if (!_context.GiaiThuongs.Any(gt => gt.MaGiaiThuong == maGiaiThuong))
                return NotFound("Giải thưởng không tồn tại.");

            if (await _context.DeTaiDatGiais.AnyAsync(dtg => dtg.MaDeTai == maDeTai))
            {
                return BadRequest("Đề tài này đã nhận giải thưởng. Không thể thêm giải khác.");
            }

            if (await _context.DeTaiDatGiais.AnyAsync(dtg => dtg.MaGiaiThuong == maGiaiThuong))
            {
                return BadRequest("Giải thưởng này đã được gắn cho một đề tài khác.");
            }

            var deTaiDatGiai = new DeTaiDatGiai
            {
                MaDeTai = maDeTai,
                MaGiaiThuong = maGiaiThuong
            };

            _context.DeTaiDatGiais.Add(deTaiDatGiai);
            await _context.SaveChangesAsync();

            return Ok("Thêm liên kết thành công.");
        }



        // PUT: Cập nhật liên kết
        //[HttpPut]
        //public async Task<IActionResult> Update(int maDeTai, int maGiaiThuong, int newMaDeTai, int newMaGiaiThuong, int maCuocThi)
        //{
        //    if (maCuocThi == null)
        //        return BadRequest("Vui lòng chọn một cuộc thi.");

        //    // Lấy mã vòng thi chung kết (ThuTu = 3)
        //    int? maVongChungKet = await GetMaVongThiAsync(3, maCuocThi);
        //    if (maVongChungKet == null)
        //        return NotFound("Không tìm thấy vòng chung kết của cuộc thi.");

        //    // Kiểm tra nếu vòng chung kết đã kết thúc
        //    //var chungKet = await _context.VongThis.FirstOrDefaultAsync(vt => vt.MaVongThi == maVongChungKet);
        //    //if (chungKet == null || DateTime.Now <= chungKet.NgayKetThuc)
        //    //{
        //    //    return BadRequest("Chỉ có thể cập nhật giải thưởng sau khi vòng chung kết kết thúc.");
        //    //}

        //    var deTaiDatGiai = await _context.DeTaiDatGiais
        //        .FirstOrDefaultAsync(dtg => dtg.MaDeTai == maDeTai && dtg.MaGiaiThuong == maGiaiThuong);

        //    if (deTaiDatGiai == null)
        //        return NotFound("Không tìm thấy liên kết.");

        //    if (!_context.DeTais.Any(dt => dt.MaDeTai == newMaDeTai))
        //        return NotFound("Đề tài mới không tồn tại.");

        //    if (!_context.GiaiThuongs.Any(gt => gt.MaGiaiThuong == newMaGiaiThuong))
        //        return NotFound("Giải thưởng mới không tồn tại.");

        //    if (await _context.DeTaiDatGiais.AnyAsync(dtg => dtg.MaDeTai == newMaDeTai))
        //    {
        //        return BadRequest("Đề tài mới đã nhận giải thưởng. Không thể thêm giải khác.");
        //    }

        //    if (await _context.DeTaiDatGiais.AnyAsync(dtg => dtg.MaGiaiThuong == newMaGiaiThuong))
        //    {
        //        return BadRequest("Giải thưởng này đã được gắn cho một đề tài khác.");
        //    }

        //    deTaiDatGiai.MaDeTai = newMaDeTai;
        //    deTaiDatGiai.MaGiaiThuong = newMaGiaiThuong;

        //    await _context.SaveChangesAsync();

        //    return Ok("Cập nhật thành công.");
        //}



        // DELETE: Xóa liên kết
        [HttpDelete]
        public async Task<IActionResult> Delete(int maDeTai, int maGiaiThuong)
        {
            var deTaiDatGiai = await _context.DeTaiDatGiais
                .FirstOrDefaultAsync(dtg => dtg.MaDeTai == maDeTai && dtg.MaGiaiThuong == maGiaiThuong);

            if (deTaiDatGiai == null)
                return NotFound("Không tìm thấy liên kết.");

            _context.DeTaiDatGiais.Remove(deTaiDatGiai);
            await _context.SaveChangesAsync();

            return Ok("Xóa thành công.");
        }
    }
}
