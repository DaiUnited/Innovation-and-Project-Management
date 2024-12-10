using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
    

namespace DoAnChuyenNganh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GiamKhaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GiamKhaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang GiamKhao
        public IActionResult GiamKhao()
        {
            return View("~/Areas/Admin/Views/Home/GiamKhao.cshtml");
        }

        // Lấy tất cả giám khảo
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var giamKhaoList = await _context.GiamKhaos
                .Include(gk => gk.NguoiDung)
                .Select(gk => new
                {
                    gk.MaGiamKhao,
                    gk.HoTen,
                    gk.Email,
                    gk.SoDienThoai,
                    NguoiDung = gk.NguoiDung != null ? gk.NguoiDung.TenDangNhap : "Không có"
                }).ToListAsync();
            return Json(giamKhaoList);
        }

        // Lấy giám khảo theo Id
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var giamKhao = await _context.GiamKhaos.Include(gk => gk.NguoiDung).FirstOrDefaultAsync(gk => gk.MaGiamKhao == id);
            if (giamKhao == null)
                return NotFound();
            return Json(giamKhao);
        }

        // Thêm mới giám khảo
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            string email = Request.Form["Email"];
            string soDienThoai = Request.Form["SoDienThoai"];

            if (await _context.GiamKhaos.AnyAsync(gk => gk.Email == email))
                return BadRequest("Email này đã được sử dụng.");

            if (await _context.GiamKhaos.AnyAsync(gk => gk.SoDienThoai == soDienThoai))
                return BadRequest("Số điện thoại này đã được sử dụng.");
            var giamKhao = new GiamKhao
            {
                HoTen = Request.Form["HoTen"],
                Email = Request.Form["Email"],
                SoDienThoai = Request.Form["SoDienThoai"],
                MaNguoiDung = int.Parse(Request.Form["MaNguoiDung"])
            };

            if (ModelState.IsValid)
            {
                _context.GiamKhaos.Add(giamKhao);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Dữ liệu không hợp lệ.");
        }

        // Cập nhật giám khảo
        [HttpPut]
        public async Task<IActionResult> Update(int id)
        {
            var giamKhao = await _context.GiamKhaos.FindAsync(id);
            if (giamKhao == null)
                return NotFound("Giám khảo không tồn tại.");

            string email = Request.Form["Email"];
            string soDienThoai = Request.Form["SoDienThoai"];

            if (await _context.GiamKhaos.AnyAsync(gk => gk.Email == email && gk.MaGiamKhao != id))
                return BadRequest("Email này đã được sử dụng.");

            if (await _context.GiamKhaos.AnyAsync(gk => gk.SoDienThoai == soDienThoai && gk.MaGiamKhao != id))
                return BadRequest("Số điện thoại này đã được sử dụng.");

            giamKhao.HoTen = Request.Form["HoTen"];
            giamKhao.Email = Request.Form["Email"];
            giamKhao.SoDienThoai = Request.Form["SoDienThoai"];
            giamKhao.MaNguoiDung = int.Parse(Request.Form["MaNguoiDung"]);

            if (ModelState.IsValid)
            {
                _context.Entry(giamKhao).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Dữ liệu không hợp lệ.");
        }

        // Xóa giám khảo
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var giamKhao = await _context.GiamKhaos.FindAsync(id);
            if (giamKhao == null)
                return NotFound();

            bool isLinkedToKetQua = await _context.KetQuaVongThis.AnyAsync(kq => kq.MaGiamKhao == id);
            if (isLinkedToKetQua)
                return BadRequest("Không thể xóa giám khảo vì giám khảo đang được phân công chấm điểm.");

            _context.GiamKhaos.Remove(giamKhao);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
