using DoAnChuyenNganh.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("DeTai")]
    public class DeTai
    {
        [Key]
        public int MaDeTai { get; set; }

        [Required(ErrorMessage = "Tên đề tài không được để trống")]
        [MaxLength(100)]
        public string TenDeTai { get; set; }

        [Required]
        public int MaLinhVuc { get; set; }
        [ForeignKey("MaLinhVuc")]
        public LinhVuc LinhVuc { get; set; }

        [Required]
        public int MaCuocThi { get; set; }
        [ForeignKey("MaCuocThi")]
        public CuocThi CuocThi { get; set; }

        [Required]
        public int MaNhom { get; set; }
        [ForeignKey("MaNhom")]
        public Nhom Nhom { get; set; }

        [Required]
        public int MaGVHD { get; set; }
        [ForeignKey("MaGVHD")]
        public GiangVienHuongDan GiangVienHuongDan { get; set; }

        public string MoTa { get; set; }

        public BaiNop BaiNop { get; set; }

    }

}
