using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("CuocThi")]
    public class CuocThi
    {
        [Key]
        public int MaCuocThi { get; set; }

        [Required(ErrorMessage = "Tên cuộc thi không được để trống")]
        [MaxLength(100)]
        public string TenCuocThi { get; set; }

        public string MoTa { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayBatDau { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayKetThuc { get; set; }

    }
}
