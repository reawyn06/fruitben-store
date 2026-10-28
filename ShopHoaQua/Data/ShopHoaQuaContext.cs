using System.Data.Entity;
using ShopHoaQua.Models;

namespace ShopHoaQua.Data
{
    public class ShopHoaQuaContext : DbContext
    {
        public ShopHoaQuaContext() : base("name=ShopHoaQuaContext")
        {
        }

        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<HinhAnhSanPham> HinhAnhSanPhams { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<MaGiamGia> MaGiamGias { get; set; }
        public DbSet<LienHe> LienHes { get; set; }
        public DbSet<TinTuc> TinTucs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HinhAnhSanPham>()
                .HasRequired(h => h.SanPham)
                .WithMany(sp => sp.HinhAnhPhu)
                .HasForeignKey(h => h.SanPhamId)
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}
