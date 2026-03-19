using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace driving_school_management.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Anhgksk> Anhgksks { get; set; }

    public virtual DbSet<Bailam> Bailams { get; set; }

    public virtual DbSet<Bailammophong> Bailammophongs { get; set; }

    public virtual DbSet<Baithi> Baithis { get; set; }

    public virtual DbSet<Bodemophong> Bodemophongs { get; set; }

    public virtual DbSet<Bodethithu> Bodethithus { get; set; }

    public virtual DbSet<Canbogiamsat> Canbogiamsats { get; set; }

    public virtual DbSet<Cauhoilythuyet> Cauhoilythuyets { get; set; }

    public virtual DbSet<Chitietbailam> Chitietbailams { get; set; }

    public virtual DbSet<Chitietbodemophong> Chitietbodemophongs { get; set; }

    public virtual DbSet<Chitietbodetracnghiem> Chitietbodetracnghiems { get; set; }

    public virtual DbSet<Chitietdangkythi> Chitietdangkythis { get; set; }

    public virtual DbSet<Chitietgplx> Chitietgplxes { get; set; }

    public virtual DbSet<Chitietketquahoctap> Chitietketquahoctaps { get; set; }

    public virtual DbSet<Chitietketquathi> Chitietketquathis { get; set; }

    public virtual DbSet<Chitietphanconggiamsat> Chitietphanconggiamsats { get; set; }

    public virtual DbSet<Chitietphieuthanhtoan> Chitietphieuthanhtoans { get; set; }

    public virtual DbSet<Chuong> Chuongs { get; set; }

    public virtual DbSet<Chuongmophong> Chuongmophongs { get; set; }

    public virtual DbSet<Dapan> Dapans { get; set; }

    public virtual DbSet<Diemtungtinhhuong> Diemtungtinhhuongs { get; set; }

    public virtual DbSet<Giaypheplaixe> Giaypheplaixes { get; set; }

    public virtual DbSet<Hanggplx> Hanggplxes { get; set; }

    public virtual DbSet<Hocvien> Hocviens { get; set; }

    public virtual DbSet<Hosothisinh> Hosothisinhs { get; set; }

    public virtual DbSet<Ketquahoctap> Ketquahoctaps { get; set; }

    public virtual DbSet<Khoahoc> Khoahocs { get; set; }

    public virtual DbSet<Kythi> Kythis { get; set; }

    public virtual DbSet<Lichthi> Lichthis { get; set; }

    public virtual DbSet<Phieukhamsuckhoe> Phieukhamsuckhoes { get; set; }

    public virtual DbSet<Phieuthanhtoan> Phieuthanhtoans { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Tinhhuongmophong> Tinhhuongmophongs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Yeucaunanghang> Yeucaunanghangs { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseOracle("User Id=C##HAIIT;Password=hAi0907@!;Data Source=//127.0.0.1:1522/orcl19pdb;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("C##HAIIT")
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<Anhgksk>(entity =>
        {
            entity.HasKey(e => e.Anhid);

            entity.ToTable("ANHGKSK");

            entity.Property(e => e.Anhid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ANHID");
            entity.Property(e => e.Khamsuckhoeid)
                .HasColumnType("NUMBER")
                .HasColumnName("KHAMSUCKHOEID");
            entity.Property(e => e.Urlanh)
                .HasMaxLength(300)
                .HasColumnName("URLANH");

            entity.HasOne(d => d.Khamsuckhoe).WithMany(p => p.Anhgksks)
                .HasForeignKey(d => d.Khamsuckhoeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ANHGKSK_PHIEUKHAMSUCKHOE");
        });

        modelBuilder.Entity<Bailam>(entity =>
        {
            entity.ToTable("BAILAM");

            entity.Property(e => e.Bailamid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("BAILAMID");
            entity.Property(e => e.Bodeid)
                .HasColumnType("NUMBER")
                .HasColumnName("BODEID");
            entity.Property(e => e.Ketqua)
                .HasColumnType("NUMBER(1)")
                .HasColumnName("KETQUA");
            entity.Property(e => e.Socausai)
                .HasDefaultValueSql("0 ")
                .HasColumnType("NUMBER")
                .HasColumnName("SOCAUSAI");
            entity.Property(e => e.Thoigianlambai)
                .HasColumnType("NUMBER")
                .HasColumnName("THOIGIANLAMBAI");
            entity.Property(e => e.Userid)
                .HasColumnType("NUMBER")
                .HasColumnName("USERID");

            entity.HasOne(d => d.Bode).WithMany(p => p.Bailams)
                .HasForeignKey(d => d.Bodeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BAILAM_BODETHITHU");

            entity.HasOne(d => d.User).WithMany(p => p.Bailams)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BAILAM_USER");
        });

        modelBuilder.Entity<Bailammophong>(entity =>
        {
            entity.ToTable("BAILAMMOPHONG");

            entity.Property(e => e.Bailammophongid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("BAILAMMOPHONGID");
            entity.Property(e => e.Bodemophongid)
                .HasColumnType("NUMBER")
                .HasColumnName("BODEMOPHONGID");
            entity.Property(e => e.Ketqua)
                .HasColumnType("NUMBER(1)")
                .HasColumnName("KETQUA");
            entity.Property(e => e.Tongdiem)
                .HasDefaultValueSql("0 ")
                .HasColumnType("NUMBER")
                .HasColumnName("TONGDIEM");
            entity.Property(e => e.Userid)
                .HasColumnType("NUMBER")
                .HasColumnName("USERID");

            entity.HasOne(d => d.Bodemophong).WithMany(p => p.Bailammophongs)
                .HasForeignKey(d => d.Bodemophongid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BAILAMMOPHONG_BODEMOPHONG");

            entity.HasOne(d => d.User).WithMany(p => p.Bailammophongs)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BAILAMMOPHONG_USER");
        });

        modelBuilder.Entity<Baithi>(entity =>
        {
            entity.ToTable("BAITHI");

            entity.Property(e => e.Baithiid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("BAITHIID");
            entity.Property(e => e.Kythiid)
                .HasColumnType("NUMBER")
                .HasColumnName("KYTHIID");
            entity.Property(e => e.Loaibaithi)
                .HasMaxLength(50)
                .HasColumnName("LOAIBAITHI");
            entity.Property(e => e.Mota)
                .HasMaxLength(255)
                .HasColumnName("MOTA");
            entity.Property(e => e.Tenbaithi)
                .HasMaxLength(100)
                .HasColumnName("TENBAITHI");

            entity.HasOne(d => d.Kythi).WithMany(p => p.Baithis)
                .HasForeignKey(d => d.Kythiid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BAITHI_KYTHI");
        });

        modelBuilder.Entity<Bodemophong>(entity =>
        {
            entity.ToTable("BODEMOPHONG");

            entity.Property(e => e.Bodemophongid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("BODEMOPHONGID");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("1 ")
                .HasColumnType("NUMBER(1)")
                .HasColumnName("ISACTIVE");
            entity.Property(e => e.Sotinhhuong)
                .HasColumnType("NUMBER")
                .HasColumnName("SOTINHHUONG");
            entity.Property(e => e.Taoluc)
                .HasPrecision(3)
                .HasDefaultValueSql("SYSTIMESTAMP ")
                .HasColumnName("TAOLUC");
            entity.Property(e => e.Tenbode)
                .HasMaxLength(255)
                .HasColumnName("TENBODE");
        });

        modelBuilder.Entity<Bodethithu>(entity =>
        {
            entity.HasKey(e => e.Bodeid);

            entity.ToTable("BODETHITHU");

            entity.Property(e => e.Bodeid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("BODEID");
            entity.Property(e => e.Hangid)
                .HasColumnType("NUMBER")
                .HasColumnName("HANGID");
            entity.Property(e => e.Hoatdong)
                .IsRequired()
                .HasDefaultValueSql("1 ")
                .HasColumnType("NUMBER(1)")
                .HasColumnName("HOATDONG");
            entity.Property(e => e.Socauhoi)
                .HasColumnType("NUMBER")
                .HasColumnName("SOCAUHOI");
            entity.Property(e => e.Taoluc)
                .HasPrecision(3)
                .HasDefaultValueSql("SYSTIMESTAMP ")
                .HasColumnName("TAOLUC");
            entity.Property(e => e.Tenbode)
                .HasMaxLength(255)
                .HasColumnName("TENBODE");
            entity.Property(e => e.Thoigian)
                .HasColumnType("NUMBER")
                .HasColumnName("THOIGIAN");

            entity.HasOne(d => d.Hang).WithMany(p => p.Bodethithus)
                .HasForeignKey(d => d.Hangid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BODETHITHU_HANGGPLX");
        });

        modelBuilder.Entity<Canbogiamsat>(entity =>
        {
            entity.HasKey(e => e.Canboid);

            entity.ToTable("CANBOGIAMSAT");

            entity.Property(e => e.Canboid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("CANBOID");
            entity.Property(e => e.Diachi)
                .HasMaxLength(255)
                .HasColumnName("DIACHI");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Gioitinh)
                .HasMaxLength(10)
                .HasColumnName("GIOITINH");
            entity.Property(e => e.Hoten)
                .HasMaxLength(100)
                .HasColumnName("HOTEN");
            entity.Property(e => e.Ngaysinh)
                .HasColumnType("DATE")
                .HasColumnName("NGAYSINH");
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .HasColumnName("SDT");
        });

        modelBuilder.Entity<Cauhoilythuyet>(entity =>
        {
            entity.HasKey(e => e.Cauhoiid);

            entity.ToTable("CAUHOILYTHUYET");

            entity.Property(e => e.Cauhoiid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("CAUHOIID");
            entity.Property(e => e.Cauliet)
                .HasColumnType("NUMBER(1)")
                .HasColumnName("CAULIET");
            entity.Property(e => e.Chuongid)
                .HasColumnType("NUMBER")
                .HasColumnName("CHUONGID");
            entity.Property(e => e.Chuy)
                .HasColumnType("NUMBER(1)")
                .HasColumnName("CHUY");
            entity.Property(e => e.Hinhanh)
                .HasMaxLength(500)
                .HasColumnName("HINHANH");
            entity.Property(e => e.Noidung)
                .HasColumnType("NCLOB")
                .HasColumnName("NOIDUNG");
            entity.Property(e => e.Urlanhmeo)
                .HasMaxLength(500)
                .HasColumnName("URLANHMEO");
            entity.Property(e => e.Xemay)
                .HasColumnType("NUMBER(1)")
                .HasColumnName("XEMAY");

            entity.HasOne(d => d.Chuong).WithMany(p => p.Cauhoilythuyets)
                .HasForeignKey(d => d.Chuongid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CAUHOILYTHUYET_CHUONG");
        });

        modelBuilder.Entity<Chitietbailam>(entity =>
        {
            entity.HasKey(e => new { e.Bailamid, e.Cauhoiid });

            entity.ToTable("CHITIETBAILAM");

            entity.Property(e => e.Bailamid)
                .HasColumnType("NUMBER")
                .HasColumnName("BAILAMID");
            entity.Property(e => e.Cauhoiid)
                .HasColumnType("NUMBER")
                .HasColumnName("CAUHOIID");
            entity.Property(e => e.Dapandachon)
                .HasMaxLength(50)
                .HasColumnName("DAPANDACHON");
            entity.Property(e => e.Ketquacau)
                .HasColumnType("NUMBER(1)")
                .HasColumnName("KETQUACAU");

            entity.HasOne(d => d.Bailam).WithMany(p => p.Chitietbailams)
                .HasForeignKey(d => d.Bailamid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETBAILAM_BAILAM");

            entity.HasOne(d => d.Cauhoi).WithMany(p => p.Chitietbailams)
                .HasForeignKey(d => d.Cauhoiid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETBAILAM_CAUHOILYTHUYET");
        });

        modelBuilder.Entity<Chitietbodemophong>(entity =>
        {
            entity.HasKey(e => new { e.Bodemophongid, e.Tinhhuongmophongid });

            entity.ToTable("CHITIETBODEMOPHONG");

            entity.Property(e => e.Bodemophongid)
                .HasColumnType("NUMBER")
                .HasColumnName("BODEMOPHONGID");
            entity.Property(e => e.Tinhhuongmophongid)
                .HasColumnType("NUMBER")
                .HasColumnName("TINHHUONGMOPHONGID");
            entity.Property(e => e.Thutu)
                .HasColumnType("NUMBER")
                .HasColumnName("THUTU");

            entity.HasOne(d => d.Bodemophong).WithMany(p => p.Chitietbodemophongs)
                .HasForeignKey(d => d.Bodemophongid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETBODEMOPHONG_BODEMOPHONG");

            entity.HasOne(d => d.Tinhhuongmophong).WithMany(p => p.Chitietbodemophongs)
                .HasForeignKey(d => d.Tinhhuongmophongid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETBODEMOPHONG_TINHHUONGMOPHONG");
        });

        modelBuilder.Entity<Chitietbodetracnghiem>(entity =>
        {
            entity.HasKey(e => new { e.Bodeid, e.Cauhoiid });

            entity.ToTable("CHITIETBODETRACNGHIEM");

            entity.Property(e => e.Bodeid)
                .HasColumnType("NUMBER")
                .HasColumnName("BODEID");
            entity.Property(e => e.Cauhoiid)
                .HasColumnType("NUMBER")
                .HasColumnName("CAUHOIID");
            entity.Property(e => e.Thutu)
                .HasDefaultValueSql("0 ")
                .HasColumnType("NUMBER")
                .HasColumnName("THUTU");

            entity.HasOne(d => d.Bode).WithMany(p => p.Chitietbodetracnghiems)
                .HasForeignKey(d => d.Bodeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETBODETRACNGHIEM_BODETHITHU");

            entity.HasOne(d => d.Cauhoi).WithMany(p => p.Chitietbodetracnghiems)
                .HasForeignKey(d => d.Cauhoiid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETBODETRACNGHIEM_CAUHOILYTHUYET");
        });

        modelBuilder.Entity<Chitietdangkythi>(entity =>
        {
            entity.HasKey(e => new { e.Kythiid, e.Hosoid });

            entity.ToTable("CHITIETDANGKYTHI");

            entity.Property(e => e.Kythiid)
                .HasColumnType("NUMBER")
                .HasColumnName("KYTHIID");
            entity.Property(e => e.Hosoid)
                .HasColumnType("NUMBER")
                .HasColumnName("HOSOID");
            entity.Property(e => e.Thoigiandangky)
                .HasPrecision(6)
                .HasColumnName("THOIGIANDANGKY");

            entity.HasOne(d => d.Hoso).WithMany(p => p.Chitietdangkythis)
                .HasForeignKey(d => d.Hosoid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETDANGKYTHI_HOSOTHISINH");

            entity.HasOne(d => d.Kythi).WithMany(p => p.Chitietdangkythis)
                .HasForeignKey(d => d.Kythiid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETDANGKYTHI_KYTHI");
        });

        modelBuilder.Entity<Chitietgplx>(entity =>
        {
            entity.HasKey(e => new { e.Hangid, e.Gplxid });

            entity.ToTable("CHITIETGPLX");

            entity.Property(e => e.Hangid)
                .HasColumnType("NUMBER")
                .HasColumnName("HANGID");
            entity.Property(e => e.Gplxid)
                .HasColumnType("NUMBER")
                .HasColumnName("GPLXID");
            entity.Property(e => e.Ngaycapchitiet)
                .HasColumnType("DATE")
                .HasColumnName("NGAYCAPCHITIET");

            entity.HasOne(d => d.Gplx).WithMany(p => p.Chitietgplxes)
                .HasForeignKey(d => d.Gplxid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETGPLX_GIAYPHEPLAIXE");

            entity.HasOne(d => d.Hang).WithMany(p => p.Chitietgplxes)
                .HasForeignKey(d => d.Hangid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETGPLX_HANGGPLX");
        });

        modelBuilder.Entity<Chitietketquahoctap>(entity =>
        {
            entity.HasKey(e => e.Ketquahoctapid);

            entity.ToTable("CHITIETKETQUAHOCTAP");

            entity.Property(e => e.Ketquahoctapid)
                .HasColumnType("NUMBER")
                .HasColumnName("KETQUAHOCTAPID");
            entity.Property(e => e.Duongtruongkq)
                .HasColumnType("NUMBER(1)")
                .HasColumnName("DUONGTRUONGKQ");
            entity.Property(e => e.Khoahocid)
                .HasColumnType("NUMBER")
                .HasColumnName("KHOAHOCID");
            entity.Property(e => e.Lythuyetkq)
                .HasColumnType("NUMBER(1)")
                .HasColumnName("LYTHUYETKQ");
            entity.Property(e => e.Mophongkq)
                .HasColumnType("NUMBER(1)")
                .HasColumnName("MOPHONGKQ");
            entity.Property(e => e.Sahinhkq)
                .HasColumnType("NUMBER(1)")
                .HasColumnName("SAHINHKQ");

            entity.HasOne(d => d.Ketquahoctap).WithOne(p => p.Chitietketquahoctap)
                .HasForeignKey<Chitietketquahoctap>(d => d.Ketquahoctapid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETKETQUAHOCTAP_KETQUAHOCTAP");

            entity.HasOne(d => d.Khoahoc).WithMany(p => p.Chitietketquahoctaps)
                .HasForeignKey(d => d.Khoahocid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETKETQUAHOCTAP_KHOAHOC");
        });

        modelBuilder.Entity<Chitietketquathi>(entity =>
        {
            entity.HasKey(e => new { e.Baithiid, e.Hosoid });

            entity.ToTable("CHITIETKETQUATHI");

            entity.Property(e => e.Baithiid)
                .HasColumnType("NUMBER")
                .HasColumnName("BAITHIID");
            entity.Property(e => e.Hosoid)
                .HasColumnType("NUMBER")
                .HasColumnName("HOSOID");
            entity.Property(e => e.Ketquadatduoc)
                .HasMaxLength(50)
                .HasColumnName("KETQUADATDUOC");
            entity.Property(e => e.Tongdiem).HasColumnName("TONGDIEM");

            entity.HasOne(d => d.Baithi).WithMany(p => p.Chitietketquathis)
                .HasForeignKey(d => d.Baithiid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETKETQUATHI_BAITHI");

            entity.HasOne(d => d.Hoso).WithMany(p => p.Chitietketquathis)
                .HasForeignKey(d => d.Hosoid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETKETQUATHI_HOSOTHISINH");
        });

        modelBuilder.Entity<Chitietphanconggiamsat>(entity =>
        {
            entity.HasKey(e => new { e.Baithiid, e.Canboid });

            entity.ToTable("CHITIETPHANCONGGIAMSAT");

            entity.Property(e => e.Baithiid)
                .HasColumnType("NUMBER")
                .HasColumnName("BAITHIID");
            entity.Property(e => e.Canboid)
                .HasColumnType("NUMBER")
                .HasColumnName("CANBOID");
            entity.Property(e => e.Ghichu)
                .HasMaxLength(255)
                .HasColumnName("GHICHU");
            entity.Property(e => e.Phongthi)
                .HasMaxLength(100)
                .HasColumnName("PHONGTHI");
            entity.Property(e => e.Thoigianbatdau)
                .HasPrecision(6)
                .HasColumnName("THOIGIANBATDAU");
            entity.Property(e => e.Thoigianketthuc)
                .HasPrecision(6)
                .HasColumnName("THOIGIANKETTHUC");

            entity.HasOne(d => d.Baithi).WithMany(p => p.Chitietphanconggiamsats)
                .HasForeignKey(d => d.Baithiid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETPHANCONGGIAMSAT_BAITHI");

            entity.HasOne(d => d.Canbo).WithMany(p => p.Chitietphanconggiamsats)
                .HasForeignKey(d => d.Canboid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETPHANCONGGIAMSAT_CANBOGIAMSAT");
        });

        modelBuilder.Entity<Chitietphieuthanhtoan>(entity =>
        {
            entity.HasKey(e => new { e.Hosoid, e.Phieuid });

            entity.ToTable("CHITIETPHIEUTHANHTOAN");

            entity.Property(e => e.Hosoid)
                .HasColumnType("NUMBER")
                .HasColumnName("HOSOID");
            entity.Property(e => e.Phieuid)
                .HasColumnType("NUMBER")
                .HasColumnName("PHIEUID");
            entity.Property(e => e.Ghichu)
                .HasMaxLength(255)
                .HasColumnName("GHICHU");
            entity.Property(e => e.Loaiphi)
                .HasMaxLength(100)
                .HasColumnName("LOAIPHI");

            entity.HasOne(d => d.Hoso).WithMany(p => p.Chitietphieuthanhtoans)
                .HasForeignKey(d => d.Hosoid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETPHIEUTHANHTOAN_HOSOTHISINH");

            entity.HasOne(d => d.Phieu).WithMany(p => p.Chitietphieuthanhtoans)
                .HasForeignKey(d => d.Phieuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIETPHIEUTHANHTOAN_PHIEUTHANHTOAN");
        });

        modelBuilder.Entity<Chuong>(entity =>
        {
            entity.ToTable("CHUONG");

            entity.Property(e => e.Chuongid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("CHUONGID");
            entity.Property(e => e.Tenchuong)
                .HasMaxLength(255)
                .HasColumnName("TENCHUONG");
            entity.Property(e => e.Thutu)
                .HasColumnType("NUMBER")
                .HasColumnName("THUTU");
        });

        modelBuilder.Entity<Chuongmophong>(entity =>
        {
            entity.ToTable("CHUONGMOPHONG");

            entity.Property(e => e.Chuongmophongid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("CHUONGMOPHONGID");
            entity.Property(e => e.Tenchuong)
                .HasMaxLength(255)
                .HasColumnName("TENCHUONG");
            entity.Property(e => e.Thutu)
                .HasColumnType("NUMBER")
                .HasColumnName("THUTU");
        });

        modelBuilder.Entity<Dapan>(entity =>
        {
            entity.ToTable("DAPAN");

            entity.Property(e => e.Dapanid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("DAPANID");
            entity.Property(e => e.Cauhoiid)
                .HasColumnType("NUMBER")
                .HasColumnName("CAUHOIID");
            entity.Property(e => e.Dapandung)
                .IsRequired()
                .HasDefaultValueSql("0 ")
                .HasColumnType("NUMBER(1)")
                .HasColumnName("DAPANDUNG");
            entity.Property(e => e.Noidung)
                .HasColumnType("NCLOB")
                .HasColumnName("NOIDUNG");
            entity.Property(e => e.Thutu)
                .HasDefaultValueSql("0 ")
                .HasColumnType("NUMBER")
                .HasColumnName("THUTU");

            entity.HasOne(d => d.Cauhoi).WithMany(p => p.Dapans)
                .HasForeignKey(d => d.Cauhoiid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DAPAN_CAUHOILYTHUYET");
        });

        modelBuilder.Entity<Diemtungtinhhuong>(entity =>
        {
            entity.HasKey(e => new { e.Bailammophongid, e.Tinhhuongmophongid });

            entity.ToTable("DIEMTUNGTINHHUONG");

            entity.Property(e => e.Bailammophongid)
                .HasColumnType("NUMBER")
                .HasColumnName("BAILAMMOPHONGID");
            entity.Property(e => e.Tinhhuongmophongid)
                .HasColumnType("NUMBER")
                .HasColumnName("TINHHUONGMOPHONGID");
            entity.Property(e => e.Thoidiemnguoidungnhan).HasColumnName("THOIDIEMNGUOIDUNGNHAN");

            entity.HasOne(d => d.Bailammophong).WithMany(p => p.Diemtungtinhhuongs)
                .HasForeignKey(d => d.Bailammophongid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DIEMTUNGTINHHUONG_BAILAMMOPHONG");

            entity.HasOne(d => d.Tinhhuongmophong).WithMany(p => p.Diemtungtinhhuongs)
                .HasForeignKey(d => d.Tinhhuongmophongid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DIEMTUNGTINHHUONG_TINHHUONGMOPHONG");
        });

        modelBuilder.Entity<Giaypheplaixe>(entity =>
        {
            entity.HasKey(e => e.Gplxid);

            entity.ToTable("GIAYPHEPLAIXE");

            entity.Property(e => e.Gplxid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("GPLXID");
            entity.Property(e => e.Hosoid)
                .HasColumnType("NUMBER")
                .HasColumnName("HOSOID");
            entity.Property(e => e.Ngaycap)
                .HasColumnType("DATE")
                .HasColumnName("NGAYCAP");
            entity.Property(e => e.Ngayhethan)
                .HasColumnType("DATE")
                .HasColumnName("NGAYHETHAN");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(50)
                .HasColumnName("TRANGTHAI");

            entity.HasOne(d => d.Hoso).WithMany(p => p.Giaypheplaixes)
                .HasForeignKey(d => d.Hosoid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GIAYPHEPLAIXE_HOSOTHISINH");
        });

        modelBuilder.Entity<Hanggplx>(entity =>
        {
            entity.HasKey(e => e.Hangid);

            entity.ToTable("HANGGPLX");

            entity.Property(e => e.Hangid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("HANGID");
            entity.Property(e => e.Diemdat)
                .HasDefaultValueSql("0 ")
                .HasColumnType("NUMBER")
                .HasColumnName("DIEMDAT");
            entity.Property(e => e.Hocphi)
                .HasColumnType("NUMBER(18,2)")
                .HasColumnName("HOCPHI");
            entity.Property(e => e.Loaiphuongtien)
                .HasMaxLength(100)
                .HasColumnName("LOAIPHUONGTIEN");
            entity.Property(e => e.Mahang)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("MAHANG");
            entity.Property(e => e.Mota)
                .HasColumnType("NCLOB")
                .HasColumnName("MOTA");
            entity.Property(e => e.Socauhoi)
                .HasDefaultValueSql("0 ")
                .HasColumnType("NUMBER")
                .HasColumnName("SOCAUHOI");
            entity.Property(e => e.Tenhang)
                .HasMaxLength(20)
                .HasColumnName("TENHANG");
            entity.Property(e => e.Thoigiantn)
                .HasColumnType("NUMBER")
                .HasColumnName("THOIGIANTN");
            entity.Property(e => e.Thoihanlythuyet)
                .HasColumnType("NUMBER")
                .HasColumnName("THOIHANLYTHUYET");
            entity.Property(e => e.Thoihanthuchanh)
                .HasColumnType("NUMBER")
                .HasColumnName("THOIHANTHUCHANH");
        });

        modelBuilder.Entity<Hocvien>(entity =>
        {
            entity.ToTable("HOCVIEN");

            entity.Property(e => e.Hocvienid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("HOCVIENID");
            entity.Property(e => e.Avatarurl)
                .HasMaxLength(500)
                .HasColumnName("AVATARURL");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Gioitinh)
                .HasMaxLength(10)
                .HasColumnName("GIOITINH");
            entity.Property(e => e.Hoten)
                .HasMaxLength(100)
                .HasColumnName("HOTEN");
            entity.Property(e => e.Namsinh)
                .HasColumnType("DATE")
                .HasColumnName("NAMSINH");
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .HasColumnName("SDT");
            entity.Property(e => e.Socmndcccd)
                .HasMaxLength(20)
                .HasColumnName("SOCMNDCCCD");
            entity.Property(e => e.Userid)
                .HasColumnType("NUMBER")
                .HasColumnName("USERID");

            entity.HasOne(d => d.User).WithMany(p => p.Hocviens)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("FK_HOCVIEN_USER");
        });

        modelBuilder.Entity<Hosothisinh>(entity =>
        {
            entity.HasKey(e => e.Hosoid);

            entity.ToTable("HOSOTHISINH");

            entity.Property(e => e.Hosoid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("HOSOID");
            entity.Property(e => e.Ghichu)
                .HasMaxLength(255)
                .HasColumnName("GHICHU");
            entity.Property(e => e.Hangid)
                .HasColumnType("NUMBER")
                .HasColumnName("HANGID");
            entity.Property(e => e.Hocvienid)
                .HasColumnType("NUMBER")
                .HasColumnName("HOCVIENID");
            entity.Property(e => e.Khamsuckhoeid)
                .HasColumnType("NUMBER")
                .HasColumnName("KHAMSUCKHOEID");
            entity.Property(e => e.Loaihoso)
                .HasMaxLength(50)
                .HasColumnName("LOAIHOSO");
            entity.Property(e => e.Ngaydangky)
                .HasColumnType("DATE")
                .HasColumnName("NGAYDANGKY");
            entity.Property(e => e.Tenhoso)
                .HasMaxLength(100)
                .HasColumnName("TENHOSO");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(50)
                .HasColumnName("TRANGTHAI");

            entity.HasOne(d => d.Hang).WithMany(p => p.Hosothisinhs)
                .HasForeignKey(d => d.Hangid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HOSOTHISINH_HANGGPLX");

            entity.HasOne(d => d.Hocvien).WithMany(p => p.Hosothisinhs)
                .HasForeignKey(d => d.Hocvienid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HOSOTHISINH_HOCVIEN");

            entity.HasOne(d => d.Khamsuckhoe).WithMany(p => p.Hosothisinhs)
                .HasForeignKey(d => d.Khamsuckhoeid)
                .HasConstraintName("FK_HOSOTHISINH_PHIEUKHAMSUCKHOE");
        });

        modelBuilder.Entity<Ketquahoctap>(entity =>
        {
            entity.ToTable("KETQUAHOCTAP");

            entity.Property(e => e.Ketquahoctapid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("KETQUAHOCTAPID");
            entity.Property(e => e.Hosoid)
                .HasColumnType("NUMBER")
                .HasColumnName("HOSOID");
            entity.Property(e => e.Nhanxet)
                .HasMaxLength(255)
                .HasColumnName("NHANXET");
            entity.Property(e => e.Sobuoihoc)
                .HasColumnType("NUMBER")
                .HasColumnName("SOBUOIHOC");
            entity.Property(e => e.Sobuoivang)
                .HasColumnType("NUMBER")
                .HasColumnName("SOBUOIVANG");
            entity.Property(e => e.Sokmhoanthanh)
                .HasMaxLength(100)
                .HasColumnName("SOKMHOANTHANH");

            entity.HasOne(d => d.Hoso).WithMany(p => p.Ketquahoctaps)
                .HasForeignKey(d => d.Hosoid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KETQUAHOCTAP_HOSOTHISINH");
        });

        modelBuilder.Entity<Khoahoc>(entity =>
        {
            entity.ToTable("KHOAHOC");

            entity.Property(e => e.Khoahocid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("KHOAHOCID");
            entity.Property(e => e.Diadiem)
                .HasMaxLength(255)
                .HasColumnName("DIADIEM");
            entity.Property(e => e.Hangid)
                .HasColumnType("NUMBER")
                .HasColumnName("HANGID");
            entity.Property(e => e.Ngaybatdau)
                .HasColumnType("DATE")
                .HasColumnName("NGAYBATDAU");
            entity.Property(e => e.Ngayketthuc)
                .HasColumnType("DATE")
                .HasColumnName("NGAYKETTHUC");
            entity.Property(e => e.Tenkhoahoc)
                .HasMaxLength(200)
                .HasColumnName("TENKHOAHOC");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(50)
                .HasColumnName("TRANGTHAI");

            entity.HasOne(d => d.Hang).WithMany(p => p.Khoahocs)
                .HasForeignKey(d => d.Hangid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KHOAHOC_HANGGPLX");
        });

        modelBuilder.Entity<Kythi>(entity =>
        {
            entity.ToTable("KYTHI");

            entity.Property(e => e.Kythiid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("KYTHIID");
            entity.Property(e => e.Loaikythi)
                .HasMaxLength(50)
                .HasColumnName("LOAIKYTHI");
            entity.Property(e => e.Tenkythi)
                .HasMaxLength(100)
                .HasColumnName("TENKYTHI");
        });

        modelBuilder.Entity<Lichthi>(entity =>
        {
            entity.ToTable("LICHTHI");

            entity.Property(e => e.Lichthiid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("LICHTHIID");
            entity.Property(e => e.Diadiem)
                .HasMaxLength(255)
                .HasColumnName("DIADIEM");
            entity.Property(e => e.Kythiid)
                .HasColumnType("NUMBER")
                .HasColumnName("KYTHIID");
            entity.Property(e => e.Thoigianthi)
                .HasPrecision(6)
                .HasColumnName("THOIGIANTHI");

            entity.HasOne(d => d.Kythi).WithMany(p => p.Lichthis)
                .HasForeignKey(d => d.Kythiid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LICHTHI_KYTHI");
        });

        modelBuilder.Entity<Phieukhamsuckhoe>(entity =>
        {
            entity.HasKey(e => e.Khamsuckhoeid);

            entity.ToTable("PHIEUKHAMSUCKHOE");

            entity.Property(e => e.Khamsuckhoeid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("KHAMSUCKHOEID");
            entity.Property(e => e.Cannang)
                .HasColumnType("NUMBER(5,2)")
                .HasColumnName("CANNANG");
            entity.Property(e => e.Chieucao)
                .HasColumnType("NUMBER(5,2)")
                .HasColumnName("CHIEUCAO");
            entity.Property(e => e.Hieuluc)
                .HasMaxLength(50)
                .HasColumnName("HIEULUC");
            entity.Property(e => e.Huyetap)
                .HasMaxLength(50)
                .HasColumnName("HUYETAP");
            entity.Property(e => e.Khammat)
                .HasMaxLength(50)
                .HasColumnName("KHAMMAT");
            entity.Property(e => e.Thoihan)
                .HasColumnType("DATE")
                .HasColumnName("THOIHAN");
            entity.Property(e => e.Urlanh)
                .HasMaxLength(500)
                .HasColumnName("URLANH");
        });

        modelBuilder.Entity<Phieuthanhtoan>(entity =>
        {
            entity.HasKey(e => e.Phieuid);

            entity.ToTable("PHIEUTHANHTOAN");

            entity.Property(e => e.Phieuid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("PHIEUID");
            entity.Property(e => e.Ngaylap)
                .HasColumnType("DATE")
                .HasColumnName("NGAYLAP");
            entity.Property(e => e.Ngaynop)
                .HasColumnType("DATE")
                .HasColumnName("NGAYNOP");
            entity.Property(e => e.Tenphieu)
                .HasMaxLength(100)
                .HasColumnName("TENPHIEU");
            entity.Property(e => e.Tongtien)
                .HasColumnType("NUMBER(18,2)")
                .HasColumnName("TONGTIEN");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Roleid).HasName("PK_ROLE");

            entity.ToTable("Role");

            entity.Property(e => e.Roleid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ROLEID");
            entity.Property(e => e.Mota)
                .HasMaxLength(255)
                .HasColumnName("MOTA");
            entity.Property(e => e.Rolename)
                .HasMaxLength(100)
                .HasColumnName("ROLENAME");
        });

        modelBuilder.Entity<Tinhhuongmophong>(entity =>
        {
            entity.ToTable("TINHHUONGMOPHONG");

            entity.Property(e => e.Tinhhuongmophongid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("TINHHUONGMOPHONGID");
            entity.Property(e => e.Chuongmophongid)
                .HasColumnType("NUMBER")
                .HasColumnName("CHUONGMOPHONGID");
            entity.Property(e => e.Kho)
                .HasColumnType("NUMBER(1)")
                .HasColumnName("KHO");
            entity.Property(e => e.Ngaytao)
                .HasPrecision(3)
                .HasDefaultValueSql("SYSTIMESTAMP ")
                .HasColumnName("NGAYTAO");
            entity.Property(e => e.Tgbatdau).HasColumnName("TGBATDAU");
            entity.Property(e => e.Tgketthuc).HasColumnName("TGKETTHUC");
            entity.Property(e => e.Thutu)
                .HasColumnType("NUMBER")
                .HasColumnName("THUTU");
            entity.Property(e => e.Tieude)
                .HasMaxLength(255)
                .HasColumnName("TIEUDE");
            entity.Property(e => e.Urlanhmeo)
                .HasMaxLength(500)
                .HasColumnName("URLANHMEO");
            entity.Property(e => e.Videourl)
                .HasMaxLength(1000)
                .HasColumnName("VIDEOURL");

            entity.HasOne(d => d.Chuongmophong).WithMany(p => p.Tinhhuongmophongs)
                .HasForeignKey(d => d.Chuongmophongid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TINHHUONGMOPHONG_CHUONGMOPHONG");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("PK_USER");

            entity.ToTable("User");

            entity.Property(e => e.Userid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("USERID");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("1 ")
                .HasColumnType("NUMBER(1)")
                .HasColumnName("ISACTIVE");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Roleid)
                .HasColumnType("NUMBER")
                .HasColumnName("ROLEID");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("USERNAME");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_ROLE");
        });

        modelBuilder.Entity<Yeucaunanghang>(entity =>
        {
            entity.HasKey(e => e.Yeucauid);

            entity.ToTable("YEUCAUNANGHANG");

            entity.Property(e => e.Yeucauid)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("YEUCAUID");
            entity.Property(e => e.Dieukien)
                .HasMaxLength(255)
                .HasColumnName("DIEUKIEN");
            entity.Property(e => e.Gplxid)
                .HasColumnType("NUMBER")
                .HasColumnName("GPLXID");
            entity.Property(e => e.Noidung)
                .HasMaxLength(255)
                .HasColumnName("NOIDUNG");

            entity.HasOne(d => d.Gplx).WithMany(p => p.Yeucaunanghangs)
                .HasForeignKey(d => d.Gplxid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_YEUCAUNANGHANG_GIAYPHEPLAIXE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
