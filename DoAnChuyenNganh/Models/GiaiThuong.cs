using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("GiaiThuong")]
    public class GiaiThuong
    {
        [Key]
        public int MaGiaiThuong { get; set; }

        [Required(ErrorMessage = "Tên giải thưởng không được để trống")]
        [MaxLength(100)]
        public string TenGiaiThuong { get; set; }

        [Required(ErrorMessage = "Số tiền thưởng không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền thưởng phải lớn hơn hoặc bằng 0")]
        public decimal SoTienThuong { get; set; }

        public string MoTa { get; set; }
    }
}
