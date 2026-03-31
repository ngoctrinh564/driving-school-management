using driving_school_management.Models;
using driving_school_management.ViewModels;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace driving_school_management.Services
{
    public class AdminKhoaHocService
    {
        private readonly string _connectionString;

        public AdminKhoaHocService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDb");
        }

        public (List<KhoaHocVM> Data, int Total) GetList(string? keyword, int? hangId, string? trangThai, int page)
        {
            var list = new List<KhoaHocVM>();
            int total = 0;

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_ADMINKHOAHOC.GET_LIST_KHOAHOC", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_keyword", OracleDbType.NVarchar2).Value =
                string.IsNullOrWhiteSpace(keyword) ? DBNull.Value : keyword;

            cmd.Parameters.Add("p_hangId", OracleDbType.Decimal).Value =
                hangId.HasValue ? hangId.Value : DBNull.Value;

            cmd.Parameters.Add("p_trangThai", OracleDbType.NVarchar2).Value =
                string.IsNullOrWhiteSpace(trangThai) ? DBNull.Value : trangThai;

            cmd.Parameters.Add("p_page", OracleDbType.Decimal).Value = page;
            cmd.Parameters.Add("p_pageSize", OracleDbType.Decimal).Value = 10;

            cmd.Parameters.Add("p_total", OracleDbType.Decimal).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new KhoaHocVM
                    {
                        STT = ToInt32Safe(reader["STT"]),
                        KhoaHocId = ToInt32Safe(reader["KHOAHOCID"]),
                        TenKhoaHoc = reader["TENKHOAHOC"]?.ToString() ?? string.Empty,
                        HangId = ToInt32Safe(reader["HANGID"]),
                        TenHang = reader["TENHANG"]?.ToString() ?? string.Empty,
                        NgayBatDau = ToDateTimeSafe(reader["NGAYBATDAU"]),
                        NgayKetThuc = ToDateTimeSafe(reader["NGAYKETTHUC"]),
                        TrangThai = reader["TRANGTHAI"]?.ToString() ?? string.Empty
                    });
                }
            }

            total = ToInt32Safe(cmd.Parameters["p_total"].Value);

            return (list, total);
        }

        private int ToInt32Safe(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            if (value is OracleDecimal oracleDecimal)
                return oracleDecimal.ToInt32();

            if (value is decimal decimalValue)
                return decimal.ToInt32(decimalValue);

            if (value is int intValue)
                return intValue;

            return int.Parse(value.ToString()!);
        }

        private DateTime ToDateTimeSafe(object value)
        {
            if (value == null || value == DBNull.Value)
                return DateTime.MinValue;

            if (value is OracleDate oracleDate)
                return oracleDate.Value;

            if (value is DateTime dateTimeValue)
                return dateTimeValue;

            return DateTime.Parse(value.ToString()!);
        }
    }
}