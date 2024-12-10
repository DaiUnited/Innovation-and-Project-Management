using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DoAnChuyenNganh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class VongThiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VongThiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action để load trang VongThi.cshtml
        public IActionResult VongThi()
        {
            return View("~/Areas/Admin/Views/Home/VongThi.cshtml");
        }

        // GET: Lấy tất cả các vòng thi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vongThiList = await _context.VongThis
                .Include(vt => vt.CuocThi)
                .ToListAsync();
            return Json(vongThiList);
        }

        // GET: Lấy vòng thi theo Id
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var vongThi = await _context.VongThis.FindAsync(id);
            if (vongThi == null)
                return NotFound();
            return Json(vongThi);
        }

        // POST: Thêm mới vòng thi
        //[HttpPost]
        //public async Task<IActionResult> Create(int maCuocThi, string tenVongThi, int thuTu, string ngayBatDau, string ngayKetThuc)
        //{
        //    var vongThi = new VongThi
        //    {
        //        MaCuocThi = maCuocThi,
        //        TenVongThi = tenVongThi,
        //        ThuTu = thuTu,
        //        NgayBatDau = DateTime.Parse(ngayBatDau),
        //        NgayKetThuc = DateTime.Parse(ngayKetThuc)
        //    };

        //    _context.VongThis.Add(vongThi);
        //    await _context.SaveChangesAsync();
        //    return Ok();
        //}

        [HttpPost]
        public async Task<IActionResult> Create(int maCuocThi, string tenVongThi, int thuTu, string ngayBatDau, string ngayKetThuc)
        {
            DateTime ngayBatDauParsed = DateTime.Parse(ngayBatDau);
            DateTime ngayKetThucParsed = DateTime.Parse(ngayKetThuc);

            // 1. Kiểm tra ngày bắt đầu phải nhỏ hơn ngày kết thúc
            if (ngayBatDauParsed >= ngayKetThucParsed)
            {
                return BadRequest("Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");
            }

            // 2. Kiểm tra ngày diễn ra vòng thi nằm trong khoảng thời gian của cuộc thi
            var cuocThi = await _context.CuocThis.FindAsync(maCuocThi);
            if (cuocThi == null)
            {
                return NotFound("Cuộc thi không tồn tại.");
            }

            if (ngayBatDauParsed < cuocThi.NgayBatDau || ngayKetThucParsed > cuocThi.NgayKetThuc)
            {
                return BadRequest("Thời gian vòng thi phải nằm trong khoảng thời gian của cuộc thi.");
            }

            // 3. Kiểm tra ngày bắt đầu của vòng thi tiếp theo lớn hơn ngày kết thúc của vòng thi hiện tại
            var vongThiTruoc = await _context.VongThis
                .Where(vt => vt.MaCuocThi == maCuocThi && vt.ThuTu < thuTu)
                .OrderByDescending(vt => vt.ThuTu)
                .FirstOrDefaultAsync();

            if (vongThiTruoc != null && ngayBatDauParsed <= vongThiTruoc.NgayKetThuc)
            {
                return BadRequest("Ngày bắt đầu của vòng thi phải lớn hơn ngày kết thúc của vòng thi trước đó.");
            }

            // 4. Kiểm tra ngày kết thúc của vòng thi hiện tại nhỏ hơn ngày bắt đầu của vòng thi sau
            var vongThiSau = await _context.VongThis
                .Where(vt => vt.MaCuocThi == maCuocThi && vt.ThuTu > thuTu)
                .OrderBy(vt => vt.ThuTu)
                .FirstOrDefaultAsync();

            if (vongThiSau != null && ngayKetThucParsed >= vongThiSau.NgayBatDau)
            {
                return BadRequest("Ngày kết thúc của vòng thi phải nhỏ hơn ngày bắt đầu của vòng thi tiếp theo.");
            }

            // Nếu không có lỗi, thêm mới vòng thi
            var vongThi = new VongThi
            {
                MaCuocThi = maCuocThi,
                TenVongThi = tenVongThi,
                ThuTu = thuTu,
                NgayBatDau = ngayBatDauParsed,
                NgayKetThuc = ngayKetThucParsed
            };

            _context.VongThis.Add(vongThi);
            await _context.SaveChangesAsync();
            return Ok("Thêm vòng thi thành công.");
        }


        // PUT: Cập nhật vòng thi
        [HttpPut]
        public async Task<IActionResult> Update(int id, int maCuocThi, string tenVongThi, int thuTu, string ngayBatDau, string ngayKetThuc)
        {
            var vongThi = await _context.VongThis
                .Include(vt => vt.CuocThi)
                .FirstOrDefaultAsync(vt => vt.MaVongThi == id);

            if (vongThi == null)
                return NotFound("Vòng thi không tồn tại.");

            // Kiểm tra nếu cuộc thi đã có vòng thi tiếp theo bắt đầu
            bool hasNextRoundStarted = await _context.VongThis
                .AnyAsync(vt => vt.MaCuocThi == maCuocThi && vt.ThuTu > vongThi.ThuTu && vt.NgayBatDau <= DateTime.Now);
            if (hasNextRoundStarted)
            {
                return BadRequest("Không thể cập nhật vòng thi vì vòng thi tiếp theo đã bắt đầu.");
            }

            // Kiểm tra nếu vòng thi hiện tại đã bắt đầu hoặc kết thúc
            if (vongThi.NgayBatDau <= DateTime.Now && vongThi.NgayKetThuc >= DateTime.Now)
            {
                return BadRequest("Không thể thay đổi thời gian hoặc thứ tự khi vòng thi đang diễn ra.");
            }
            else if (vongThi.NgayKetThuc < DateTime.Now)
            {
                return BadRequest("Không thể thay đổi vòng thi đã kết thúc.");
            }

            // Kiểm tra nếu có đề tài đã nộp bài cho vòng thi
            bool hasSubmissions = await _context.BaiNops.AnyAsync(bn => bn.MaBaiNop == id);
            if (hasSubmissions)
            {
                return BadRequest("Không thể thay đổi vòng thi vì đã có đề tài nộp bài.");
            }

            // Cập nhật thông tin vòng thi
            vongThi.MaCuocThi = maCuocThi;
            vongThi.TenVongThi = tenVongThi;
            vongThi.ThuTu = thuTu;
            vongThi.NgayBatDau = DateTime.Parse(ngayBatDau);
            vongThi.NgayKetThuc = DateTime.Parse(ngayKetThuc);

            _context.Entry(vongThi).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok("Cập nhật vòng thi thành công.");
        }


        // DELETE: Xóa vòng thi
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Kiểm tra nếu có đề tài tham gia vòng thi
            var vongThiCoDeTai = await _context.KetQuaVongThis.AnyAsync(kq => kq.MaVongThi == id);
            if (vongThiCoDeTai)
            {
                return BadRequest("Không thể xóa vòng thi vì có đề tài đang tham gia.");
            }

            var vongThi = await _context.VongThis.FindAsync(id);
            if (vongThi == null)
                return NotFound();

            _context.VongThis.Remove(vongThi);
            await _context.SaveChangesAsync();
            return Ok();
        }


        // GET: Lấy danh sách cuộc thi
        [HttpGet]
        public async Task<IActionResult> GetAllCuocThi()
        {
            var cuocThiList = await _context.CuocThis.ToListAsync();
            return Json(cuocThiList);
        }
    }
}
