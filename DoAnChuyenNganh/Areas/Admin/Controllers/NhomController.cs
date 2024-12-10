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
    public class NhomController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NhomController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang chính
        public IActionResult Nhom()
        {
            return View("~/Areas/Admin/Views/Home/Nhom.cshtml");
        }

        // Lấy tất cả các nhóm
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var nhomList = await _context.Nhoms.ToListAsync();
            return Json(nhomList);
        }

        // Lấy nhóm theo Id
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var nhom = await _context.Nhoms.FindAsync(id);
            if (nhom == null)
                return NotFound();
            return Json(nhom);
        }

        // Thêm mới nhóm
        [HttpPost]
        public async Task<IActionResult> Create(string tenNhom)
        {
            // Kiểm tra tên nhóm đã tồn tại
            bool isExisting = await _context.Nhoms.AnyAsync(n => n.TenNhom == tenNhom);
            if (isExisting)
                return BadRequest("Tên nhóm đã tồn tại.");

            var nhom = new Nhom { TenNhom = tenNhom };
            _context.Nhoms.Add(nhom);
            await _context.SaveChangesAsync();
            return Ok();
        }


        // Cập nhật nhóm
        [HttpPut]
        public async Task<IActionResult> Update(int id, string tenNhom)
        {
            var nhom = await _context.Nhoms
                .Include(n => n.ThanhVienNhoms)
                .FirstOrDefaultAsync(n => n.MaNhom == id);

            if (nhom == null)
                return NotFound("Nhóm không tồn tại.");

            // Kiểm tra nếu nhóm hiện tại đang được phân công cho đề tài
            bool isAssignedToDeTai = await _context.DeTais.AnyAsync(dt => dt.MaNhom == id);
            if (isAssignedToDeTai)
            {
                return BadRequest("Nhóm này đã được phân công đề tài, không thể thay đổi tên.");
            }

            // Kiểm tra nếu nhóm đã có đề tài và đề tài đó đang trong quá trình thi
            var deTai = await _context.DeTais
                .Include(dt => dt.CuocThi)
                .FirstOrDefaultAsync(dt => dt.MaNhom == id);
            if (deTai != null)
            {
                bool isOngoing = await _context.VongThis
                    .AnyAsync(vt => vt.MaCuocThi == deTai.MaCuocThi && vt.NgayBatDau <= DateTime.Now && vt.NgayKetThuc >= DateTime.Now);
                if (isOngoing)
                {
                    return BadRequest("Nhóm không thể cập nhật khi đề tài của nhóm đang trong quá trình thi.");
                }
            }

            // Cập nhật tên nhóm
            nhom.TenNhom = tenNhom;
            _context.Entry(nhom).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Cập nhật nhóm thành công.");
        }


        // Xóa nhóm
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var nhomCoDeTai = await _context.DeTais.AnyAsync(dt => dt.MaNhom == id);
            if (nhomCoDeTai)
                return BadRequest("Không thể xóa nhóm vì có đề tài đang thuộc nhóm này.");

            var nhomCoThanhVien = await _context.ThanhVienNhoms.AnyAsync(tv => tv.MaNhom == id);
            if (nhomCoThanhVien)
                return BadRequest("Không thể xóa nhóm vì vẫn còn thành viên.");

            var nhom = await _context.Nhoms.FindAsync(id);
            if (nhom == null)
                return NotFound();

            _context.Nhoms.Remove(nhom);
            await _context.SaveChangesAsync();
            return Ok();
        }



        // Lấy chi tiết nhóm
        [HttpGet]
        public async Task<IActionResult> GetByNhom(int id)
        {
            var thanhVienList = await _context.ThanhVienNhoms
                .Include(tv => tv.SinhVien)
                .Where(tv => tv.MaNhom == id)
                .ToListAsync();
            return Json(thanhVienList);
        }

        // Lấy danh sách thành viên theo nhóm
        [HttpGet]
        [Route("/Admin/Nhom/Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var nhom = await _context.Nhoms
                .Include(n => n.ThanhVienNhoms)
                .ThenInclude(tv => tv.SinhVien)
                .FirstOrDefaultAsync(n => n.MaNhom == id);

            if (nhom == null)
                return NotFound();

            return View("~/Areas/Admin/Views/Home/ThanhVienNhom.cshtml", nhom);
        }

        // Kiểm tra sinh viên đã thuộc nhóm chưa
        [HttpGet]
        public async Task<IActionResult> GetThanhVienById(int maSV)
        {
            var thanhVien = await _context.ThanhVienNhoms
                .Include(tv => tv.SinhVien)
                .FirstOrDefaultAsync(tv => tv.MaSV == maSV);
            return Json(thanhVien);
        }

        // Thêm hoặc cập nhật thành viên vào nhóm
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate(int maNhom, int maSV, bool truongNhom)
        {
            // Kiểm tra nếu sinh viên đã thuộc nhóm khác nhưng không phải nhóm hiện tại
            var thanhVienHienTai = await _context.ThanhVienNhoms.FirstOrDefaultAsync(tv => tv.MaSV == maSV);
            if (thanhVienHienTai != null && thanhVienHienTai.MaNhom != maNhom)
                return BadRequest("Sinh viên này đã thuộc một nhóm khác.");

            // Kiểm tra số lượng thành viên hiện tại của nhóm
            int soThanhVien = await _context.ThanhVienNhoms.CountAsync(tv => tv.MaNhom == maNhom);
            if (soThanhVien >= 5 && thanhVienHienTai == null)
                return BadRequest("Nhóm đã đạt tối đa số thành viên (5).");

            // Kiểm tra nếu nhóm đã có trưởng nhóm và bạn đang cố gắng gán thêm một trưởng nhóm mới
            if (truongNhom)
            {
                var truongNhomHienTai = await _context.ThanhVienNhoms.FirstOrDefaultAsync(tv => tv.MaNhom == maNhom && tv.TruongNhom);
                if (truongNhomHienTai != null && truongNhomHienTai.MaSV != maSV)
                {
                    truongNhomHienTai.TruongNhom = false;
                    _context.Entry(truongNhomHienTai).State = EntityState.Modified;
                }
            }

            // Thêm hoặc cập nhật thành viên
            if (thanhVienHienTai == null)
            {
                // Thêm mới nếu sinh viên chưa thuộc nhóm
                var thanhVienMoi = new ThanhVienNhom { MaNhom = maNhom, MaSV = maSV, TruongNhom = truongNhom };
                _context.ThanhVienNhoms.Add(thanhVienMoi);
            }
            else
            {
                // Cập nhật nếu sinh viên đã thuộc nhóm hiện tại
                thanhVienHienTai.TruongNhom = truongNhom;
                _context.Entry(thanhVienHienTai).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }


        // Xóa thành viên khỏi nhóm
        [HttpDelete]
        public async Task<IActionResult> DeleteThanhVien(int maSV, int maNhom)
        {
            // Kiểm tra nếu nhóm của thành viên đang có đề tài
            var nhomCoDeTai = await _context.DeTais.AnyAsync(dt => dt.MaNhom == maNhom);
            if (nhomCoDeTai)
            {
                return BadRequest("Không thể xóa thành viên vì nhóm đang có đề tài.");
            }

            var thanhVien = await _context.ThanhVienNhoms.FirstOrDefaultAsync(tv => tv.MaSV == maSV && tv.MaNhom == maNhom);
            if (thanhVien == null)
                return NotFound("Thành viên không tồn tại trong nhóm.");

            _context.ThanhVienNhoms.Remove(thanhVien);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
