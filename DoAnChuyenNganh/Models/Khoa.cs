using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DoAnChuyenNganh.Models
{
    [Table("Khoa")]
    public class Khoa
    {
        [Key]
        public int MaKhoa { get; set; }

        [Required]
        [StringLength(100)]
        public string TenKhoa { get; set; }

    }
}
