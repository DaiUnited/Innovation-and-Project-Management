using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("SinhVien")]
    public class SinhVien
    {
        [Key]
        public int MaSV { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(100)]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Lớp không được để trống")]
        [MaxLength(50)]
        public string Lop { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(20)]
        public string SoDienThoai { get; set; }

        [Required]
        public int MaKhoa { get; set; }

        [ForeignKey("MaKhoa")]
        public Khoa Khoa { get; set; }

        [Required]
        public int MaNguoiDung { get; set; }

        [ForeignKey("MaNguoiDung")]
        public NguoiDung NguoiDung { get; set; }
    }
}
