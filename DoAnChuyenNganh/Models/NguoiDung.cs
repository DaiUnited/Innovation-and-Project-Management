using DoAnChuyenNganh.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("NguoiDung")]
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [MinLength(2, ErrorMessage = "Tên đăng nhập phải có ít nhất 2 ký tự")]
        [MaxLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [MaxLength(20, ErrorMessage = "Mật khẩu không được vượt quá 20 ký tự")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vai trò không được để trống")]
        public int MaVaiTro { get; set; }

        [ForeignKey("MaVaiTro")]
        public VaiTro VaiTro { get; set; }
    }
}
