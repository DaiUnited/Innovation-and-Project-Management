using DoAnChuyenNganh.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("ThanhVienNhom")]
    public class ThanhVienNhom
    {
        [Key, Column(Order = 0)]
        public int MaNhom { get; set; }

        [Key, Column(Order = 1)]
        public int MaSV { get; set; }

        public bool TruongNhom { get; set; }

        [ForeignKey("MaNhom")]
        public Nhom Nhom { get; set; }

        [ForeignKey("MaSV")]
        public SinhVien SinhVien { get; set; }
    }
}
