using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DoAnChuyenNganh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BaiNopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BaiNopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang quản lý bài nộp
        public IActionResult BaiNop()
        {
            return View("~/Areas/Admin/Views/Home/BaiNop.cshtml");
        }

        // Lấy tất cả bài nộp
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var baiNopList = await _context.BaiNops
                .Include(bn => bn.DeTai)
                .Include(bn => bn.Nhom)
                .Select(bn => new
                {
                    bn.MaBaiNop,
                    DeTai = bn.DeTai.TenDeTai,
                    Nhom = bn.Nhom.TenNhom,
                    bn.NgayNop,
                    bn.NgayChinhSua,
                    bn.TrangThai,
                    bn.FileDinhKem
                })
                .ToListAsync();

            return Json(baiNopList);
        }

        // Lấy bài nộp theo Id
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var baiNop = await _context.BaiNops
                .Include(bn => bn.DeTai)
                .Include(bn => bn.Nhom)
                .FirstOrDefaultAsync(bn => bn.MaBaiNop == id);

            if (baiNop == null)
                return NotFound();

            return Json(new
            {
                baiNop.MaBaiNop,
                baiNop.MaDeTai,
                baiNop.MaNhom,
                baiNop.NgayNop,
                baiNop.NgayChinhSua,
                baiNop.TrangThai,
                baiNop.FileDinhKem
            });
        }

        // Thêm mới bài nộp
        [HttpPost]
        public async Task<IActionResult> Create(int maDeTai, int maNhom, List<IFormFile> files)
        {
            if (_context.BaiNops.Any(bn => bn.MaDeTai == maDeTai && bn.MaNhom == maNhom))
            {
                return BadRequest("Bài nộp cho đề tài này đã tồn tại.");
            }

            var baiNop = new BaiNop
            {
                MaDeTai = maDeTai,
                MaNhom = maNhom,
                NgayNop = DateTime.Now,
                TrangThai = "Đã nộp"
            };

            if (files != null && files.Count > 0)
            {
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files");
                Directory.CreateDirectory(uploadPath);

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        string filePath = Path.Combine(uploadPath, file.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        baiNop.FileDinhKem = file.FileName;
                    }
                }
            }

            _context.BaiNops.Add(baiNop);
            await _context.SaveChangesAsync();
            return Ok("Thêm bài nộp thành công!");
        }

        // Sửa bài nộp
        [HttpPost]
        public async Task<IActionResult> Update(int maBaiNop, List<IFormFile> files)
        {
            var baiNop = await _context.BaiNops.FindAsync(maBaiNop);
            if (baiNop == null)
            {
                return NotFound("Không tìm thấy bài nộp.");
            }

            baiNop.NgayChinhSua = DateTime.Now;
            baiNop.TrangThai = "Đã chỉnh sửa";

            if (files != null && files.Count > 0)
            {
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files");
                Directory.CreateDirectory(uploadPath);

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        string filePath = Path.Combine(uploadPath, file.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        baiNop.FileDinhKem = file.FileName;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Cập nhật bài nộp thành công!");
        }


        // Xóa bài nộp
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var baiNop = await _context.BaiNops.FindAsync(id);
            if (baiNop == null)
                return NotFound();

            // Kiểm tra nếu bài nộp đã được chấm điểm
            var ketQua = await _context.KetQuaVongThis.AnyAsync(kq => kq.MaDeTai == baiNop.MaDeTai);
            if (ketQua)
            {
                return BadRequest("Không thể xóa bài nộp vì đã được chấm điểm.");
            }

            _context.BaiNops.Remove(baiNop);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
