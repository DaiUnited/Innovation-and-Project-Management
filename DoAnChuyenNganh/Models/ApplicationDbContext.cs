using DoAnChuyenNganh.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnChuyenNganh.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet cho các bảng
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<Khoa> Khoas { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<SinhVien> SinhViens { get; set; }
        public DbSet<Nhom> Nhoms { get; set; }
        public DbSet<ThanhVienNhom> ThanhVienNhoms { get; set; }
        public DbSet<LinhVuc> LinhVucs { get; set; }
        public DbSet<CuocThi> CuocThis { get; set; }
        public DbSet<VongThi> VongThis { get; set; }
        public DbSet<DeTai> DeTais { get; set; }
        public DbSet<KetQuaVongThi> KetQuaVongThis { get; set; }
        public DbSet<GiamKhao> GiamKhaos { get; set; }
        public DbSet<GiangVienHuongDan> GiangVienHuongDans { get; set; }
        public DbSet<BaiNop> BaiNops { get; set; }
        public DbSet<LoaiTinTuc> LoaiTinTucs { get; set; }
        public DbSet<TinTuc> TinTucs { get; set; }
        public DbSet<GiaiThuong> GiaiThuongs { get; set; }
        public DbSet<DeTaiDatGiai> DeTaiDatGiais { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ràng buộc cho VaiTro
            modelBuilder.Entity<VaiTro>()
                .HasIndex(v => v.TenVaiTro)
                .IsUnique();

            // Ràng buộc cho NguoiDung
            modelBuilder.Entity<NguoiDung>()
                .HasOne(nd => nd.VaiTro)
                .WithMany()
                .HasForeignKey(nd => nd.MaVaiTro)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<NguoiDung>()
                .HasIndex(nd => nd.TenDangNhap)
                .IsUnique();

            // Ràng buộc cho SinhVien
            modelBuilder.Entity<SinhVien>()
                .HasIndex(sv => sv.Email)
                .IsUnique();
            modelBuilder.Entity<SinhVien>()
                .HasIndex(sv => sv.SoDienThoai)
                .IsUnique();
            modelBuilder.Entity<SinhVien>()
                .HasOne(sv => sv.Khoa)
                .WithMany()
                .HasForeignKey(sv => sv.MaKhoa)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SinhVien>()
                .HasOne(sv => sv.NguoiDung)
                .WithMany()
                .HasForeignKey(sv => sv.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            // Ràng buộc cho GiangVienHuongDan
            modelBuilder.Entity<GiangVienHuongDan>()
                .HasIndex(gv => gv.Email)
                .IsUnique();
            modelBuilder.Entity<GiangVienHuongDan>()
                .HasIndex(gv => gv.SoDienThoai)
                .IsUnique();
            modelBuilder.Entity<GiangVienHuongDan>()
                .HasOne(gv => gv.Khoa)
                .WithMany()
                .HasForeignKey(gv => gv.MaKhoa)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<GiangVienHuongDan>()
                .HasOne(gv => gv.NguoiDung)
                .WithMany()
                .HasForeignKey(gv => gv.MaNguoiDung)
                .OnDelete(DeleteBehavior.Restrict);

            // Ràng buộc cho GiamKhao
            modelBuilder.Entity<GiamKhao>()
                .HasIndex(gk => gk.Email)
                .IsUnique();
            modelBuilder.Entity<GiamKhao>()
                .HasIndex(gk => gk.SoDienThoai)
                .IsUnique();
            modelBuilder.Entity<GiamKhao>()
                .HasOne(gk => gk.NguoiDung)
                .WithMany()
                .HasForeignKey(gk => gk.MaNguoiDung)
                .OnDelete(DeleteBehavior.Restrict);

            // Ràng buộc cho DeTai
            modelBuilder.Entity<DeTai>()
                .HasOne(dt => dt.GiangVienHuongDan)
                .WithMany()
                .HasForeignKey(dt => dt.MaGVHD)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DeTai>()
                .HasOne(dt => dt.LinhVuc)
                .WithMany()
                .HasForeignKey(dt => dt.MaLinhVuc)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DeTai>()
                .HasOne(dt => dt.CuocThi)
                .WithMany()
                .HasForeignKey(dt => dt.MaCuocThi)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DeTai>()
                .HasOne(dt => dt.Nhom)
                .WithMany()
                .HasForeignKey(dt => dt.MaNhom)
                .OnDelete(DeleteBehavior.Cascade);

            // Ràng buộc cho ThanhVienNhom
            modelBuilder.Entity<ThanhVienNhom>()
                .HasKey(tv => new { tv.MaNhom, tv.MaSV });
            modelBuilder.Entity<ThanhVienNhom>()
                .HasIndex(tv => tv.MaSV)
                .IsUnique();
            modelBuilder.Entity<ThanhVienNhom>()
                .HasOne(tv => tv.Nhom)
                .WithMany(n => n.ThanhVienNhoms)
                .HasForeignKey(tv => tv.MaNhom)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ThanhVienNhom>()
                .HasOne(tv => tv.SinhVien)
                .WithMany()
                .HasForeignKey(tv => tv.MaSV)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ThanhVienNhom>()
                .HasIndex(tv => new { tv.MaNhom, tv.TruongNhom })
                .IsUnique()
                .HasFilter("TruongNhom = 1");

            // Ràng buộc cho VongThi
            modelBuilder.Entity<VongThi>()
                .HasOne(vt => vt.CuocThi)
                .WithMany()
                .HasForeignKey(vt => vt.MaCuocThi)
                .OnDelete(DeleteBehavior.Cascade);

            // Ràng buộc cho KetQuaVongThi
            modelBuilder.Entity<KetQuaVongThi>()
                .HasOne(kq => kq.GiamKhao)
                .WithMany()
                .HasForeignKey(kq => kq.MaGiamKhao)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<KetQuaVongThi>()
                .HasOne(kq => kq.VongThi)
                .WithMany()
                .HasForeignKey(kq => kq.MaVongThi)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<KetQuaVongThi>()
                .HasOne(kq => kq.DeTai)
                .WithMany()
                .HasForeignKey(kq => kq.MaDeTai)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<KetQuaVongThi>()
                .HasIndex(kq => new { kq.MaGiamKhao, kq.MaVongThi })
                .IsUnique();
            modelBuilder.Entity<KetQuaVongThi>()
                .HasIndex(kq => new { kq.MaDeTai, kq.MaVongThi })
                .IsUnique();
            // Đảm bảo mỗi giám khảo chỉ chấm một đề tài duy nhất trong một vòng thi
            modelBuilder.Entity<KetQuaVongThi>()
                .HasIndex(kq => new { kq.MaGiamKhao, kq.MaVongThi })
                .IsUnique()
                .HasDatabaseName("IX_KetQuaVongThi_UniqueGiamKhaoVongThi");

            // Ràng buộc cho BaiNop
            modelBuilder.Entity<BaiNop>()
                .HasOne(bn => bn.Nhom)
                .WithMany()
                .HasForeignKey(bn => bn.MaNhom)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BaiNop>()
                .HasOne(bn => bn.DeTai)
                .WithOne(dt => dt.BaiNop)
                .HasForeignKey<BaiNop>(bn => bn.MaDeTai)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình cho LoaiTinTuc
            modelBuilder.Entity<LoaiTinTuc>()
                .HasIndex(ltt => ltt.TenLoai)
                .IsUnique();

            // Cấu hình cho TinTuc
            modelBuilder.Entity<TinTuc>()
                .HasOne(t => t.LoaiTinTuc)
                .WithMany(lt => lt.TinTucs)
                .HasForeignKey(t => t.MaLoaiTinTuc)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình cho bảng TinTuc
            modelBuilder.Entity<TinTuc>()
                .HasIndex(t => t.TieuDe)
                .IsUnique()
                .HasDatabaseName("IX_TieuDe_Unique");

            // Cấu hình cho GiaiThuong
            modelBuilder.Entity<DeTaiDatGiai>()
                .HasKey(dtg => new { dtg.MaDeTai, dtg.MaGiaiThuong});

            // Cấu hình cho DeTaiDatGiai
            modelBuilder.Entity<DeTaiDatGiai>()
                .HasOne(dtg => dtg.GiaiThuong)
                .WithMany()
                .HasForeignKey(dtg => dtg.MaGiaiThuong)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DeTaiDatGiai>()
                .HasOne(dtg => dtg.DeTai)
                .WithMany()
                .HasForeignKey(dtg => dtg.MaDeTai)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeTaiDatGiai>()
                .HasIndex(dtg => new { dtg.MaGiaiThuong, dtg.MaDeTai })
                .IsUnique()
                .HasDatabaseName("IX_GiaiThuongDeTai_Unique");

            modelBuilder.Entity<DeTaiDatGiai>()
                .HasIndex(dtg => dtg.MaDeTai)
                .IsUnique()
                .HasDatabaseName("IX_DeTaiDatGiai_MaDeTai_Unique"); // Đảm bảo mỗi đề tài chỉ được nhận một giải thưởng

            modelBuilder.Entity<DeTaiDatGiai>()
                .HasIndex(dtg => dtg.MaGiaiThuong)
                .IsUnique()
                .HasDatabaseName("IX_DeTaiDatGiai_MaGiaiThuong_Unique"); // Đảm bảo mỗi giải thưởng chỉ gắn với một đề tài
        }
    }
}
