using driving_school_management.Models.DTOs;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Globalization;

namespace driving_school_management.Services
{
    public class ExamService
    {
        private readonly string _connectionString;

        public ExamService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDb");
        }

        private static object GetOracleValueSafe(OracleDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return DBNull.Value;

            return reader.GetOracleValue(ordinal);
        }

        private static string? GetStringSafe(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            if (value is OracleString os)
                return os.IsNull ? null : os.Value;

            if (value is OracleClob clob)
                return clob.IsNull ? null : clob.Value;

            return value.ToString();
        }

        private static int GetInt32Safe(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            if (value is OracleDecimal od)
            {
                if (od.IsNull)
                    return 0;

                var raw = od.ToString();
                if (string.IsNullOrWhiteSpace(raw))
                    return 0;

                if (int.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out int iv))
                    return iv;

                if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out double dv))
                    return Convert.ToInt32(dv);

                throw new InvalidCastException($"Cannot convert OracleDecimal '{raw}' to Int32.");
            }

            if (value is OracleString os)
            {
                if (os.IsNull || string.IsNullOrWhiteSpace(os.Value))
                    return 0;

                if (int.TryParse(os.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out int iv))
                    return iv;

                if (double.TryParse(os.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double dv))
                    return Convert.ToInt32(dv);

                throw new InvalidCastException($"Cannot convert OracleString '{os.Value}' to Int32.");
            }

            if (value is decimal dec) return (int)dec;
            if (value is int i) return i;
            if (value is long l) return (int)l;
            if (value is short s) return s;
            if (value is byte b) return b;
            if (value is bool bo) return bo ? 1 : 0;

            var text = value.ToString();
            if (int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
                return result;

            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double dresult))
                return Convert.ToInt32(dresult);

            throw new InvalidCastException($"Cannot convert value '{text}' ({value.GetType().FullName}) to Int32.");
        }

        private static decimal GetDecimalSafe(object value, string columnName)
        {
            if (value == null || value == DBNull.Value)
                return 0m;

            string raw;

            if (value is OracleDecimal od)
            {
                if (od.IsNull)
                    return 0m;

                raw = od.ToString();
            }
            else if (value is OracleString os)
            {
                if (os.IsNull || string.IsNullOrWhiteSpace(os.Value))
                    return 0m;

                raw = os.Value;
            }
            else if (value is decimal dec)
            {
                return dec;
            }
            else if (value is int i)
            {
                return i;
            }
            else if (value is long l)
            {
                return l;
            }
            else if (value is short s)
            {
                return s;
            }
            else if (value is byte b)
            {
                return b;
            }
            else if (value is double db)
            {
                if (db > (double)decimal.MaxValue || db < (double)decimal.MinValue)
                    throw new OverflowException($"Column {columnName}: value '{db}' is outside decimal range.");

                return Convert.ToDecimal(db);
            }
            else if (value is float f)
            {
                if (f > (float)decimal.MaxValue || f < (float)decimal.MinValue)
                    throw new OverflowException($"Column {columnName}: value '{f}' is outside decimal range.");

                return Convert.ToDecimal(f);
            }
            else
            {
                raw = value.ToString() ?? "0";
            }

            if (string.IsNullOrWhiteSpace(raw))
                return 0m;

            raw = raw.Replace(',', '.');

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decResult))
                return decResult;

            if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out double dblResult))
            {
                if (double.IsNaN(dblResult) || double.IsInfinity(dblResult))
                    throw new OverflowException($"Column {columnName}: value '{raw}' is not a finite number.");

                if (Math.Abs(dblResult) < 1e-28)
                    return 0m;

                if (dblResult > (double)decimal.MaxValue || dblResult < (double)decimal.MinValue)
                    throw new OverflowException($"Column {columnName}: value '{raw}' is outside decimal range.");

                return Convert.ToDecimal(dblResult);
            }

            throw new InvalidCastException($"Column {columnName}: cannot convert value '{raw}' to Decimal.");
        }

        public async Task<List<UserExamDto>> GetKyThiForUserAsync(int userId)
        {
            var list = new List<UserExamDto>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_KYTHI.GET_KYTHI_FOR_USER", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add(new OracleParameter("p_cursor", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.Output
            });

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new UserExamDto
                {
                    KyThiId = GetInt32Safe(GetOracleValueSafe(reader, "KYTHIID")),
                    TenKyThi = GetStringSafe(GetOracleValueSafe(reader, "TENKYTHI")),
                    LoaiKyThi = GetStringSafe(GetOracleValueSafe(reader, "LOAIKYTHI")),
                    HoSoId = GetInt32Safe(GetOracleValueSafe(reader, "HOSOID")),
                    HangId = GetInt32Safe(GetOracleValueSafe(reader, "HANGID")),
                    MaHang = GetStringSafe(GetOracleValueSafe(reader, "MAHANG")),
                    TenHang = GetStringSafe(GetOracleValueSafe(reader, "TENHANG")),
                    HocPhi = GetDecimalSafe(GetOracleValueSafe(reader, "HOCPHI"), "HOCPHI"),
                    DuDieuKienThiTotNghiep = GetInt32Safe(GetOracleValueSafe(reader, "DUDKTOTNGHIEP")) == 1,
                    DuDieuKienThiSatHach = GetInt32Safe(GetOracleValueSafe(reader, "DUDKSATHACH")) == 1,
                    DauTotNghiep = GetInt32Safe(GetOracleValueSafe(reader, "DAUTOTNGHIEP")) == 1,
                    DaHoanThanh = GetInt32Safe(GetOracleValueSafe(reader, "DAHOANTHANH")) == 1,
                    CoTheDangKy = GetInt32Safe(GetOracleValueSafe(reader, "COTHEDANGKY")) == 1,
                    SoKyThiDangKy = GetInt32Safe(GetOracleValueSafe(reader, "SOKYTHIDANGKY")),
                    TongPhiDuKien = GetDecimalSafe(GetOracleValueSafe(reader, "TONGPHIDUKIEN"), "TONGPHIDUKIEN"),
                    CanhBao = GetStringSafe(GetOracleValueSafe(reader, "CANHBAO"))
                });
            }

            return list;
        }

        public async Task<UserExamConfirmDto?> GetConfirmDangKyInfoAsync(int userId, int kyThiId)
        {
            var result = new UserExamConfirmDto();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_KYTHI.GET_CONFIRM_DANGKY_INFO", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = kyThiId;
            cmd.Parameters.Add(new OracleParameter("p_cursor", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.Output
            });

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            bool hasData = false;

            while (await reader.ReadAsync())
            {
                if (!hasData)
                {
                    result.HoTen = GetStringSafe(GetOracleValueSafe(reader, "HOTEN"));
                    result.HoSoId = GetInt32Safe(GetOracleValueSafe(reader, "HOSOID"));
                    result.HangId = GetInt32Safe(GetOracleValueSafe(reader, "HANGID"));
                    result.MaHang = GetStringSafe(GetOracleValueSafe(reader, "MAHANG"));
                    result.TenHang = GetStringSafe(GetOracleValueSafe(reader, "TENHANG"));
                    result.HocPhi = GetDecimalSafe(GetOracleValueSafe(reader, "HOCPHI"), "HOCPHI");
                    result.LePhiMotKy = GetDecimalSafe(GetOracleValueSafe(reader, "LEPHIMOTKY"), "LEPHIMOTKY");
                    result.TongPhi = GetDecimalSafe(GetOracleValueSafe(reader, "TONGPHI"), "TONGPHI");
                    result.DuDieuKienThiTotNghiep = GetInt32Safe(GetOracleValueSafe(reader, "DUDKTOTNGHIEP")) == 1;
                    result.DuDieuKienThiSatHach = GetInt32Safe(GetOracleValueSafe(reader, "DUDKSATHACH")) == 1;
                    result.DauTotNghiep = GetInt32Safe(GetOracleValueSafe(reader, "DAUTOTNGHIEP")) == 1;
                    result.DaHoanThanh = GetInt32Safe(GetOracleValueSafe(reader, "DAHOANTHANH")) == 1;
                    hasData = true;
                }

                result.DanhSachKyThi.Add(new UserExamConfirmItemDto
                {
                    ThuTu = GetInt32Safe(GetOracleValueSafe(reader, "THUTU")),
                    KyThiId = GetInt32Safe(GetOracleValueSafe(reader, "KYTHIID")),
                    TenKyThi = GetStringSafe(GetOracleValueSafe(reader, "TENKYTHI")),
                    LoaiKyThi = GetStringSafe(GetOracleValueSafe(reader, "LOAIKYTHI")),
                    GhiChu = GetStringSafe(GetOracleValueSafe(reader, "GHICHU"))
                });
            }

            return hasData ? result : null;
        }

        public async Task DangKyKyThiUserAsync(int userId, int kyThiId)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_KYTHI.DANGKY_KYTHI_USER", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = kyThiId;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}