using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("Nhom")]
    public class Nhom
    {
        [Key]
        public int MaNhom { get; set; }

        [Required(ErrorMessage = "Tên nhóm không được để trống")]
        [MaxLength(100)]
        public string TenNhom { get; set; }

        // Thêm thuộc tính này để liên kết với ThanhVienNhom
        public ICollection<ThanhVienNhom> ThanhVienNhoms { get; set; }

    }
}
