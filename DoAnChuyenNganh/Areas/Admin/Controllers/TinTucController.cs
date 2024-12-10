using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

namespace DoAnChuyenNganh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TinTucController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TinTucController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang chính
        public IActionResult TinTuc()
        {
            return View("~/Areas/Admin/Views/Home/TinTuc.cshtml");
        }

        // Lấy danh sách tin tức
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tinTucs = await _context.TinTucs
                .Include(tt => tt.LoaiTinTuc)
                .Select(tt => new
                {
                    tt.MaTinTuc,
                    tt.TieuDe,
                    tt.MoTa,
                    tt.AnhBia,
                    tt.FileNoiDung,
                    tt.NgayTao,
                    LoaiTinTuc = tt.LoaiTinTuc.TenLoai
                })
                .ToListAsync();

            return Json(tinTucs);
        }

        // Lấy tin tức theo ID
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var tinTuc = await _context.TinTucs
                .Include(tt => tt.LoaiTinTuc)
                .FirstOrDefaultAsync(tt => tt.MaTinTuc == id);

            if (tinTuc == null) return NotFound();

            return Json(new
            {
                tinTuc.MaTinTuc,
                tinTuc.TieuDe,
                tinTuc.MoTa,
                tinTuc.AnhBia,
                tinTuc.FileNoiDung,
                tinTuc.NgayTao,
                tinTuc.MaLoaiTinTuc
            });
        }

        // Thêm mới tin tức
        [HttpPost]
        public async Task<IActionResult> Create(IFormFile anhBia, IFormFile fileNoiDung, string tieuDe, string moTa, int maLoaiTinTuc)
        {
            if (!_context.LoaiTinTucs.Any(lt => lt.MaLoaiTinTuc == maLoaiTinTuc))
                return BadRequest("Loại tin tức không hợp lệ.");

            string anhBiaPath = "";
            string filePath = "";

            // Xử lý ảnh bìa
            if (anhBia != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = $"{Guid.NewGuid()}_{anhBia.FileName}";
                anhBiaPath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(anhBiaPath, FileMode.Create))
                {
                    await anhBia.CopyToAsync(stream);
                }
                anhBiaPath = $"/images/{uniqueFileName}";
            }

            // Xử lý file nội dung
            if (fileNoiDung != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = $"{Guid.NewGuid()}_{fileNoiDung.FileName}";
                filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileNoiDung.CopyToAsync(stream);
                }
                filePath = $"/files/{uniqueFileName}";
            }

            var tinTuc = new TinTuc
            {
                TieuDe = tieuDe,
                MoTa = moTa,
                NgayTao = DateTime.Now,
                AnhBia = anhBiaPath,
                FileNoiDung = filePath,
                MaLoaiTinTuc = maLoaiTinTuc
            };

            _context.TinTucs.Add(tinTuc);
            await _context.SaveChangesAsync();
            return Ok("Thêm mới tin tức thành công.");
        }

        // Cập nhật tin tức
        [HttpPut]
        public async Task<IActionResult> Update(int id, IFormFile anhBia, IFormFile fileNoiDung, string tieuDe, int maLoaiTinTuc)
        {
            var tinTuc = await _context.TinTucs.FindAsync(id);
            if (tinTuc == null) return NotFound("Không tìm thấy tin tức.");

            if (!_context.LoaiTinTucs.Any(lt => lt.MaLoaiTinTuc == maLoaiTinTuc))
                return BadRequest("Loại tin tức không hợp lệ.");

            tinTuc.TieuDe = tieuDe;
            tinTuc.MaLoaiTinTuc = maLoaiTinTuc;

            string wwwRootPath = Directory.GetCurrentDirectory();

            // Cập nhật ảnh bìa nếu có
            if (anhBia != null)
            {
                string uploadsFolder = Path.Combine(wwwRootPath, "wwwroot/images");
                Directory.CreateDirectory(uploadsFolder);

                // Xóa ảnh cũ
                if (!string.IsNullOrEmpty(tinTuc.AnhBia))
                {
                    string oldImagePath = Path.Combine(wwwRootPath, tinTuc.AnhBia.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                }

                // Lưu ảnh mới
                string uniqueFileName = $"{Guid.NewGuid()}_{anhBia.FileName}";
                string imagePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await anhBia.CopyToAsync(stream);
                }

                tinTuc.AnhBia = $"/images/{uniqueFileName}";
            }

            // Cập nhật file nội dung nếu có
            if (fileNoiDung != null)
            {
                string uploadsFolder = Path.Combine(wwwRootPath, "wwwroot/files");
                Directory.CreateDirectory(uploadsFolder);

                // Xóa file cũ
                if (!string.IsNullOrEmpty(tinTuc.FileNoiDung))
                {
                    string oldFilePath = Path.Combine(wwwRootPath, tinTuc.FileNoiDung.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                }

                // Lưu file mới
                string uniqueFileName = $"{Guid.NewGuid()}_{fileNoiDung.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileNoiDung.CopyToAsync(stream);
                }

                tinTuc.FileNoiDung = $"/files/{uniqueFileName}";
            }

            _context.Entry(tinTuc).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok("Cập nhật thành công.");
        }

        // Xóa tin tức
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var tinTuc = await _context.TinTucs.FindAsync(id);
            if (tinTuc == null) return NotFound();

            if (!string.IsNullOrEmpty(tinTuc.AnhBia))
            {
                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", tinTuc.AnhBia.TrimStart('/'));
                if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
            }

            if (!string.IsNullOrEmpty(tinTuc.FileNoiDung))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", tinTuc.FileNoiDung.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _context.TinTucs.Remove(tinTuc);
            await _context.SaveChangesAsync();

            return Ok("Xóa tin tức thành công.");
        }
    }
}
