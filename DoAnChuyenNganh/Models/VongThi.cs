using DoAnChuyenNganh.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("VongThi")]
    public class VongThi
    {
        [Key]
        public int MaVongThi { get; set; }

        [Required]
        public int MaCuocThi { get; set; }
        [ForeignKey("MaCuocThi")]
        public CuocThi CuocThi { get; set; }

        [Required(ErrorMessage = "Tên vòng thi không được để trống")]
        [MaxLength(100)]
        public string TenVongThi { get; set; }

        public int ThuTu { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayBatDau { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayKetThuc { get; set; }
    }
}
