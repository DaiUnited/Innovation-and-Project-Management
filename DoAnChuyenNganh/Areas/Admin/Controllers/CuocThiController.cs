using DoAnChuyenNganh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace DoAnChuyenNganh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CuocThiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CuocThiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action để load trang CuocThi.cshtml
        public IActionResult CuocThi()
        {
            return View("~/Areas/Admin/Views/Home/CuocThi.cshtml");
        }

        // GET: Lấy tất cả các cuộc thi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cuocThiList = await _context.CuocThis.ToListAsync();
            return Json(cuocThiList);
        }

        // GET: Lấy cuộc thi theo Id
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var cuocThi = await _context.CuocThis.FindAsync(id);
            if (cuocThi == null)
                return NotFound();
            return Json(cuocThi);
        }

        // POST: Thêm mới cuộc thi
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            string tenCuocThi = Request.Form["TenCuocThi"];

            // Kiểm tra tên cuộc thi đã tồn tại
            bool isExisting = await _context.CuocThis.AnyAsync(ct => ct.TenCuocThi == tenCuocThi);
            if (isExisting)
                return BadRequest("Tên cuộc thi đã tồn tại.");

            var cuocThi = new CuocThi
            {
                TenCuocThi = tenCuocThi,
                MoTa = Request.Form["MoTa"],
                NgayBatDau = DateTime.Parse(Request.Form["NgayBatDau"]),
                NgayKetThuc = DateTime.Parse(Request.Form["NgayKetThuc"])
            };

            if (ModelState.IsValid)
            {
                _context.CuocThis.Add(cuocThi);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Dữ liệu không hợp lệ.");
        }



        // PUT: Cập nhật cuộc thi
        [HttpPut]
        public async Task<IActionResult> Update(int id)
        {
            var cuocThi = await _context.CuocThis.FindAsync(id);
            if (cuocThi == null)
            {
                return NotFound("Cuộc thi không tồn tại.");
            }

            // Lấy dữ liệu từ `FormCollection`
            cuocThi.TenCuocThi = Request.Form["TenCuocThi"];
            cuocThi.MoTa = Request.Form["MoTa"];
            cuocThi.NgayBatDau = DateTime.Parse(Request.Form["NgayBatDau"]);
            cuocThi.NgayKetThuc = DateTime.Parse(Request.Form["NgayKetThuc"]);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(cuocThi).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest($"Lỗi cập nhật: {ex.Message}");
                }
            }
            return BadRequest("Dữ liệu không hợp lệ.");
        }


        // DELETE: Xóa cuộc thi
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var vongThiDangDienRa = await _context.VongThis.AnyAsync(vt => vt.MaCuocThi == id && DateTime.Now >= vt.NgayBatDau && DateTime.Now <= vt.NgayKetThuc);
            if (vongThiDangDienRa)
                return BadRequest("Không thể xóa cuộc thi vì đang có vòng thi diễn ra.");

            var deTaiDangKy = await _context.DeTais.AnyAsync(dt => dt.MaCuocThi == id);
            if (deTaiDangKy)
                return BadRequest("Không thể xóa cuộc thi vì có đề tài tham gia.");

            var cuocThi = await _context.CuocThis.FindAsync(id);
            if (cuocThi == null)
                return NotFound();

            _context.CuocThis.Remove(cuocThi);
            await _context.SaveChangesAsync();
            return Ok();
        }


    }
}
