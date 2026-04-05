using HotelManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<RoomImage> RoomImages => Set<RoomImage>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Payout> Payouts => Set<Payout>();
    public DbSet<PartnerBankAccount> PartnerBankAccounts => Set<PartnerBankAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Map tên bảng ──────────────────────────────────────
        modelBuilder.Entity<User>().ToTable("nguoi_dung");
        modelBuilder.Entity<Role>().ToTable("vai_tro");
        modelBuilder.Entity<UserRole>().ToTable("nguoi_dung_vai_tro");
        modelBuilder.Entity<Room>().ToTable("phong");
        modelBuilder.Entity<Property>().ToTable("co_so_luu_tru");
        modelBuilder.Entity<RoomImage>().ToTable("anh_phong");
        modelBuilder.Entity<Booking>().ToTable("dat_phong");
        modelBuilder.Entity<Review>().ToTable("danh_gia");
        modelBuilder.Entity<Payment>().ToTable("thanh_toan");
        modelBuilder.Entity<Partner>().ToTable("doi_tac");

        // ── User ─────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.FullName).HasColumnName("ho_ten");
            e.Property(u => u.Email).HasColumnName("email");
            e.Property(u => u.Phone).HasColumnName("so_dien_thoai");
            e.Property(u => u.Cccd).HasColumnName("cccd");
            e.Property(u => u.Username).HasColumnName("ten_dang_nhap");
            e.Property(u => u.Password).HasColumnName("mat_khau");
            e.Property(u => u.Avatar).HasColumnName("anh_dai_dien");
            e.Property(u => u.Address).HasColumnName("dia_chi");
            e.Property(u => u.Status).HasColumnName("trang_thai");
            e.Property(u => u.LastActive).HasColumnName("lan_cuoi_hoat_dong");
            e.Property(u => u.CreatedAt).HasColumnName("ngay_tao");
            e.Property(u => u.UpdatedAt).HasColumnName("ngay_cap_nhat");
            e.HasIndex(u => u.Email).IsUnique();
        });

        // ── Role ─────────────────────────────────────────────
        modelBuilder.Entity<Role>(e =>
        {
            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.RoleName).HasColumnName("ten_vai_tro");
        });

        // ── UserRole (junction) ───────────────────────────────
        modelBuilder.Entity<UserRole>(e =>
        {
            e.HasKey(ur => new { ur.UserId, ur.RoleId });
            e.Property(ur => ur.UserId).HasColumnName("nguoi_dung_id");
            e.Property(ur => ur.RoleId).HasColumnName("vai_tro_id");
            e.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
            e.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
        });

        // ── Partner (doi_tac) ───────────────────────────────
        modelBuilder.Entity<Partner>(e =>
        {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.UserId).HasColumnName("nguoi_dung_id");
            e.Property(p => p.PartnerCode).HasColumnName("ma_doi_tac");
            e.Property(p => p.BusinessName).HasColumnName("ten_doanh_nghiep");
            e.Property(p => p.BusinessType).HasColumnName("loai_hinh_kinh_doanh");
            e.Property(p => p.Address).HasColumnName("dia_chi");
            e.Property(p => p.Description).HasColumnName("mo_ta");
            e.Property(p => p.Status).HasColumnName("trang_thai");
            e.Property(p => p.WalletBalance).HasColumnName("so_du_vi").HasColumnType("decimal(18,2)");
            e.Property(p => p.CreatedAt).HasColumnName("ngay_tao");
            e.Property(p => p.UpdatedAt).HasColumnName("ngay_cap_nhat");
            
            e.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId);
        });

        // ── Property ─────────────────────────────────────────────
        modelBuilder.Entity<Property>(e =>
        {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.PartnerId).HasColumnName("doi_tac_id");
            e.Property(p => p.Name).HasColumnName("ten_co_so");
            e.Property(p => p.Type).HasColumnName("loai_co_so");
            e.Property(p => p.City).HasColumnName("tinh_thanh");
            e.Property(p => p.District).HasColumnName("quan_huyen");
            e.Property(p => p.Ward).HasColumnName("phuong_xa");
            e.Property(p => p.DetailedAddress).HasColumnName("dia_chi_chi_tiet");
            e.Property(p => p.Longitude).HasColumnName("kinh_do").HasColumnType("decimal(18,8)");
            e.Property(p => p.Latitude).HasColumnName("vi_do").HasColumnType("decimal(18,8)");
            e.Property(p => p.Description).HasColumnName("mo_ta");
            e.Property(p => p.DefaultCheckInTime).HasColumnName("gio_nhan_phong_mac_dinh");
            e.Property(p => p.DefaultCheckOutTime).HasColumnName("gio_tra_phong_mac_dinh");
            e.Property(p => p.Status).HasColumnName("trang_thai");
            e.Property(p => p.AverageRating).HasColumnName("diem_trung_binh").HasColumnType("decimal(10,2)");
            e.Property(p => p.TotalReviews).HasColumnName("tong_so_danh_gia");
            e.Property(p => p.CreatedAt).HasColumnName("ngay_tao");
            e.Property(p => p.UpdatedAt).HasColumnName("ngay_cap_nhat");

            e.HasOne(p => p.Partner).WithMany(p => p.Properties).HasForeignKey(p => p.PartnerId);
        });

        // ── RoomImage ─────────────────────────────────────────────
        modelBuilder.Entity<RoomImage>(e =>
        {
            e.Property(ai => ai.Id).HasColumnName("id");
            e.Property(ai => ai.RoomId).HasColumnName("phong_id");
            e.Property(ai => ai.Url).HasColumnName("url_anh");
            e.Property(ai => ai.IsMainImage).HasColumnName("la_anh_chinh");
            e.Property(ai => ai.DisplayOrder).HasColumnName("thu_tu_hien_thi");
            e.Property(ai => ai.CreatedAt).HasColumnName("ngay_tao");
            e.HasOne(ai => ai.Room).WithMany(r => r.Images).HasForeignKey(ai => ai.RoomId);
        });

        // ── Room ─────────────────────────────────────────────
        modelBuilder.Entity<Room>(e =>
        {
            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.PropertyId).HasColumnName("co_so_id");
            e.Property(r => r.Name).HasColumnName("ten_phong");
            e.Property(r => r.RoomCode).HasColumnName("ma_phong");
            e.Property(r => r.RoomType).HasColumnName("loai_phong");
            e.Property(r => r.Capacity).HasColumnName("suc_chua");
            e.Property(r => r.BedCount).HasColumnName("so_giuong");
            e.Property(r => r.BasePrice).HasColumnName("gia_co_ban").HasColumnType("decimal(18,2)");
            e.Property(r => r.Area).HasColumnName("dien_tich").HasColumnType("decimal(18,2)");
            e.Property(r => r.Description).HasColumnName("mo_ta");
            e.Property(r => r.Status).HasColumnName("trang_thai");
            e.Property(r => r.IsApproved).HasColumnName("duoc_duyet");
            e.Property(r => r.CreatedAt).HasColumnName("ngay_tao");
            e.Property(r => r.UpdatedAt).HasColumnName("ngay_cap_nhat");
            e.HasOne(r => r.Property).WithMany(p => p.Rooms).HasForeignKey(r => r.PropertyId);
        });

        // ── Booking ───────────────────────────────────────────
        modelBuilder.Entity<Booking>(e =>
        {
            e.Property(b => b.Id).HasColumnName("id");
            e.Property(b => b.UserId).HasColumnName("nguoi_dung_id");
            e.Property(b => b.RoomId).HasColumnName("phong_id");
            e.Property(b => b.CheckIn).HasColumnName("ngay_nhan_phong");
            e.Property(b => b.CheckOut).HasColumnName("ngay_tra_phong");
            e.Property(b => b.Nights).HasColumnName("so_dem");
            e.Property(b => b.GuestCount).HasColumnName("so_nguoi");
            e.Property(b => b.CheckInTime).HasColumnName("gio_nhan_phong");
            e.Property(b => b.CheckOutTime).HasColumnName("gio_tra_phong");
            e.Property(b => b.PricePerNight).HasColumnName("gia_moi_dem").HasColumnType("decimal(18,2)");
            e.Property(b => b.CleaningFee).HasColumnName("phi_don_dep").HasColumnType("decimal(18,2)");
            e.Property(b => b.ServiceFee).HasColumnName("phi_dich_vu").HasColumnType("decimal(18,2)");
            e.Property(b => b.Tax).HasColumnName("thue").HasColumnType("decimal(18,2)");
            e.Property(b => b.TotalPrice).HasColumnName("tong_tien").HasColumnType("decimal(18,2)");
            e.Property(b => b.Deposit).HasColumnName("dat_coc").HasColumnType("decimal(18,2)");
            e.Property(b => b.Status).HasColumnName("trang_thai");
            e.Property(b => b.Note).HasColumnName("ghi_chu");
            e.Property(b => b.CreatedAt).HasColumnName("ngay_tao");
            e.Property(b => b.UpdatedAt).HasColumnName("ngay_cap_nhat");
            e.HasOne(b => b.User).WithMany(u => u.Bookings).HasForeignKey(b => b.UserId);
            e.HasOne(b => b.Room).WithMany(r => r.Bookings).HasForeignKey(b => b.RoomId);
        });

        // ── Review ────────────────────────────────────────────
        modelBuilder.Entity<Review>(e =>
        {
            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.UserId).HasColumnName("nguoi_dung_id");
            e.Property(r => r.RoomId).HasColumnName("phong_id");
            e.Property(r => r.BookingId).HasColumnName("dat_phong_id");
            e.Property(r => r.Stars).HasColumnName("so_sao");
            e.Property(r => r.Comment).HasColumnName("binh_luan");
            e.Property(r => r.CreatedAt).HasColumnName("ngay_tao");
            e.HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId);
            e.HasOne(r => r.Room).WithMany(r => r.Reviews).HasForeignKey(r => r.RoomId);
        });

        // ── Payment ───────────────────────────────────────────
        modelBuilder.Entity<Payment>(e =>
        {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.BookingId).HasColumnName("dat_phong_id");
            e.Property(p => p.PaymentType).HasColumnName("loai_thanh_toan");
            e.Property(p => p.Amount).HasColumnName("so_tien").HasColumnType("decimal(18,2)");
            e.Property(p => p.Method).HasColumnName("phuong_thuc");
            e.Property(p => p.Status).HasColumnName("trang_thai");
            e.Property(p => p.DueDate).HasColumnName("han_thanh_toan");
            e.Property(p => p.PaidAt).HasColumnName("ngay_thanh_toan");
            e.Property(p => p.CreatedAt).HasColumnName("ngay_tao");
            e.HasOne(p => p.Booking).WithMany().HasForeignKey(p => p.BookingId);
        });

        // ── Payout (thanh_toan_doi_tac) ───────────────────────
        modelBuilder.Entity<Payout>(e =>
        {
            e.ToTable("thanh_toan_doi_tac");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.PartnerId).HasColumnName("doi_tac_id");
            e.Property(p => p.Amount).HasColumnName("so_tien").HasColumnType("decimal(18,2)");
            e.Property(p => p.BankName).HasColumnName("ten_ngan_hang");
            e.Property(p => p.AccountNumber).HasColumnName("so_tai_khoan");
            e.Property(p => p.AccountHolder).HasColumnName("chu_tai_khoan");
            e.Property(p => p.Status).HasColumnName("trang_thai");
            e.Property(p => p.ProofImageUrl).HasColumnName("url_bang_chung");
            e.Property(p => p.TransactionId).HasColumnName("id_giao_dich");
            e.Property(p => p.AdminNote).HasColumnName("ghi_chu_admin");
            e.Property(p => p.CreatedAt).HasColumnName("ngay_tao");
            e.Property(p => p.UpdatedAt).HasColumnName("ngay_cap_nhat");
            e.Property(p => p.PaidAt).HasColumnName("ngay_thanh_toan");

            e.HasOne(p => p.Partner).WithMany().HasForeignKey(p => p.PartnerId);
        });

        // ── PartnerBankAccount (tai_khoan_ngan_hang_doi_tac) ──
        modelBuilder.Entity<PartnerBankAccount>(e =>
        {
            e.ToTable("tai_khoan_ngan_hang_doi_tac");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.PartnerId).HasColumnName("doi_tac_id");
            e.Property(p => p.BankName).HasColumnName("ten_ngan_hang");
            e.Property(p => p.AccountNumber).HasColumnName("so_tai_khoan");
            e.Property(p => p.AccountHolder).HasColumnName("chu_tai_khoan");
            e.Property(p => p.IsPrimary).HasColumnName("la_tai_khoan_chinh");
            e.Property(p => p.CreatedAt).HasColumnName("ngay_tao");
            e.Property(p => p.UpdatedAt).HasColumnName("ngay_cap_nhat");

            e.HasOne(p => p.Partner).WithMany().HasForeignKey(p => p.PartnerId);
        });
    }
}
//dotnet ef database update