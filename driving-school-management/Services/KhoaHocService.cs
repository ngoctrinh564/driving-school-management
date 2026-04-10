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
        using (var cmd = new OracleCommand("PKG_KHOAHOC.SP_GET_KHOAHOC_DANG_MO", conn))
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
                        TenKhoaHoc = reader["tenKhoaHoc"] == DBNull.Value ? string.Empty : reader["tenKhoaHoc"].ToString()!,
                        NgayBatDau = reader["ngayBatDau"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["ngayBatDau"]),
                        NgayKetThuc = reader["ngayKetThuc"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["ngayKetThuc"]),
                        DiaDiem = reader["diaDiem"] == DBNull.Value ? string.Empty : reader["diaDiem"].ToString()!,
                        TrangThai = reader["trangThai"] == DBNull.Value ? string.Empty : reader["trangThai"].ToString()!,
                        TenHang = reader["tenHang"] == DBNull.Value ? string.Empty : reader["tenHang"].ToString()!,
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
        using (var cmd = new OracleCommand("PKG_KHOAHOC.SP_GET_KHOAHOC_DETAIL", conn))
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
                        HocPhi = GetDecimalValue(reader["hocPhi"])
                    };
                }
            }
        }

        return result;
    }

    public HoSoIndexStatusDto? GetHoSoStatusIndex(int userId)
    {
        HoSoIndexStatusDto? result = null;

        using (var conn = new OracleConnection(_connectionString))
        using (var cmd = new OracleCommand("PKG_KHOAHOC.SP_GET_HOSO_STATUS_INDEX", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = new HoSoIndexStatusDto
                    {
                        TongHoSo = GetInt32Value(reader["tongHoSo"]),
                        TongHoSoConHan = GetInt32Value(reader["tongHoSoConHan"]),
                        TongHoSoDaDuyetConHan = GetInt32Value(reader["tongHoSoDaDuyetConHan"]),
                        TongHoSoDangXuLyConHan = GetInt32Value(reader["tongHoSoDangXuLyConHan"]),
                        TongHoSoHetHan = GetInt32Value(reader["tongHoSoHetHan"]),
                        ShowModal = GetInt32Value(reader["showModal"]),
                        StatusCode = reader["statusCode"] == DBNull.Value ? string.Empty : reader["statusCode"].ToString()!,
                        StatusMessage = reader["statusMessage"] == DBNull.Value ? string.Empty : reader["statusMessage"].ToString()!
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
        using (var cmd = new OracleCommand("PKG_KHOAHOC.SP_CHECK_DANGKY", conn))
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

                        TongHoSo = GetInt32Value(reader["tongHoSo"]),
                        TongHoSoConHan = GetInt32Value(reader["tongHoSoConHan"]),
                        TongHoSoCungHang = GetInt32Value(reader["tongHoSoCungHang"]),
                        TongHoSoDaDuyetConHan = GetInt32Value(reader["tongHoSoDaDuyetConHan"]),
                        TongHoSoDangXuLyConHan = GetInt32Value(reader["tongHoSoDangXuLyConHan"]),
                        TongHoSoBiLoaiConHan = GetInt32Value(reader["tongHoSoBiLoaiConHan"]),
                        TongHoSoHetHan = GetInt32Value(reader["tongHoSoHetHan"]),

                        StatusCode = reader["statusCode"] == DBNull.Value ? string.Empty : reader["statusCode"].ToString()!,
                        StatusMessage = reader["statusMessage"] == DBNull.Value ? string.Empty : reader["statusMessage"].ToString()!,

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
                        TenKhoaHoc = GetStringValue(reader, "tenKhoaHoc"),
                        NgayBatDau = GetNullableDateTimeValue(reader, "ngayBatDau"),
                        NgayKetThuc = GetNullableDateTimeValue(reader, "ngayKetThuc"),
                        DiaDiem = GetStringValue(reader, "diaDiem"),
                        TrangThaiKhoaHocGoc = GetStringValue(reader, "trangThaiKhoaHocGoc"),

                        HangId = GetInt32Value(reader["hangId"]),
                        TenHang = GetStringValue(reader, "tenHang"),
                        LoaiPhuongTien = GetStringValue(reader, "loaiPhuongTien"),
                        HocPhi = GetDecimalValueSafe(reader, "hocPhi"),

                        HoSoId = GetInt32Value(reader["hoSoId"]),
                        TenHoSo = GetStringValue(reader, "tenHoSo"),
                        HocVienId = GetInt32Value(reader["hocVienId"]),
                        HoTenHocVien = GetStringValue(reader, "hoTenHocVien"),

                        PhieuId = GetInt32Value(reader["phieuId"]),
                        TenPhieu = GetStringValue(reader, "tenPhieu"),
                        NgayLap = GetNullableDateTimeValue(reader, "ngayLap"),
                        NgayNop = GetNullableDateTimeValue(reader, "ngayNop"),
                        TongTien = GetDecimalValueSafe(reader, "tongTien"),
                        PhuongThuc = GetStringValue(reader, "phuongThuc"),
                        LoaiPhi = GetStringValue(reader, "loaiPhi"),
                        GhiChu = GetStringValue(reader, "ghiChu"),

                        KetQuaHocTapId = GetInt32Value(reader["ketQuaHocTapId"]),
                        LyThuyetKq = GetNullableInt32ValueSafe(reader, "lyThuyetKq"),
                        SaHinhKq = GetNullableInt32ValueSafe(reader, "saHinhKq"),
                        DuongTruongKq = GetNullableInt32ValueSafe(reader, "duongTruongKq"),
                        MoPhongKq = GetNullableInt32ValueSafe(reader, "moPhongKq"),

                        SoBuoiHoc = GetNullableInt32ValueSafe(reader, "soBuoiHoc"),
                        SoBuoiToiThieu = GetNullableInt32ValueSafe(reader, "soBuoiToiThieu"),
                        KmToiThieu = GetNullableInt32ValueSafe(reader, "kmToiThieu"),
                        SoKmHoanThanh = GetDecimalValueSafe(reader, "soKmHoanThanh"),
                        DuDieuKienThiTotNghiep = GetNullableInt32ValueSafe(reader, "DU_DK_THITOTNGHIEP"),
                        DauTotNghiep = GetNullableInt32ValueSafe(reader, "DAUTOTNGHIEP"),
                        DuDieuKienThiSatHach = GetNullableInt32ValueSafe(reader, "DU_DK_THISATHACH"),
                        ThoiGianCapNhat = GetNullableDateTimeValue(reader, "THOIGIANCAPNHAT"),

                        TrangThaiHocTap = GetStringValue(reader, "trangThaiHocTap"),
                        DaHoanThanh = GetInt32ValueSafe(reader, "daHoanThanh"),
                        DangHoc = GetInt32ValueSafe(reader, "dangHoc"),
                        KhongHoanThanh = GetInt32ValueSafe(reader, "khongHoanThanh")
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
                        TenKhoaHoc = GetStringValue(reader, "tenKhoaHoc"),
                        NgayBatDau = GetNullableDateTimeValue(reader, "ngayBatDau"),
                        NgayKetThuc = GetNullableDateTimeValue(reader, "ngayKetThuc"),
                        DiaDiem = GetStringValue(reader, "diaDiem"),
                        TrangThaiKhoaHocGoc = GetStringValue(reader, "trangThaiKhoaHocGoc"),

                        HangId = GetInt32Value(reader["hangId"]),
                        TenHang = GetStringValue(reader, "tenHang"),
                        MoTa = GetStringValue(reader, "moTa"),
                        LoaiPhuongTien = GetStringValue(reader, "loaiPhuongTien"),
                        SoCauHoi = GetNullableInt32ValueSafe(reader, "soCauHoi"),
                        DiemDat = GetNullableInt32ValueSafe(reader, "diemDat"),
                        ThoiGianTn = GetNullableInt32ValueSafe(reader, "thoiGianTn"),
                        HocPhi = GetDecimalValueSafe(reader, "hocPhi"),

                        HocVienId = GetInt32Value(reader["hocVienId"]),
                        HoTenHocVien = GetStringValue(reader, "hoTenHocVien"),
                        Sdt = GetStringValue(reader, "sdt"),
                        Email = GetStringValue(reader, "email"),

                        UserId = GetInt32Value(reader["userId"]),
                        UserName = GetStringValue(reader, "userName"),
                        IsActive = GetInt32ValueSafe(reader, "isActive"),

                        HoSoId = GetInt32Value(reader["hoSoId"]),
                        TenHoSo = GetStringValue(reader, "tenHoSo"),
                        NgayDangKy = GetNullableDateTimeValue(reader, "ngayDangKy"),
                        TrangThaiHoSo = GetStringValue(reader, "trangThaiHoSo"),
                        GhiChuHoSo = GetStringValue(reader, "ghiChuHoSo"),

                        PhieuId = GetInt32Value(reader["phieuId"]),
                        TenPhieu = GetStringValue(reader, "tenPhieu"),
                        NgayLap = GetNullableDateTimeValue(reader, "ngayLap"),
                        NgayNop = GetNullableDateTimeValue(reader, "ngayNop"),
                        TongTien = GetDecimalValueSafe(reader, "tongTien"),
                        PhuongThuc = GetStringValue(reader, "phuongThuc"),
                        LoaiPhi = GetStringValue(reader, "loaiPhi"),
                        GhiChuThanhToan = GetStringValue(reader, "ghiChuThanhToan"),

                        KetQuaHocTapId = GetInt32Value(reader["ketQuaHocTapId"]),
                        NhanXet = GetStringValue(reader, "nhanXet"),
                        SoBuoiHoc = GetNullableInt32ValueSafe(reader, "soBuoiHoc"),
                        SoBuoiVang = GetNullableInt32ValueSafe(reader, "soBuoiVang"),
                        SoBuoiToiThieu = GetNullableInt32ValueSafe(reader, "soBuoiToiThieu"),
                        KmToiThieu = GetNullableInt32ValueSafe(reader, "kmToiThieu"),
                        SoKmHoanThanh = GetDecimalValueSafe(reader, "soKmHoanThanh"),
                        DuDieuKienThiTotNghiep = GetNullableInt32ValueSafe(reader, "DU_DK_THITOTNGHIEP"),
                        DauTotNghiep = GetNullableInt32ValueSafe(reader, "DAUTOTNGHIEP"),
                        DuDieuKienThiSatHach = GetNullableInt32ValueSafe(reader, "DU_DK_THISATHACH"),
                        ThoiGianCapNhat = GetNullableDateTimeValue(reader, "THOIGIANCAPNHAT"),

                        LyThuyetKq = GetNullableInt32ValueSafe(reader, "lyThuyetKq"),
                        SaHinhKq = GetNullableInt32ValueSafe(reader, "saHinhKq"),
                        DuongTruongKq = GetNullableInt32ValueSafe(reader, "duongTruongKq"),
                        MoPhongKq = GetNullableInt32ValueSafe(reader, "moPhongKq"),

                        TrangThaiHocTap = GetStringValue(reader, "trangThaiHocTap")
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

    private string GetStringValue(IDataRecord reader, string columnName)
    {
        int ordinal = GetOrdinalSafe(reader, columnName);
        if (reader.IsDBNull(ordinal))
            return string.Empty;

        return reader.GetValue(ordinal)?.ToString() ?? string.Empty;
    }

    private DateTime? GetNullableDateTimeValue(IDataRecord reader, string columnName)
    {
        if (!HasColumn(reader, columnName))
            return null;

        int ordinal = GetOrdinalSafe(reader, columnName);
        if (reader.IsDBNull(ordinal))
            return null;

        object value = reader.GetValue(ordinal);

        if (value is DateTime dateTimeValue)
            return dateTimeValue;

        return Convert.ToDateTime(value);
    }

    private int? GetNullableInt32ValueSafe(IDataRecord reader, string columnName)
    {
        if (!HasColumn(reader, columnName))
            return null;

        int ordinal = GetOrdinalSafe(reader, columnName);
        if (reader.IsDBNull(ordinal))
            return null;

        return GetInt32Value(reader.GetValue(ordinal));
    }

    private int GetInt32ValueSafe(IDataRecord reader, string columnName)
    {
        if (!HasColumn(reader, columnName))
            return 0;

        int ordinal = GetOrdinalSafe(reader, columnName);
        if (reader.IsDBNull(ordinal))
            return 0;

        return GetInt32Value(reader.GetValue(ordinal));
    }

    private decimal GetDecimalValueSafe(IDataRecord reader, string columnName)
    {
        if (!HasColumn(reader, columnName))
            return 0;

        int ordinal = GetOrdinalSafe(reader, columnName);
        if (reader.IsDBNull(ordinal))
            return 0;

        return GetDecimalValue(reader.GetValue(ordinal));
    }

    private bool HasColumn(IDataRecord reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private int GetOrdinalSafe(IDataRecord reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        throw new IndexOutOfRangeException($"Không tìm thấy cột {columnName}");
    }
}