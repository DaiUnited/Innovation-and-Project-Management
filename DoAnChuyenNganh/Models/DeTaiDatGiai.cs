using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("DeTaiDatGiai")]
    public class DeTaiDatGiai
    {

        [Required]
        public int MaGiaiThuong { get; set; }
        [ForeignKey("MaGiaiThuong")]
        public GiaiThuong GiaiThuong { get; set; }

        [Required]
        public int MaDeTai { get; set; }
        [ForeignKey("MaDeTai")]
        public DeTai DeTai { get; set; }
    }
}
