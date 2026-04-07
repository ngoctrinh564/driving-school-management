using driving_school_management.Models.DTOs;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace driving_school_management.Services
{
    public class AdminExamService
    {
        private readonly string _connectionString;

        public AdminExamService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDb");
        }

        private static int GetInt32Safe(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            if (value is OracleDecimal oracleDecimal)
                return oracleDecimal.ToInt32();

            if (value is decimal decimalValue)
                return (int)decimalValue;

            if (value is int intValue)
                return intValue;

            if (value is long longValue)
                return (int)longValue;

            if (value is short shortValue)
                return shortValue;

            return int.Parse(value.ToString()!);
        }

        private static int? GetNullableInt32Safe(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            if (value is OracleDecimal oracleDecimal)
                return oracleDecimal.ToInt32();

            if (value is decimal decimalValue)
                return (int)decimalValue;

            if (value is int intValue)
                return intValue;

            if (value is long longValue)
                return (int)longValue;

            if (value is short shortValue)
                return shortValue;

            return int.Parse(value.ToString()!);
        }

        private static DateTime? GetNullableDateTimeSafe(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            if (value is OracleTimeStamp oracleTimeStamp)
                return oracleTimeStamp.Value;

            if (value is DateTime dateTime)
                return dateTime;

            return DateTime.Parse(value.ToString()!);
        }

        public async Task<List<HangGplxDto>> GetHangGplxAsync()
        {
            var list = new List<HangGplxDto>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_KYTHI.GET_HANG_GPLX", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new HangGplxDto
                {
                    HangId = GetInt32Safe(reader["HANGID"]),
                    MaHang = reader["MAHANG"] == DBNull.Value ? null : reader["MAHANG"].ToString(),
                    TenHang = reader["TENHANG"] == DBNull.Value ? null : reader["TENHANG"].ToString()
                });
            }

            return list;
        }

        public async Task CreateKyThiAsync(int hangId, int dot, int nam)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_KYTHI.CREATE_KYTHI", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_hangId", OracleDbType.Int32).Value = hangId;
            cmd.Parameters.Add("p_dot", OracleDbType.Int32).Value = dot;
            cmd.Parameters.Add("p_nam", OracleDbType.Int32).Value = nam;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<KyThiDto>> GetKyThiAsync()
        {
            var list = new List<KyThiDto>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_KYTHI.GET_KYTHI", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new KyThiDto
                {
                    KyThiId = GetInt32Safe(reader["KYTHIID"]),
                    TenKyThi = reader["TENKYTHI"] == DBNull.Value ? null : reader["TENKYTHI"].ToString(),
                    LoaiKyThi = reader["LOAIKYTHI"] == DBNull.Value ? null : reader["LOAIKYTHI"].ToString(),
                    MaHang = reader["MAHANG"] == DBNull.Value ? null : reader["MAHANG"].ToString(),
                    SoLuongDangKy = GetInt32Safe(reader["SOLUONGDANGKY"])
                });
            }

            return list;
        }

        public async Task DeleteKyThiAsync(int kyThiId)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_KYTHI.DELETE_KYTHI", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = kyThiId;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<LichThiDto>> GetLichThiByKyThiAsync(int kyThiId)
        {
            var list = new List<LichThiDto>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_KYTHI.GET_LICHTHI_BY_KYTHI", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = kyThiId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new LichThiDto
                {
                    LichThiId = GetInt32Safe(reader["LICHTHIID"]),
                    KyThiId = GetInt32Safe(reader["KYTHIID"]),
                    DiaDiem = reader["DIADIEM"] == DBNull.Value ? null : reader["DIADIEM"].ToString(),
                    ThoiGianThi = GetNullableDateTimeSafe(reader["THOIGIANTHI"])
                });
            }

            return list;
        }

        public async Task AutoPhanCongLichThiAsync(int kyThiId)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_KYTHI.AUTO_PHANCONG_LICHTHI", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = kyThiId;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> GetHocVienDangKyCountAsync(int kyThiId, string? keyword)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_KYTHI.GET_HOCVIEN_DANGKY_COUNT", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = kyThiId;
            cmd.Parameters.Add("p_keyword", OracleDbType.NVarchar2).Value =
                string.IsNullOrWhiteSpace(keyword) ? DBNull.Value : keyword;
            cmd.Parameters.Add("p_total", OracleDbType.Decimal).Direction = ParameterDirection.Output;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return GetInt32Safe(cmd.Parameters["p_total"].Value);
        }

        public async Task<List<HocVienDangKyThiDto>> GetHocVienDangKyAsync(int kyThiId, string? keyword, int page, int pageSize)
        {
            var list = new List<HocVienDangKyThiDto>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_KYTHI.GET_HOCVIEN_DANGKY", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = kyThiId;
            cmd.Parameters.Add("p_keyword", OracleDbType.NVarchar2).Value =
                string.IsNullOrWhiteSpace(keyword) ? DBNull.Value : keyword;
            cmd.Parameters.Add("p_page", OracleDbType.Int32).Value = page;
            cmd.Parameters.Add("p_pageSize", OracleDbType.Int32).Value = pageSize;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new HocVienDangKyThiDto
                {
                    KyThiId = GetInt32Safe(reader["KYTHIID"]),
                    HoSoId = GetInt32Safe(reader["HOSOID"]),
                    HocVienId = GetInt32Safe(reader["HOCVIENID"]),
                    HoTen = reader["HOTEN"] == DBNull.Value ? null : reader["HOTEN"].ToString(),
                    Sdt = reader["SDT"] == DBNull.Value ? null : reader["SDT"].ToString(),
                    Email = reader["EMAIL"] == DBNull.Value ? null : reader["EMAIL"].ToString(),
                    LichThiId = GetNullableInt32Safe(reader["LICHTHIID"]),
                    PhieuId = GetInt32Safe(reader["PHIEUID"]),
                    TenPhieu = reader["TENPHIEU"] == DBNull.Value ? null : reader["TENPHIEU"].ToString()
                });
            }

            return list;
        }

        public async Task<KyThiEditDto?> GetKyThiEditInfoAsync(int kyThiId)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_KYTHI.GET_KYTHI_EDIT_INFO", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = kyThiId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new KyThiEditDto
                {
                    KyThiId = GetInt32Safe(reader["KYTHIID"]),
                    HangId = GetInt32Safe(reader["HANGID"]),
                    MaHang = reader["MAHANG"] == DBNull.Value ? null : reader["MAHANG"].ToString(),
                    TenHang = reader["TENHANG"] == DBNull.Value ? null : reader["TENHANG"].ToString(),
                    Dot = GetInt32Safe(reader["DOT"]),
                    Nam = GetInt32Safe(reader["NAM"])
                };
            }

            return null;
        }

        public async Task UpdateCapKyThiAsync(int kyThiId, int hangId, int dot, int nam)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_KYTHI.UPDATE_CAP_KYTHI", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = kyThiId;
            cmd.Parameters.Add("p_hangId", OracleDbType.Int32).Value = hangId;
            cmd.Parameters.Add("p_dot", OracleDbType.Int32).Value = dot;
            cmd.Parameters.Add("p_nam", OracleDbType.Int32).Value = nam;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}