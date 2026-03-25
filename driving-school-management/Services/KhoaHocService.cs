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