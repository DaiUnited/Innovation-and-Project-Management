using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("LoaiTinTuc")]
    public class LoaiTinTuc
    {
        [Key]
        public int MaLoaiTinTuc { get; set; }

        [Required]
        [MaxLength(50)]
        public string TenLoai { get; set; }

        [MaxLength(255)]
        public string MoTa { get; set; }

        public string FilterKey
        {
            get
            {
                return TenLoai switch
                {
                    "Cuộc thi" => "cuoc-thi",
                    "Đề tài" => "de-tai",
                    "Lĩnh vực" => "linh-vuc",
                    _ => "all"
                };
            }
        }

        // Quan hệ: Một loại tin tức có nhiều tin tức
        public ICollection<TinTuc> TinTucs { get; set; }
    }
}
