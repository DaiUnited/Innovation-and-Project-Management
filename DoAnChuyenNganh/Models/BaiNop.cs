using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("BaiNop")]
    public class BaiNop
    {
        [Key]
        public int MaBaiNop { get; set; }

        [Required]
        public int MaDeTai { get; set; }
        [ForeignKey("MaDeTai")]
        public DeTai DeTai { get; set; }

        [Required]
        public int MaNhom { get; set; }

        public DateTime NgayNop { get; set; }

        public DateTime? NgayChinhSua { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Chưa nộp";

        [StringLength(255)]
        public string FileDinhKem { get; set; }

        [ForeignKey("MaNhom")]
        public Nhom Nhom { get; set; }
    }
}
