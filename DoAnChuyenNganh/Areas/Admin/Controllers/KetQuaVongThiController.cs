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
    public class KetQuaVongThiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KetQuaVongThiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class MoveToNextRoundWithAssignmentsRequest
        {
            public int CurrentRoundThuTu { get; set; }
            public int NextRoundThuTu { get; set; }
            public int TopLimit { get; set; }
            public List<JudgeAssignment> Assignments { get; set; }
        }

        public class JudgeAssignment
        {
            public int MaDeTai { get; set; }
            public int MaGiamKhao { get; set; }
        }

        private async Task<int?> GetMaVongThiAsync(int thuTu, int maCuocThi)
        {
            return await _context.VongThis
                .Where(vt => vt.ThuTu == thuTu && vt.MaCuocThi == maCuocThi)
                .Select(vt => vt.MaVongThi)
                .FirstOrDefaultAsync();
        }

        // Trang quản lý kết quả vòng thi

        public IActionResult PhanCongChamDiem()
        {
            return View("~/Areas/Admin/Views/Home/PhanCongChamDiem.cshtml");
        }

        public IActionResult KetQuaVongThiSoKhao()
        {
            return View("~/Areas/Admin/Views/Home/KetQuaVongThiSoKhao.cshtml");
        }

        public IActionResult KetQuaVongThiBanKet()
        {
            return View("~/Areas/Admin/Views/Home/KetQuaVongThiBanKet.cshtml");
        }

        public IActionResult KetQuaVongThiChungKet()
        {
            return View("~/Areas/Admin/Views/Home/KetQuaVongThiChungKet.cshtml");
        }

        // Lấy danh sách kết quả theo vòng thi
        // Lấy tất cả kết quả chấm điểm
        [HttpGet]
        public async Task<IActionResult> GetAll(int? maCuocThi = null)
        {
            var ketQuaList = await _context.KetQuaVongThis
                .Include(kq => kq.DeTai)
                .Include(kq => kq.VongThi)
                .Include(kq => kq.GiamKhao)
                .Where(kq => maCuocThi == null || kq.DeTai.MaCuocThi == maCuocThi)
                .Select(kq => new
                {
                    kq.MaKetQua,
                    kq.MaGiamKhao,
                    GiamKhao = kq.GiamKhao.HoTen,
                    VongThi = kq.VongThi.TenVongThi,
                    DeTai = kq.DeTai.TenDeTai,
                    kq.DiemSangTao,
                    kq.DiemKhaThi,
                    kq.DiemTacDongXaHoi,
                    kq.DiemTiemNangThiTruong,
                    TongDiem = kq.DiemSangTao + kq.DiemKhaThi + kq.DiemTacDongXaHoi + kq.DiemTiemNangThiTruong,
                    kq.TinhTrang,
                    kq.NhanXet,
                    kq.NgayNhanXet,
                })
                .OrderByDescending(kq => kq.TongDiem)
                .ToListAsync();

            return Json(ketQuaList);
        }



        [HttpGet]
        public async Task<IActionResult> GetAllSoKhao(int? maCuocThi = null)
        {
            if (!maCuocThi.HasValue)
                return BadRequest("Mã cuộc thi không được để trống.");

            var maVongThi = await GetMaVongThiAsync(1, maCuocThi.Value); // ThuTu = 1 cho sơ khảo
            if (!maVongThi.HasValue)
                return NotFound("Không tìm thấy vòng thi sơ khảo.");

            var data = await GetKetQuaByVongThi(maVongThi.Value);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBanKet(int? maCuocThi = null)
        {
            if (!maCuocThi.HasValue)
                return BadRequest("Mã cuộc thi không được để trống.");

            var maVongThi = await GetMaVongThiAsync(2, maCuocThi.Value); // ThuTu = 2 cho bán kết
            if (!maVongThi.HasValue)
                return NotFound("Không tìm thấy vòng thi bán kết.");

            var data = await GetKetQuaByVongThi(maVongThi.Value);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChungKet(int? maCuocThi = null)
        {
            if (!maCuocThi.HasValue)
                return BadRequest("Mã cuộc thi không được để trống.");

            var maVongThi = await GetMaVongThiAsync(3, maCuocThi.Value); // ThuTu = 3 cho chung kết
            if (!maVongThi.HasValue)
                return NotFound("Không tìm thấy vòng thi chung kết.");

            var data = await GetKetQuaByVongThi(maVongThi.Value);
            return Json(data);
        }


        private async Task<object> GetKetQuaByVongThi(int maVongThi, int? maCuocThi = null)
        {
            var results = await _context.KetQuaVongThis
                .Include(kq => kq.DeTai)
                .Include(kq => kq.VongThi)
                .Include(kq => kq.GiamKhao)
                .Where(kq => kq.MaVongThi == maVongThi && (maCuocThi == null || kq.DeTai.MaCuocThi == maCuocThi)) // Lọc theo maCuocThi nếu có
                .Select(kq => new
                {
                    kq.MaKetQua,
                    kq.MaGiamKhao,
                    GiamKhao = kq.GiamKhao.HoTen,
                    VongThi = kq.VongThi.TenVongThi,
                    DeTai = kq.DeTai.TenDeTai,
                    kq.DiemSangTao,
                    kq.DiemKhaThi,
                    kq.DiemTacDongXaHoi,
                    kq.DiemTiemNangThiTruong,
                    TongDiem = kq.DiemSangTao + kq.DiemKhaThi + kq.DiemTacDongXaHoi + kq.DiemTiemNangThiTruong,
                    kq.TinhTrang,
                    kq.NhanXet,
                    kq.NgayNhanXet,
                    kq.XepHang
                })
                .OrderByDescending(kq => kq.TongDiem)
                .ToListAsync();

            return results;
        }


        // Cập nhật xếp hạng cho vòng thi
        [HttpPost]
        public async Task<IActionResult> UpdateRanking(int thuTu, int maCuocThi)
        {
            var maVongThi = await GetMaVongThiAsync(thuTu, maCuocThi);
            if (!maVongThi.HasValue)
                return BadRequest("Không tìm thấy vòng thi tương ứng với thứ tự và mã cuộc thi.");

            var ketQuaList = await _context.KetQuaVongThis
                .Include(kq => kq.DeTai)
                .Where(kq => kq.MaVongThi == maVongThi.Value && kq.DeTai.MaCuocThi == maCuocThi)
                .OrderByDescending(kq => kq.DiemSangTao + kq.DiemKhaThi + kq.DiemTacDongXaHoi + kq.DiemTiemNangThiTruong)
                .ThenByDescending(kq => kq.DiemSangTao) // Ưu tiên sáng tạo nếu bằng điểm
                .ThenByDescending(kq => kq.DiemKhaThi)  // Ưu tiên khả thi nếu vẫn bằng
                .ToListAsync();

            if (!ketQuaList.Any())
                return BadRequest("Không có dữ liệu để xếp hạng.");

            int rank = 1;
            for (int i = 0; i < ketQuaList.Count; i++)
            {
                // Nếu không phải đề tài đầu tiên và điểm tổng khác điểm của đề tài trước, tăng hạng
                if (i > 0 &&
                    (ketQuaList[i].DiemSangTao + ketQuaList[i].DiemKhaThi + ketQuaList[i].DiemTacDongXaHoi + ketQuaList[i].DiemTiemNangThiTruong) !=
                    (ketQuaList[i - 1].DiemSangTao + ketQuaList[i - 1].DiemKhaThi + ketQuaList[i - 1].DiemTacDongXaHoi + ketQuaList[i - 1].DiemTiemNangThiTruong))
                {
                    rank = i + 1;
                }

                ketQuaList[i].XepHang = rank;

                // Cập nhật trạng thái bị loại nếu vượt quá giới hạn top
                if (thuTu == 1 && rank > 10)
                    ketQuaList[i].TinhTrang = "Đề tài của nhóm đã bị loại";
                else if (thuTu == 2 && rank > 5)
                    ketQuaList[i].TinhTrang = "Đề tài của nhóm đã bị loại";
            }

            await _context.SaveChangesAsync();
            return Ok("Xếp hạng đã được cập nhật.");
        }



        [HttpGet]
        public async Task<IActionResult> GetTopDeTai(int currentRoundThuTu, int topLimit, int? maCuocThi)
        {
            // Lấy mã vòng thi dựa trên thứ tự vòng thi và mã cuộc thi
            var maVongThi = await _context.VongThis
                .Where(vt => vt.ThuTu == currentRoundThuTu && vt.MaCuocThi == maCuocThi)
                .Select(vt => vt.MaVongThi)
                .FirstOrDefaultAsync();

            if (maVongThi == 0)
                return NotFound("Không tìm thấy vòng thi tương ứng.");

            // Lấy các đề tài có xếp hạng <= topLimit ở vòng hiện tại và thuộc cuộc thi
            var topDeTai = await _context.KetQuaVongThis
                .Where(kq => kq.MaVongThi == maVongThi
                          && kq.XepHang > 0
                          && kq.XepHang <= topLimit
                          && (maCuocThi == null || kq.DeTai.MaCuocThi == maCuocThi))
                .Select(kq => new
                {
                    kq.MaDeTai,
                    TenDeTai = kq.DeTai.TenDeTai
                })
                .ToListAsync();

            if (!topDeTai.Any())
                return NotFound("Không có đề tài nằm trong top.");

            return Json(topDeTai);
        }

        // Kiểm tra nếu vòng thi tiếp theo chưa bắt đầu hoặc đã kết thúc
        //var nextRound = await _context.VongThis.FindAsync(request.NextRoundId);
        //if (nextRound == null)
        //{
        //    return BadRequest("Vòng thi tiếp theo không tồn tại.");
        //}

        //if (DateTime.Now < nextRound.NgayBatDau)
        //{
        //    return BadRequest("Vòng thi tiếp theo chưa bắt đầu. Không thể chuyển đề tài.");
        //}

        //if (DateTime.Now > nextRound.NgayKetThuc)
        //{
        //    return BadRequest("Vòng thi tiếp theo đã kết thúc. Không thể chuyển đề tài.");
        //}

        [HttpPost]
        public async Task<IActionResult> MoveToNextRoundWithAssignments([FromBody] MoveToNextRoundWithAssignmentsRequest request, int maCuocThi)
        {
            if (request == null || !request.Assignments.Any())
                return BadRequest("Không có dữ liệu hợp lệ.");

            var currentMaVongThi = await GetMaVongThiAsync(request.CurrentRoundThuTu, maCuocThi);
            var nextMaVongThi = await GetMaVongThiAsync(request.NextRoundThuTu, maCuocThi);

            if (!currentMaVongThi.HasValue || !nextMaVongThi.HasValue)
                return BadRequest("Không tìm thấy vòng thi tương ứng.");

            // Lấy các đề tài nằm trong top cần chuyển thuộc cuộc thi chỉ định
            var topDeTai = await _context.KetQuaVongThis
                .Where(kq => kq.MaVongThi == currentMaVongThi.Value && kq.XepHang <= request.TopLimit)
                .Select(kq => kq.MaDeTai)
                .Distinct()
                .ToListAsync();

            if (!topDeTai.Any())
                return BadRequest("Không có đề tài phù hợp để chuyển sang vòng tiếp theo.");

            // Tập hợp để kiểm tra trùng lặp giám khảo
            var assignedJudges = new HashSet<int>();

            foreach (var assignment in request.Assignments)
            {
                if (!topDeTai.Contains(assignment.MaDeTai))
                    return BadRequest($"Đề tài ID {assignment.MaDeTai} không nằm trong danh sách top được chuyển.");

                // Kiểm tra nếu giám khảo đã được phân công trong vòng tiếp theo
                if (await _context.KetQuaVongThis.AnyAsync(kq =>
                    kq.MaGiamKhao == assignment.MaGiamKhao && kq.MaVongThi == nextMaVongThi.Value))
                {
                    return BadRequest($"Giám khảo ID {assignment.MaGiamKhao} đã được phân công trong vòng thi tiếp theo.");
                }

                // Kiểm tra trùng lặp giám khảo chấm nhiều đề tài
                if (assignedJudges.Contains(assignment.MaGiamKhao))
                {
                    return BadRequest($"Giám khảo ID {assignment.MaGiamKhao} đã được gán cho một đề tài khác trong cùng lần phân công.");
                }

                // Thêm giám khảo vào danh sách đã được gán
                assignedJudges.Add(assignment.MaGiamKhao);

                // Tạo mới kết quả cho vòng tiếp theo
                var newKetQua = new KetQuaVongThi
                {
                    MaDeTai = assignment.MaDeTai,
                    MaVongThi = nextMaVongThi.Value,
                    MaGiamKhao = assignment.MaGiamKhao,
                    DiemSangTao = 0,
                    DiemKhaThi = 0,
                    DiemTacDongXaHoi = 0,
                    DiemTiemNangThiTruong = 0,
                    TinhTrang = "Chưa chấm",
                    NhanXet = "Chưa cập nhật",
                    NgayNhanXet = DateTime.Now,
                    XepHang = 0
                };

                _context.KetQuaVongThis.Add(newKetQua);
            }

            // Lưu thay đổi
            await _context.SaveChangesAsync();
            return Ok("Phân công giám khảo và chuyển đề tài sang vòng tiếp theo thành công.");
        }




        // Lấy chi tiết kết quả theo ID
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var ketQua = await _context.KetQuaVongThis
                .Include(kq => kq.DeTai)
                .Include(kq => kq.VongThi)
                .Include(kq => kq.GiamKhao)
                .FirstOrDefaultAsync(kq => kq.MaKetQua == id);

            if (ketQua == null)
                return NotFound("Không tìm thấy kết quả.");

            return Json(new
            {
                ketQua.MaKetQua,
                ketQua.MaGiamKhao,
                ketQua.MaVongThi,
                ketQua.MaDeTai,
                ketQua.DiemSangTao,
                ketQua.DiemKhaThi,
                ketQua.DiemTacDongXaHoi,
                ketQua.DiemTiemNangThiTruong,
                ketQua.TongDiem,
                ketQua.TinhTrang,
                ketQua.NhanXet,
                ketQua.NgayNhanXet
            });
        }

        // Xóa kết quả
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var ketQua = await _context.KetQuaVongThis.FindAsync(id);
            if (ketQua == null)
                return NotFound("Không tìm thấy kết quả.");

            _context.KetQuaVongThis.Remove(ketQua);
            await _context.SaveChangesAsync();
            return Ok("Xóa thành công.");
        }

        // Thêm mới kết quả chấm điểm
        [HttpPost]
        public async Task<IActionResult> Create(int maGiamKhao, int maVongThi, int maDeTai)
        {
            // Kiểm tra nếu đề tài đã bị loại
            var deTai = await _context.DeTais.FindAsync(maDeTai);
            if (deTai != null && _context.KetQuaVongThis.Any(kq => kq.MaDeTai == maDeTai && kq.TinhTrang == "Đề tài của nhóm đã bị loại"))
            {
                return BadRequest("Đề tài đã bị loại, không thể phân công giám khảo cho vòng thi tiếp theo.");
            }

            // Kiểm tra xem giám khảo đã chấm đề tài nào trong vòng thi này chưa
            bool isGiamKhaoAssigned = await _context.KetQuaVongThis
                .AnyAsync(kq => kq.MaGiamKhao == maGiamKhao && kq.MaVongThi == maVongThi);
            if (isGiamKhaoAssigned)
            {
                return BadRequest("Giám khảo đã chấm một đề tài khác trong vòng thi này.");
            }

            // Kiểm tra xem đề tài đã được chấm bởi giám khảo nào chưa
            bool isDeTaiAssigned = await _context.KetQuaVongThis
                .AnyAsync(kq => kq.MaDeTai == maDeTai && kq.MaVongThi == maVongThi);
            if (isDeTaiAssigned)
            {
                return BadRequest("Đề tài đã được chấm bởi một giám khảo khác trong vòng thi này.");
            }

            //var vongThi = await _context.VongThis.FindAsync(maVongThi);
            //if (vongThi == null || DateTime.Now < vongThi.NgayBatDau || DateTime.Now > vongThi.NgayKetThuc)
            //{
            //    return BadRequest("Vòng thi hiện tại chưa bắt đầu hoặc đã kết thúc, không thể phân công giám khảo.");
            //}

            // Tạo mới kết quả chấm điểm
            var ketQua = new KetQuaVongThi
            {
                MaGiamKhao = maGiamKhao,
                MaVongThi = maVongThi,
                MaDeTai = maDeTai,
                DiemSangTao = 0,
                DiemKhaThi = 0,
                DiemTacDongXaHoi = 0,
                DiemTiemNangThiTruong = 0,
                TinhTrang = "Chưa cập nhật",
                NhanXet = "Chưa cập nhật",
                NgayNhanXet = DateTime.Now,
                XepHang = 0
            };

            _context.KetQuaVongThis.Add(ketQua);
            await _context.SaveChangesAsync();
            return Ok("Thêm kết quả thành công.");
        }

        [HttpPut]
        public async Task<IActionResult> Update(int id, double? diemSangTao, double? diemKhaThi, double? diemTacDongXaHoi, double? diemTiemNangThiTruong, string nhanXet)
        {
            var ketQua = await _context.KetQuaVongThis
                .Include(kq => kq.DeTai)
                .Include(kq => kq.VongThi)
                .FirstOrDefaultAsync(kq => kq.MaKetQua == id);

            if (ketQua == null)
                return NotFound("Không tìm thấy kết quả chấm điểm.");
            // Kiểm tra nếu đề tài đã bị loại
            if (ketQua.TinhTrang == "Đề tài của nhóm đã bị loại")
                return BadRequest("Không thể cập nhật kết quả cho đề tài đã bị loại.");

            // Kiểm tra nếu vòng thi đã kết thúc
            //var vongThi = ketQua.VongThi;
            //if (vongThi == null || DateTime.Now < vongThi.NgayBatDau || DateTime.Now > vongThi.NgayKetThuc)
            //    return BadRequest("Vòng thi hiện tại chưa diễn ra hoặc đã kết thúc, không thể cập nhật kết quả.");
            // Cập nhật thông tin chấm điểm
            ketQua.DiemSangTao = diemSangTao ?? ketQua.DiemSangTao;
            ketQua.DiemKhaThi = diemKhaThi ?? ketQua.DiemKhaThi;
            ketQua.DiemTacDongXaHoi = diemTacDongXaHoi ?? ketQua.DiemTacDongXaHoi;
            ketQua.DiemTiemNangThiTruong = diemTiemNangThiTruong ?? ketQua.DiemTiemNangThiTruong;
            ketQua.NhanXet = nhanXet ?? ketQua.NhanXet;
            ketQua.NgayNhanXet = DateTime.Now;
            ketQua.TinhTrang = "Đã chấm";

            await _context.SaveChangesAsync();
            return Ok("Cập nhật kết quả chấm điểm thành công.");
        }
    }
}