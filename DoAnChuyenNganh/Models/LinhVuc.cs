using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("LinhVuc")]
    public class LinhVuc
    {
        [Key]
        public int MaLinhVuc { get; set; }

        [Required(ErrorMessage = "Tên lĩnh vực không được để trống")]
        [MaxLength(100)]
        public string TenLinhVuc { get; set; }
    }
}
