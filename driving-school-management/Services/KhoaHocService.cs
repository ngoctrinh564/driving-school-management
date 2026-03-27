using driving_school_management.Models.DTOs;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

public class KhoaHocService
{
    private readonly string _connectionString;

    public KhoaHocService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("OracleDb");
    }

    public List<KhoaHocDto> GetKhoaHocDangMo()
    {
        var result = new List<KhoaHocDto>();

        using (var conn = new OracleConnection(_connectionString))
        using (var cmd = new OracleCommand("sp_GetKhoaHocDangMo", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(new KhoaHocDto
                    {
                        KhoaHocId = GetInt32Value(reader["khoaHocId"]),
                        TenKhoaHoc = reader["tenKhoaHoc"] == DBNull.Value ? string.Empty : reader["tenKhoaHoc"].ToString(),
                        NgayBatDau = reader["ngayBatDau"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["ngayBatDau"]),
                        NgayKetThuc = reader["ngayKetThuc"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["ngayKetThuc"]),
                        DiaDiem = reader["diaDiem"] == DBNull.Value ? string.Empty : reader["diaDiem"].ToString(),
                        TrangThai = reader["trangThai"] == DBNull.Value ? string.Empty : reader["trangThai"].ToString(),
                        TenHang = reader["tenHang"] == DBNull.Value ? string.Empty : reader["tenHang"].ToString(),
                        HocPhi = GetDecimalValue(reader["hocPhi"])
                    });
                }
            }
        }

        return result;
    }

    public KhoaHocDetailDto? GetKhoaHocDetail(int khoaHocId)
    {
        KhoaHocDetailDto? result = null;

        using (var conn = new OracleConnection(_connectionString))
        using (var cmd = new OracleCommand("sp_GetKhoaHocDetail", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_khoaHocId", OracleDbType.Int32).Value = khoaHocId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = new KhoaHocDetailDto
                    {
                        KhoaHocId = GetInt32Value(reader["khoaHocId"]),
                        TenKhoaHoc = reader["tenKhoaHoc"] == DBNull.Value ? string.Empty : reader["tenKhoaHoc"].ToString(),
                        NgayBatDau = reader["ngayBatDau"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["ngayBatDau"]),
                        NgayKetThuc = reader["ngayKetThuc"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["ngayKetThuc"]),
                        DiaDiem = reader["diaDiem"] == DBNull.Value ? string.Empty : reader["diaDiem"].ToString(),
                        TrangThai = reader["trangThai"] == DBNull.Value ? string.Empty : reader["trangThai"].ToString(),

                        HangId = GetInt32Value(reader["hangId"]),
                        TenHang = reader["tenHang"] == DBNull.Value ? string.Empty : reader["tenHang"].ToString(),
                        MoTa = reader["moTa"] == DBNull.Value ? string.Empty : reader["moTa"].ToString(),
                        LoaiPhuongTien = reader["loaiPhuongTien"] == DBNull.Value ? string.Empty : reader["loaiPhuongTien"].ToString(),
                        SoCauHoi = GetNullableInt32Value(reader["soCauHoi"]),
                        DiemDat = GetNullableInt32Value(reader["diemDat"]),
                        ThoiGianTn = GetNullableInt32Value(reader["thoiGianTn"]),
                        HocPhi = GetDecimalValue(reader["hocPhi"])
                    };
                }
            }
        }

        return result;
    }

    public KhoaHocDangKyCheckDto? CheckDangKyKhoaHoc(int userId, int khoaHocId)
    {
        KhoaHocDangKyCheckDto? result = null;

        using (var conn = new OracleConnection(_connectionString))
        using (var cmd = new OracleCommand("SP_KHOAHOC_CHECK_DANGKY", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_khoaHocId", OracleDbType.Int32).Value = khoaHocId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = new KhoaHocDangKyCheckDto
                    {
                        KhoaHocId = GetInt32Value(reader["khoaHocId"]),
                        TenKhoaHoc = reader["tenKhoaHoc"] == DBNull.Value ? string.Empty : reader["tenKhoaHoc"].ToString()!,
                        NgayBatDau = reader["ngayBatDau"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["ngayBatDau"]),
                        NgayKetThuc = reader["ngayKetThuc"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["ngayKetThuc"]),
                        DiaDiem = reader["diaDiem"] == DBNull.Value ? string.Empty : reader["diaDiem"].ToString()!,
                        TrangThai = reader["trangThai"] == DBNull.Value ? string.Empty : reader["trangThai"].ToString()!,

                        HangId = GetInt32Value(reader["hangId"]),
                        TenHang = reader["tenHang"] == DBNull.Value ? string.Empty : reader["tenHang"].ToString()!,
                        MoTa = reader["moTa"] == DBNull.Value ? string.Empty : reader["moTa"].ToString()!,
                        LoaiPhuongTien = reader["loaiPhuongTien"] == DBNull.Value ? string.Empty : reader["loaiPhuongTien"].ToString()!,
                        SoCauHoi = GetNullableInt32Value(reader["soCauHoi"]),
                        DiemDat = GetNullableInt32Value(reader["diemDat"]),
                        ThoiGianTn = GetNullableInt32Value(reader["thoiGianTn"]),
                        HocPhi = GetDecimalValue(reader["hocPhi"]),

                        IsMoDangKy = GetInt32Value(reader["isMoDangKy"]),
                        HasHoSoPhuHop = GetInt32Value(reader["hasHoSoPhuHop"]),
                        HoSoIdPhuHop = GetInt32Value(reader["hoSoIdPhuHop"]),
                        TenHoSoPhuHop = reader["tenHoSoPhuHop"] == DBNull.Value ? string.Empty : reader["tenHoSoPhuHop"].ToString()!,
                        NgayDangKyHoSo = reader["ngayDangKyHoSo"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayDangKyHoSo"]),
                        TrangThaiHoSo = reader["trangThaiHoSo"] == DBNull.Value ? string.Empty : reader["trangThaiHoSo"].ToString()!,
                        DaTungHocHang = GetInt32Value(reader["daTungHocHang"]),
                        SoLanDaHocHang = GetInt32Value(reader["soLanDaHocHang"]),
                        HasHoSoChuaDuyet = GetInt32Value(reader["hasHoSoChuaDuyet"]),

                        BiTrungThoiGianHoc = GetInt32Value(reader["biTrungThoiGianHoc"]),
                        KhoaHocIdTrungThoiGian = GetInt32Value(reader["khoaHocIdTrungThoiGian"]),
                        TenKhoaHocTrungThoiGian = reader["tenKhoaHocTrungThoiGian"] == DBNull.Value ? string.Empty : reader["tenKhoaHocTrungThoiGian"].ToString()!,
                        NgayBatDauTrungThoiGian = reader["ngayBatDauTrungThoiGian"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayBatDauTrungThoiGian"]),
                        NgayKetThucTrungThoiGian = reader["ngayKetThucTrungThoiGian"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayKetThucTrungThoiGian"]),

                        DaTungDangKyCungHang = GetInt32Value(reader["daTungDangKyCungHang"]),
                        KhoaHocIdCungHangGanNhat = GetInt32Value(reader["khoaHocIdCungHangGanNhat"]),
                        TenKhoaHocCungHangGanNhat = reader["tenKhoaHocCungHangGanNhat"] == DBNull.Value ? string.Empty : reader["tenKhoaHocCungHangGanNhat"].ToString()!,

                        CoTheDangKy = GetInt32Value(reader["coTheDangKy"]),
                        DaDangKyChinhKhoaHoc = GetInt32Value(reader["daDangKyChinhKhoaHoc"]),
                        KhoaHocIdDaDangKy = GetInt32Value(reader["khoaHocIdDaDangKy"]),
                        TenKhoaHocDaDangKy = reader["tenKhoaHocDaDangKy"] == DBNull.Value ? string.Empty : reader["tenKhoaHocDaDangKy"].ToString()!
                    };
                }
            }
        }

        return result;
    }

    public List<MyCourseDto> GetMyCourses(int userId)
    {
        var result = new List<MyCourseDto>();

        using (var conn = new OracleConnection(_connectionString))
        using (var cmd = new OracleCommand("SP_MY_COURSES_BY_USER", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(new MyCourseDto
                    {
                        KhoaHocId = GetInt32Value(reader["khoaHocId"]),
                        TenKhoaHoc = reader["tenKhoaHoc"] == DBNull.Value ? string.Empty : reader["tenKhoaHoc"].ToString()!,
                        NgayBatDau = reader["ngayBatDau"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayBatDau"]),
                        NgayKetThuc = reader["ngayKetThuc"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayKetThuc"]),
                        DiaDiem = reader["diaDiem"] == DBNull.Value ? string.Empty : reader["diaDiem"].ToString()!,
                        TrangThaiKhoaHocGoc = reader["trangThaiKhoaHocGoc"] == DBNull.Value ? string.Empty : reader["trangThaiKhoaHocGoc"].ToString()!,

                        HangId = GetInt32Value(reader["hangId"]),
                        TenHang = reader["tenHang"] == DBNull.Value ? string.Empty : reader["tenHang"].ToString()!,
                        LoaiPhuongTien = reader["loaiPhuongTien"] == DBNull.Value ? string.Empty : reader["loaiPhuongTien"].ToString()!,
                        HocPhi = GetDecimalValue(reader["hocPhi"]),

                        HoSoId = GetInt32Value(reader["hoSoId"]),
                        TenHoSo = reader["tenHoSo"] == DBNull.Value ? string.Empty : reader["tenHoSo"].ToString()!,
                        HocVienId = GetInt32Value(reader["hocVienId"]),
                        HoTenHocVien = reader["hoTenHocVien"] == DBNull.Value ? string.Empty : reader["hoTenHocVien"].ToString()!,

                        PhieuId = GetInt32Value(reader["phieuId"]),
                        TenPhieu = reader["tenPhieu"] == DBNull.Value ? string.Empty : reader["tenPhieu"].ToString()!,
                        NgayLap = reader["ngayLap"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayLap"]),
                        NgayNop = reader["ngayNop"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayNop"]),
                        TongTien = GetDecimalValue(reader["tongTien"]),
                        PhuongThuc = reader["phuongThuc"] == DBNull.Value ? string.Empty : reader["phuongThuc"].ToString()!,
                        LoaiPhi = reader["loaiPhi"] == DBNull.Value ? string.Empty : reader["loaiPhi"].ToString()!,
                        GhiChu = reader["ghiChu"] == DBNull.Value ? string.Empty : reader["ghiChu"].ToString()!,

                        KetQuaHocTapId = GetInt32Value(reader["ketQuaHocTapId"]),
                        LyThuyetKq = GetNullableInt32Value(reader["lyThuyetKq"]),
                        SaHinhKq = GetNullableInt32Value(reader["saHinhKq"]),
                        DuongTruongKq = GetNullableInt32Value(reader["duongTruongKq"]),
                        MoPhongKq = GetNullableInt32Value(reader["moPhongKq"]),

                        TrangThaiHocTap = reader["trangThaiHocTap"] == DBNull.Value ? string.Empty : reader["trangThaiHocTap"].ToString()!,
                        DaHoanThanh = GetInt32Value(reader["daHoanThanh"]),
                        DangHoc = GetInt32Value(reader["dangHoc"]),
                        KhongHoanThanh = GetInt32Value(reader["khongHoanThanh"])
                    });
                }
            }
        }

        return result;
    }

    public MyCourseDetailDto? GetMyCourseDetail(int userId, int khoaHocId)
    {
        MyCourseDetailDto? result = null;

        using (var conn = new OracleConnection(_connectionString))
        using (var cmd = new OracleCommand("SP_MY_COURSE_DETAIL", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_khoaHocId", OracleDbType.Int32).Value = khoaHocId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = new MyCourseDetailDto
                    {
                        KhoaHocId = GetInt32Value(reader["khoaHocId"]),
                        TenKhoaHoc = reader["tenKhoaHoc"] == DBNull.Value ? string.Empty : reader["tenKhoaHoc"].ToString()!,
                        NgayBatDau = reader["ngayBatDau"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayBatDau"]),
                        NgayKetThuc = reader["ngayKetThuc"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayKetThuc"]),
                        DiaDiem = reader["diaDiem"] == DBNull.Value ? string.Empty : reader["diaDiem"].ToString()!,
                        TrangThaiKhoaHocGoc = reader["trangThaiKhoaHocGoc"] == DBNull.Value ? string.Empty : reader["trangThaiKhoaHocGoc"].ToString()!,

                        HangId = GetInt32Value(reader["hangId"]),
                        TenHang = reader["tenHang"] == DBNull.Value ? string.Empty : reader["tenHang"].ToString()!,
                        MoTa = reader["moTa"] == DBNull.Value ? string.Empty : reader["moTa"].ToString()!,
                        LoaiPhuongTien = reader["loaiPhuongTien"] == DBNull.Value ? string.Empty : reader["loaiPhuongTien"].ToString()!,
                        SoCauHoi = GetNullableInt32Value(reader["soCauHoi"]),
                        DiemDat = GetNullableInt32Value(reader["diemDat"]),
                        ThoiGianTn = GetNullableInt32Value(reader["thoiGianTn"]),
                        HocPhi = GetDecimalValue(reader["hocPhi"]),

                        HocVienId = GetInt32Value(reader["hocVienId"]),
                        HoTenHocVien = reader["hoTenHocVien"] == DBNull.Value ? string.Empty : reader["hoTenHocVien"].ToString()!,
                        Sdt = reader["sdt"] == DBNull.Value ? string.Empty : reader["sdt"].ToString()!,
                        Email = reader["email"] == DBNull.Value ? string.Empty : reader["email"].ToString()!,

                        UserId = GetInt32Value(reader["userId"]),
                        UserName = reader["userName"] == DBNull.Value ? string.Empty : reader["userName"].ToString()!,
                        IsActive = GetInt32Value(reader["isActive"]),

                        HoSoId = GetInt32Value(reader["hoSoId"]),
                        TenHoSo = reader["tenHoSo"] == DBNull.Value ? string.Empty : reader["tenHoSo"].ToString()!,
                        NgayDangKy = reader["ngayDangKy"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayDangKy"]),
                        TrangThaiHoSo = reader["trangThaiHoSo"] == DBNull.Value ? string.Empty : reader["trangThaiHoSo"].ToString()!,
                        GhiChuHoSo = reader["ghiChuHoSo"] == DBNull.Value ? string.Empty : reader["ghiChuHoSo"].ToString()!,

                        PhieuId = GetInt32Value(reader["phieuId"]),
                        TenPhieu = reader["tenPhieu"] == DBNull.Value ? string.Empty : reader["tenPhieu"].ToString()!,
                        NgayLap = reader["ngayLap"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayLap"]),
                        NgayNop = reader["ngayNop"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayNop"]),
                        TongTien = GetDecimalValue(reader["tongTien"]),
                        PhuongThuc = reader["phuongThuc"] == DBNull.Value ? string.Empty : reader["phuongThuc"].ToString()!,
                        LoaiPhi = reader["loaiPhi"] == DBNull.Value ? string.Empty : reader["loaiPhi"].ToString()!,
                        GhiChuThanhToan = reader["ghiChuThanhToan"] == DBNull.Value ? string.Empty : reader["ghiChuThanhToan"].ToString()!,

                        KetQuaHocTapId = GetInt32Value(reader["ketQuaHocTapId"]),
                        NhanXet = reader["nhanXet"] == DBNull.Value ? string.Empty : reader["nhanXet"].ToString()!,
                        SoBuoiHoc = GetNullableInt32Value(reader["soBuoiHoc"]),
                        SoBuoiVang = GetNullableInt32Value(reader["soBuoiVang"]),
                        SoKmHoanThanh = reader["soKmHoanThanh"] == DBNull.Value ? string.Empty : reader["soKmHoanThanh"].ToString()!,

                        LyThuyetKq = GetNullableInt32Value(reader["lyThuyetKq"]),
                        SaHinhKq = GetNullableInt32Value(reader["saHinhKq"]),
                        DuongTruongKq = GetNullableInt32Value(reader["duongTruongKq"]),
                        MoPhongKq = GetNullableInt32Value(reader["moPhongKq"]),

                        TrangThaiHocTap = reader["trangThaiHocTap"] == DBNull.Value ? string.Empty : reader["trangThaiHocTap"].ToString()!
                    };
                }
            }
        }

        return result;
    }

    public KhoaHocDangKyCheckDto? GetKhoaHocConfirmInfo(int userId, int khoaHocId)
    {
        return CheckDangKyKhoaHoc(userId, khoaHocId);
    }

    private int GetInt32Value(object dbValue)
    {
        if (dbValue == DBNull.Value) return 0;

        if (dbValue is OracleDecimal oracleDecimal)
            return oracleDecimal.ToInt32();

        if (dbValue is decimal decValue)
            return decimal.ToInt32(decValue);

        if (dbValue is int intValue)
            return intValue;

        return int.Parse(dbValue.ToString()!);
    }

    private int GetNullableInt32Value(object dbValue)
    {
        if (dbValue == DBNull.Value) return 0;

        if (dbValue is OracleDecimal oracleDecimal)
            return oracleDecimal.ToInt32();

        if (dbValue is decimal decValue)
            return decimal.ToInt32(decValue);

        if (dbValue is int intValue)
            return intValue;

        return int.Parse(dbValue.ToString()!);
    }

    private decimal GetDecimalValue(object dbValue)
    {
        if (dbValue == DBNull.Value) return 0;

        if (dbValue is OracleDecimal oracleDecimal)
            return oracleDecimal.Value;

        if (dbValue is decimal decValue)
            return decValue;

        return decimal.Parse(dbValue.ToString()!);
    }
}