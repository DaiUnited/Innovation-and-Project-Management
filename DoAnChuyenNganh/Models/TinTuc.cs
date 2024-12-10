using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("TinTuc")]
    public class TinTuc
    {
        [Key]
        public int MaTinTuc { get; set; }

        [Required]
        [MaxLength(255)]
        public string TieuDe { get; set; }

        public string AnhBia { get; set; }

        public string MoTa { get; set; }
        public string FileNoiDung { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayTao { get; set; }

        [Required]
        public int MaLoaiTinTuc { get; set; }

        [ForeignKey("MaLoaiTinTuc")]
        public LoaiTinTuc LoaiTinTuc { get; set; }

        [NotMapped]
        public string NoiDung { get; set; } // Nội dung sau khi đọc từ file Word
    }
}
