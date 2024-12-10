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
    public class VaiTroController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VaiTroController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action để load trang VaiTro.cshtml
        public IActionResult VaiTro()
        {
            return View("~/Areas/Admin/Views/Home/VaiTro.cshtml");
        }

        // GET: Lấy tất cả các vai trò
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vaiTroList = await _context.VaiTros.ToListAsync();
            return Json(vaiTroList);
        }

        // GET: Lấy vai trò theo Id
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var vaiTro = await _context.VaiTros.FindAsync(id);
            if (vaiTro == null)
                return NotFound();
            return Json(vaiTro);
        }

        // POST: Thêm mới vai trò
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            var vaiTro = new VaiTro
            {
                TenVaiTro = Request.Form["TenVaiTro"]
            };

            if (ModelState.IsValid)
            {
                _context.VaiTros.Add(vaiTro);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Dữ liệu không hợp lệ.");
        }

        // PUT: Cập nhật vai trò
        [HttpPut]
        public async Task<IActionResult> Update(int id)
        {
            var vaiTro = await _context.VaiTros.FindAsync(id);
            if (vaiTro == null)
                return NotFound("Vai trò không tồn tại.");

            vaiTro.TenVaiTro = Request.Form["TenVaiTro"];

            if (ModelState.IsValid)
            {
                _context.Entry(vaiTro).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Dữ liệu không hợp lệ.");
        }

        // DELETE: Xóa vai trò
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var vaiTro = await _context.VaiTros.FindAsync(id);
            if (vaiTro == null)
                return NotFound();

            bool isLinkedToNguoiDung = await _context.NguoiDungs.AnyAsync(nd => nd.MaVaiTro == id);
            if (isLinkedToNguoiDung)
                return BadRequest("Không thể xóa vai trò vì vai trò đang được sử dụng bởi người dùng.");

            _context.VaiTros.Remove(vaiTro);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
