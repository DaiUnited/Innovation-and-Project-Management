using DoAnChuyenNganh.Models;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnChuyenNganh.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tinTucs = await _context.TinTucs
                    .Include(t => t.LoaiTinTuc) // Include loại tin tức
                    .ToListAsync();
            ViewBag.Categories = await _context.LoaiTinTucs.ToListAsync();
            return View(tinTucs); // Truyền danh sách tin tức sang View
        }

        public IActionResult TheLe()
        {
            return View("TheLe");
        }

        public IActionResult GioiThieu()
        {
            return View("GioiThieu");
        }

        public IActionResult TinTucTheoLoai(int loaiTinTucId)
        {
            var tinTucs = _context.TinTucs
                .Where(t => t.MaLoaiTinTuc == loaiTinTucId)
                .ToList();

            return PartialView("_TinTucList", tinTucs);
        }

        public async Task<IActionResult> DanhSachTinTuc(string search)
        {
            var query = _context.TinTucs.Include(t => t.LoaiTinTuc).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.TieuDe.Contains(search) || t.MoTa.Contains(search));
            }

            var tinTucs = await query.ToListAsync();
            return View(tinTucs);
        }

        public async Task<IActionResult> SearchTinTuc(string keyword)
        {
            var tinTucs = await _context.TinTucs
                .Where(t => t.TieuDe.Contains(keyword) || t.MoTa.Contains(keyword))
                .Include(t => t.LoaiTinTuc)
                .ToListAsync();

            ViewBag.LinhVucList = await _context.LinhVucs.ToListAsync();
            ViewBag.DeTaiList = await _context.DeTais.ToListAsync();
            ViewBag.CuocThiList = await _context.CuocThis.ToListAsync();

            return View("DanhSachTinTuc", tinTucs);
        }

        public async Task<IActionResult> ChiTietTinTuc(int id)
        {
            var tinTuc = await _context.TinTucs
                .FirstOrDefaultAsync(t => t.MaTinTuc == id);

            if (tinTuc == null)
            {
                return NotFound("Tin tức không tồn tại.");
            }

            // Đọc nội dung từ file Word
            if (!string.IsNullOrEmpty(tinTuc.FileNoiDung))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", tinTuc.FileNoiDung.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    using var wordDoc = WordprocessingDocument.Open(filePath, false);
                    var body = wordDoc.MainDocumentPart.Document.Body;

                    // Trích xuất nội dung văn bản
                    tinTuc.NoiDung = body.InnerText;

                    // Trích xuất hình ảnh
                    var imageParts = wordDoc.MainDocumentPart.ImageParts;
                    foreach (var imagePart in imageParts)
                    {
                        var imageFileName = Guid.NewGuid().ToString() + ".jpg";
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", imageFileName);

                        // Ghi hình ảnh vào thư mục wwwroot/images
                        using (var stream = imagePart.GetStream())
                        using (var fileStream = new FileStream(imagePath, FileMode.Create))
                        {
                            await stream.CopyToAsync(fileStream);
                        }

                        // Thêm đường dẫn ảnh vào nội dung (tùy chỉnh HTML)
                        tinTuc.NoiDung += $"<img src='/images/{imageFileName}' alt='Hình ảnh' class='img-fluid' />";
                    }
                }
            }

            ViewBag.OtherTinTuc = await _context.TinTucs
        .Where(t => t.MaTinTuc != id)
        .OrderByDescending(t => t.NgayTao)
        .Take(5)
        .ToListAsync();

            return View(tinTuc);
        }
    }
}
