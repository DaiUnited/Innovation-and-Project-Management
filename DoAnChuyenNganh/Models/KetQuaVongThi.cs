using DoAnChuyenNganh.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnChuyenNganh.Models
{
    [Table("KetQuaVongThi")]
    public class KetQuaVongThi
    {
        [Key]
        public int MaKetQua { get; set; }

        [Required]
        public int MaGiamKhao { get; set; }
        [ForeignKey("MaGiamKhao")]
        public GiamKhao GiamKhao { get; set; }

        [Required]
        public int MaVongThi { get; set; }
        [ForeignKey("MaVongThi")]
        public VongThi VongThi { get; set; }

        [Required]
        public int MaDeTai { get; set; }
        [ForeignKey("MaDeTai")]
        public DeTai DeTai { get; set; }

        [Range(0, 10)]
        public double DiemSangTao { get; set; }

        [Range(0, 10)]
        public double DiemKhaThi { get; set; }

        [Range(0, 10)]
        public double DiemTacDongXaHoi { get; set; }

        [Range(0, 10)]
        public double DiemTiemNangThiTruong { get; set; }

        [NotMapped]
        public double TongDiem => DiemSangTao + DiemKhaThi + DiemTacDongXaHoi + DiemTiemNangThiTruong;

        public string TinhTrang { get; set; }

        public string NhanXet { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayNhanXet { get; set; }

        public int XepHang { get; set; }


    }

}
