CREATE DATABASE QuanLyGPLX;
GO

USE QuanLyGPLX;
GO

/* =========================================================
   1. QUẢN TRỊ HỆ THỐNG
   Ghi chú:
   - Dùng PascalCase cho tên bảng.
   - Dùng camelCase cho tên cột.
   - Dùng [User] và [Role] vì đây là từ dễ xung đột keyword.
========================================================= */

CREATE TABLE [Role] (
    roleId INT IDENTITY(1,1) NOT NULL,
    roleName NVARCHAR(100) NOT NULL,
    moTa NVARCHAR(255) NULL,

    CONSTRAINT pk_Role PRIMARY KEY (roleId)
);

CREATE TABLE [User] (
    userId INT IDENTITY(1,1) NOT NULL,
    roleId INT NOT NULL,
    userName NVARCHAR(100) NOT NULL,
    [password] NVARCHAR(255) NOT NULL,
    isActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT pk_User PRIMARY KEY (userId),
    CONSTRAINT fk_User_Role
        FOREIGN KEY (roleId) REFERENCES [Role](roleId)

    -- Ghi chú:
    -- Có thể thêm UNIQUE cho userName nếu mỗi tài khoản là duy nhất.
);

CREATE TABLE HocVien (
    hocVienId INT IDENTITY(1,1) NOT NULL,
    hoTen NVARCHAR(100) NOT NULL,
    soCmndCccd NVARCHAR(20) NOT NULL,
    namSinh DATE NULL,
    gioiTinh NVARCHAR(10) NULL,
    sdt NVARCHAR(15) NULL,
    email NVARCHAR(50) NULL,
    avatarUrl NVARCHAR(500) NULL,
    userId INT NULL,

    CONSTRAINT pk_HocVien PRIMARY KEY (hocVienId),
    CONSTRAINT fk_HocVien_User
        FOREIGN KEY (userId) REFERENCES [User](userId)

    -- Ghi chú:
    -- soCmndCccd thường nên UNIQUE.
    -- email cũng có thể thêm UNIQUE nếu cần.
);

/* =========================================================
   2. HỒ SƠ - KHÁM SỨC KHỎE - HẠNG GPLX - THANH TOÁN
========================================================= */

CREATE TABLE PhieuKhamSucKhoe (
    khamSucKhoeId INT IDENTITY(1,1) NOT NULL,
    hieuLuc NVARCHAR(50) NULL,
    thoiHan DATE NULL,
    khamMat NVARCHAR(50) NULL,
    huyetAp NVARCHAR(50) NULL,
    chieuCao DECIMAL(5,2) NULL,
    canNang DECIMAL(5,2) NULL,
    urlAnh NVARCHAR(500) NULL,

    CONSTRAINT pk_PhieuKhamSucKhoe PRIMARY KEY (khamSucKhoeId)

    -- Ghi chú:
    -- hieuLuc hiện đang lưu text ("6 tháng", "1 năm"), phù hợp hiển thị.
    -- Nếu cần xử lý nghiệp vụ mạnh hơn thì chỉ nên giữ thoiHan.
);

CREATE TABLE HangGplx (
    hangId INT IDENTITY(1,1) NOT NULL,
	maHang NCHAR(10) NOT NULL,
    tenHang NVARCHAR(20) NOT NULL,
    moTa NVARCHAR(MAX) NULL,
    loaiPhuongTien NVARCHAR(100) NULL,
	soCauHoi INT NOT NULL DEFAULT 0,
	diemDat INT NOT NULL DEFAULT 0,
	thoiGianTn INT NOT NULL,
    thoiHanLyThuyet INT NULL,
    thoiHanThucHanh INT NULL,
    hocPhi DECIMAL(18,2) NULL,

    CONSTRAINT pk_HangGplx PRIMARY KEY (hangId)

    -- Ghi chú:
    -- hocPhi nên >= 0 nếu muốn thêm CHECK constraint.
);

CREATE TABLE HoSoThiSinh (
    hoSoId INT IDENTITY(1,1) NOT NULL,
    hocVienId INT NOT NULL,
    tenHoSo NVARCHAR(100) NOT NULL,
    loaiHoSo NVARCHAR(50) NULL,
    ngayDangKy DATE NULL,
    trangThai NVARCHAR(50) NULL,
    ghiChu NVARCHAR(255) NULL,
    khamSucKhoeId INT NULL,
    hangId INT NOT NULL,

    CONSTRAINT pk_HoSoThiSinh PRIMARY KEY (hoSoId),
    CONSTRAINT fk_HoSoThiSinh_HocVien
        FOREIGN KEY (hocVienId) REFERENCES HocVien(hocVienId),
    CONSTRAINT fk_HoSoThiSinh_PhieuKhamSucKhoe
        FOREIGN KEY (khamSucKhoeId) REFERENCES PhieuKhamSucKhoe(khamSucKhoeId),
    CONSTRAINT fk_HoSoThiSinh_HangGplx
        FOREIGN KEY (hangId) REFERENCES HangGplx(hangId)
);

CREATE TABLE PhieuThanhToan (
    phieuId INT IDENTITY(1,1) NOT NULL,
    tenPhieu NVARCHAR(100) NOT NULL,
    ngayLap DATE NULL,
    tongTien DECIMAL(18,2) NULL,
    ngayNop DATE NULL,

    CONSTRAINT pk_PhieuThanhToan PRIMARY KEY (phieuId)

    -- Ghi chú:
    -- tongTien nên >= 0 nếu muốn thêm CHECK constraint.
);

CREATE TABLE ChiTietPhieuThanhToan (
    hoSoId INT NOT NULL,
    phieuId INT NOT NULL,
    loaiPhi NVARCHAR(100) NULL,
    ghiChu NVARCHAR(255) NULL,

    CONSTRAINT pk_ChiTietPhieuThanhToan PRIMARY KEY (hoSoId, phieuId),
    CONSTRAINT fk_ChiTietPhieuThanhToan_HoSoThiSinh
        FOREIGN KEY (hoSoId) REFERENCES HoSoThiSinh(hoSoId),
    CONSTRAINT fk_ChiTietPhieuThanhToan_PhieuThanhToan
        FOREIGN KEY (phieuId) REFERENCES PhieuThanhToan(phieuId)
);

CREATE TABLE GiayPhepLaiXe (
    gplxId INT IDENTITY(1,1) NOT NULL,
    ngayCap DATE NULL,
    ngayHetHan DATE NULL,
    trangThai NVARCHAR(50) NULL,
    hoSoId INT NOT NULL,

    CONSTRAINT pk_GiayPhepLaiXe PRIMARY KEY (gplxId),
    CONSTRAINT fk_GiayPhepLaiXe_HoSoThiSinh
        FOREIGN KEY (hoSoId) REFERENCES HoSoThiSinh(hoSoId)
);

CREATE TABLE ChiTietGplx (
    hangId INT NOT NULL,
    gplxId INT NOT NULL,
    ngayCapChiTiet DATE NULL,

    CONSTRAINT pk_ChiTietGplx PRIMARY KEY (hangId, gplxId),
    CONSTRAINT fk_ChiTietGplx_HangGplx
        FOREIGN KEY (hangId) REFERENCES HangGplx(hangId),
    CONSTRAINT fk_ChiTietGplx_GiayPhepLaiXe
        FOREIGN KEY (gplxId) REFERENCES GiayPhepLaiXe(gplxId)
);

CREATE TABLE YeuCauNangHang (
    yeuCauId INT IDENTITY(1,1) NOT NULL,
    noiDung NVARCHAR(255) NULL,
    dieuKien NVARCHAR(255) NULL,
    gplxId INT NOT NULL,

    CONSTRAINT pk_YeuCauNangHang PRIMARY KEY (yeuCauId),
    CONSTRAINT fk_YeuCauNangHang_GiayPhepLaiXe
        FOREIGN KEY (gplxId) REFERENCES GiayPhepLaiXe(gplxId)
);

CREATE TABLE AnhGksk (
    anhId INT IDENTITY(1,1) NOT NULL,
    khamSucKhoeId INT NOT NULL,
    urlAnh NVARCHAR(300) NOT NULL,

    CONSTRAINT pk_AnhGksk PRIMARY KEY (anhId),
    CONSTRAINT fk_AnhGksk_PhieuKhamSucKhoe
        FOREIGN KEY (khamSucKhoeId) REFERENCES PhieuKhamSucKhoe(khamSucKhoeId)
);

/* =========================================================
   3. KHÓA HỌC - KẾT QUẢ HỌC TẬP
========================================================= */

CREATE TABLE KhoaHoc (
    khoaHocId INT IDENTITY(1,1) NOT NULL,
    hangId INT NOT NULL,
    tenKhoaHoc NVARCHAR(200) NULL,
    ngayBatDau DATE NULL,
    ngayKetThuc DATE NULL,
    diaDiem NVARCHAR(255) NULL,
    trangThai NVARCHAR(50) NULL,

    CONSTRAINT pk_KhoaHoc PRIMARY KEY (khoaHocId),
    CONSTRAINT fk_KhoaHoc_HangGplx
        FOREIGN KEY (hangId) REFERENCES HangGplx(hangId)
);

CREATE TABLE KetQuaHocTap (
    ketQuaHocTapId INT IDENTITY(1,1) NOT NULL,
    hoSoId INT NOT NULL,
    nhanXet NVARCHAR(255) NULL,
    soBuoiHoc INT NULL,
    soBuoiVang INT NULL,
    soKmHoanThanh NVARCHAR(100) NULL,

    CONSTRAINT pk_KetQuaHocTap PRIMARY KEY (ketQuaHocTapId),
    CONSTRAINT fk_KetQuaHocTap_HoSoThiSinh
        FOREIGN KEY (hoSoId) REFERENCES HoSoThiSinh(hoSoId)
);
GO

CREATE TABLE ChiTietKetQuaHocTap (
    ketQuaHocTapId INT NOT NULL,
    khoaHocId INT NOT NULL,
    lyThuyetKq BIT NULL,
    saHinhKq BIT NULL,
    duongTruongKq BIT NULL,
    moPhongKq BIT NULL,

    CONSTRAINT pk_ChiTietKetQuaHocTap PRIMARY KEY (ketQuaHocTapId),
    CONSTRAINT fk_ChiTietKetQuaHocTap_KetQuaHocTap
        FOREIGN KEY (ketQuaHocTapId) REFERENCES KetQuaHocTap(ketQuaHocTapId),
    CONSTRAINT fk_ChiTietKetQuaHocTap_KhoaHoc
        FOREIGN KEY (khoaHocId) REFERENCES KhoaHoc(khoaHocId)
);
GO

/* =========================================================
   4. KỲ THI - LỊCH THI - BÀI THI - GIÁM SÁT - KẾT QUẢ THI
========================================================= */

CREATE TABLE KyThi (
    kyThiId INT IDENTITY(1,1) NOT NULL,
    tenKyThi NVARCHAR(100) NOT NULL,
    loaiKyThi NVARCHAR(50) NULL,

    CONSTRAINT pk_KyThi PRIMARY KEY (kyThiId)
);

CREATE TABLE LichThi (
    lichThiId INT IDENTITY(1,1) NOT NULL,
    thoiGianThi DATETIME NULL,
    diaDiem NVARCHAR(255) NULL,
    kyThiId INT NOT NULL,

    CONSTRAINT pk_LichThi PRIMARY KEY (lichThiId),
    CONSTRAINT fk_LichThi_KyThi
        FOREIGN KEY (kyThiId) REFERENCES KyThi(kyThiId)
);

CREATE TABLE ChiTietDangKyThi (
    kyThiId INT NOT NULL,
    hoSoId INT NOT NULL,
    thoiGianDangKy DATETIME NULL,

    CONSTRAINT pk_ChiTietDangKyThi PRIMARY KEY (kyThiId, hoSoId),
    CONSTRAINT fk_ChiTietDangKyThi_KyThi
        FOREIGN KEY (kyThiId) REFERENCES KyThi(kyThiId),
    CONSTRAINT fk_ChiTietDangKyThi_HoSoThiSinh
        FOREIGN KEY (hoSoId) REFERENCES HoSoThiSinh(hoSoId)
);

CREATE TABLE BaiThi (
    baiThiId INT IDENTITY(1,1) NOT NULL,
    tenBaiThi NVARCHAR(100) NOT NULL,
    moTa NVARCHAR(255) NULL,
    loaiBaiThi NVARCHAR(50) NULL,
    kyThiId INT NOT NULL,

    CONSTRAINT pk_BaiThi PRIMARY KEY (baiThiId),
    CONSTRAINT fk_BaiThi_KyThi
        FOREIGN KEY (kyThiId) REFERENCES KyThi(kyThiId)
);

CREATE TABLE CanBoGiamSat (
    canBoId INT IDENTITY(1,1) NOT NULL,
    hoTen NVARCHAR(100) NOT NULL,
    ngaySinh DATE NULL,
    gioiTinh NVARCHAR(10) NULL,
    diaChi NVARCHAR(255) NULL,
    email NVARCHAR(100) NULL,
    sdt NVARCHAR(15) NULL,

    CONSTRAINT pk_CanBoGiamSat PRIMARY KEY (canBoId)
);

CREATE TABLE ChiTietPhanCongGiamSat (
    baiThiId INT NOT NULL,
    canBoId INT NOT NULL,
    thoiGianBatDau DATETIME NULL,
    thoiGianKetThuc DATETIME NULL,
    phongThi NVARCHAR(100) NULL,
    ghiChu NVARCHAR(255) NULL,

    CONSTRAINT pk_ChiTietPhanCongGiamSat PRIMARY KEY (baiThiId, canBoId),
    CONSTRAINT fk_ChiTietPhanCongGiamSat_BaiThi
        FOREIGN KEY (baiThiId) REFERENCES BaiThi(baiThiId),
    CONSTRAINT fk_ChiTietPhanCongGiamSat_CanBoGiamSat
        FOREIGN KEY (canBoId) REFERENCES CanBoGiamSat(canBoId)
);

CREATE TABLE ChiTietKetQuaThi (
    baiThiId INT NOT NULL,
    hoSoId INT NOT NULL,
    ketQuaDatDuoc NVARCHAR(50) NULL,
    tongDiem FLOAT NULL,

    CONSTRAINT pk_ChiTietKetQuaThi PRIMARY KEY (baiThiId, hoSoId),
    CONSTRAINT fk_ChiTietKetQuaThi_BaiThi
        FOREIGN KEY (baiThiId) REFERENCES BaiThi(baiThiId),
    CONSTRAINT fk_ChiTietKetQuaThi_HoSoThiSinh
        FOREIGN KEY (hoSoId) REFERENCES HoSoThiSinh(hoSoId)
);

/* =========================================================
   5. LÝ THUYẾT
========================================================= */

CREATE TABLE Chuong (
    chuongId INT IDENTITY(1,1) NOT NULL,
    tenChuong NVARCHAR(255) NOT NULL,
    thuTu INT NULL,

    CONSTRAINT pk_Chuong PRIMARY KEY (chuongId)
);

CREATE TABLE CauHoiLyThuyet (
    cauHoiId INT IDENTITY(1,1) NOT NULL,
    chuongId INT NOT NULL,
    noiDung NVARCHAR(MAX) NULL,
    hinhAnh NVARCHAR(500) NULL,
    cauLiet BIT NULL,
    chuY BIT NULL,
    xeMay BIT NULL,
    urlAnhMeo NVARCHAR(500) NULL,

    CONSTRAINT pk_CauHoiLyThuyet PRIMARY KEY (cauHoiId),
    CONSTRAINT fk_CauHoiLyThuyet_Chuong
        FOREIGN KEY (chuongId) REFERENCES Chuong(chuongId)
);

CREATE TABLE DapAn (
    dapAnId INT IDENTITY(1,1) NOT NULL,
    cauHoiId INT NOT NULL,
    noiDung NVARCHAR(MAX) NULL,
    dapAnDung BIT NOT NULL DEFAULT 0,
    thuTu INT NOT NULL DEFAULT 0,

    CONSTRAINT pk_DapAn PRIMARY KEY (dapAnId),
    CONSTRAINT fk_DapAn_CauHoiLyThuyet
        FOREIGN KEY (cauHoiId) REFERENCES CauHoiLyThuyet(cauHoiId)
);

/* =========================================================
   6. THI THỬ LÝ THUYẾT
========================================================= */

CREATE TABLE BoDeThiThu (
    boDeId INT IDENTITY(1,1) NOT NULL,
    tenBoDe NVARCHAR(255) NULL,
    thoiGian INT NULL,
    soCauHoi INT NULL,
    hoatDong BIT NOT NULL DEFAULT 1,
    taoLuc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    hangId INT NOT NULL,

    CONSTRAINT pk_BoDeThiThu PRIMARY KEY (boDeId),
    CONSTRAINT fk_BoDeThiThu_HangGplx
        FOREIGN KEY (hangId) REFERENCES HangGplx(hangId)
);

CREATE TABLE ChiTietBoDeTracNghiem (
    boDeId INT NOT NULL,
    cauHoiId INT NOT NULL,
    thuTu INT NOT NULL DEFAULT 0,

    CONSTRAINT pk_ChiTietBoDeTracNghiem PRIMARY KEY (boDeId, cauHoiId),
    CONSTRAINT fk_ChiTietBoDeTracNghiem_BoDeThiThu
        FOREIGN KEY (boDeId) REFERENCES BoDeThiThu(boDeId),
    CONSTRAINT fk_ChiTietBoDeTracNghiem_CauHoiLyThuyet
        FOREIGN KEY (cauHoiId) REFERENCES CauHoiLyThuyet(cauHoiId)

    -- Ghi chú:
    -- Đổi tên từ chiTietBoDeTN sang ChiTietBoDeTracNghiem để rõ nghĩa hơn.
);

CREATE TABLE BaiLam (
    baiLamId INT IDENTITY(1,1) NOT NULL,
    thoiGianLamBai INT NULL,
    soCauSai INT NOT NULL DEFAULT 0,
    ketQua BIT NULL,
    userId INT NOT NULL,
    boDeId INT NOT NULL,

    CONSTRAINT pk_BaiLam PRIMARY KEY (baiLamId),
    CONSTRAINT fk_BaiLam_User
        FOREIGN KEY (userId) REFERENCES [User](userId),
    CONSTRAINT fk_BaiLam_BoDeThiThu
        FOREIGN KEY (boDeId) REFERENCES BoDeThiThu(boDeId)
);

CREATE TABLE ChiTietBaiLam (
    baiLamId INT NOT NULL,
    cauHoiId INT NOT NULL,
    dapAnDaChon NVARCHAR(50) NULL,
    ketQuaCau BIT NULL,

    CONSTRAINT pk_ChiTietBaiLam PRIMARY KEY (baiLamId, cauHoiId),
    CONSTRAINT fk_ChiTietBaiLam_BaiLam
        FOREIGN KEY (baiLamId) REFERENCES BaiLam(baiLamId),
    CONSTRAINT fk_ChiTietBaiLam_CauHoiLyThuyet
        FOREIGN KEY (cauHoiId) REFERENCES CauHoiLyThuyet(cauHoiId)
);

/* =========================================================
   7. MÔ PHỎNG
========================================================= */

CREATE TABLE ChuongMoPhong (
    chuongMoPhongId INT IDENTITY(1,1) NOT NULL,
    tenChuong NVARCHAR(255) NOT NULL,
    thuTu INT NULL,

    CONSTRAINT pk_ChuongMoPhong PRIMARY KEY (chuongMoPhongId)
);

CREATE TABLE TinhHuongMoPhong (
    tinhHuongMoPhongId INT IDENTITY(1,1) NOT NULL,
    chuongMoPhongId INT NOT NULL,
    tieuDe NVARCHAR(255) NULL,
    videoUrl NVARCHAR(1000) NULL,
    thuTu INT NULL,
    kho BIT NULL,
    tgBatDau FLOAT NULL,
    tgKetThuc FLOAT NULL,
    ngayTao DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    urlAnhMeo NVARCHAR(500) NULL,

    CONSTRAINT pk_TinhHuongMoPhong PRIMARY KEY (tinhHuongMoPhongId),
    CONSTRAINT fk_TinhHuongMoPhong_ChuongMoPhong
        FOREIGN KEY (chuongMoPhongId) REFERENCES ChuongMoPhong(chuongMoPhongId)
);

CREATE TABLE BoDeMoPhong (
    boDeMoPhongId INT IDENTITY(1,1) NOT NULL,
    tenBoDe NVARCHAR(255) NULL,
    soTinhHuong INT NULL,
    taoLuc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    isActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT pk_BoDeMoPhong PRIMARY KEY (boDeMoPhongId)
);

CREATE TABLE ChiTietBoDeMoPhong (
    boDeMoPhongId INT NOT NULL,
    tinhHuongMoPhongId INT NOT NULL,
    thuTu INT NULL,

    CONSTRAINT pk_ChiTietBoDeMoPhong PRIMARY KEY (boDeMoPhongId, tinhHuongMoPhongId),
    CONSTRAINT fk_ChiTietBoDeMoPhong_BoDeMoPhong
        FOREIGN KEY (boDeMoPhongId) REFERENCES BoDeMoPhong(boDeMoPhongId),
    CONSTRAINT fk_ChiTietBoDeMoPhong_TinhHuongMoPhong
        FOREIGN KEY (tinhHuongMoPhongId) REFERENCES TinhHuongMoPhong(tinhHuongMoPhongId)
);

CREATE TABLE BaiLamMoPhong (
    baiLamMoPhongId INT IDENTITY(1,1) NOT NULL,
    tongDiem INT NOT NULL DEFAULT 0,
    ketQua BIT NULL,
    userId INT NOT NULL,
    boDeMoPhongId INT NOT NULL,

    CONSTRAINT pk_BaiLamMoPhong PRIMARY KEY (baiLamMoPhongId),
    CONSTRAINT fk_BaiLamMoPhong_User
        FOREIGN KEY (userId) REFERENCES [User](userId),
    CONSTRAINT fk_BaiLamMoPhong_BoDeMoPhong
        FOREIGN KEY (boDeMoPhongId) REFERENCES BoDeMoPhong(boDeMoPhongId)

    -- Ghi chú:
    -- Đổi tên idBaiLamTongDiem -> baiLamMoPhongId để tên khóa chính tự nhiên hơn.
);

CREATE TABLE DiemTungTinhHuong (
    baiLamMoPhongId INT NOT NULL,
    tinhHuongMoPhongId INT NOT NULL,
    thoiDiemNguoiDungNhan FLOAT NOT NULL,

    CONSTRAINT pk_DiemTungTinhHuong PRIMARY KEY (baiLamMoPhongId, tinhHuongMoPhongId),
    CONSTRAINT fk_DiemTungTinhHuong_BaiLamMoPhong
        FOREIGN KEY (baiLamMoPhongId) REFERENCES BaiLamMoPhong(baiLamMoPhongId),
    CONSTRAINT fk_DiemTungTinhHuong_TinhHuongMoPhong
        FOREIGN KEY (tinhHuongMoPhongId) REFERENCES TinhHuongMoPhong(tinhHuongMoPhongId)
);
GO