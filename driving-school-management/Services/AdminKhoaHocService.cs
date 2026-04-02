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
            cmd.BindByName = true;

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

        public KhoaHocFormVM? GetDetail(int khoaHocId)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_ADMINKHOAHOC.GET_DETAIL_KHOAHOC", conn);
            cmd.BindByName = true;

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_khoaHocId", OracleDbType.Decimal).Value = khoaHocId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new KhoaHocFormVM
                {
                    KhoaHocId = ToInt32Safe(reader["KHOAHOCID"]),
                    HangId = ToInt32Safe(reader["HANGID"]),
                    TenKhoaHoc = reader["TENKHOAHOC"]?.ToString() ?? string.Empty,
                    TenHang = reader["TENHANG"]?.ToString() ?? string.Empty,
                    NgayBatDau = ToDateTimeSafe(reader["NGAYBATDAU"]),
                    NgayKetThuc = ToDateTimeSafe(reader["NGAYKETTHUC"]),
                    DiaDiem = reader["DIADIEM"]?.ToString() ?? string.Empty,
                    TrangThai = reader["TRANGTHAI"]?.ToString() ?? string.Empty
                };
            }

            return null;
        }

        public int Insert(KhoaHocFormVM model)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_ADMINKHOAHOC.INSERT_KHOAHOC", conn);
            cmd.BindByName = true;

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_hangId", OracleDbType.Decimal).Value = model.HangId;
            cmd.Parameters.Add("p_tenKhoaHoc", OracleDbType.NVarchar2).Value = model.TenKhoaHoc;
            cmd.Parameters.Add("p_ngayBatDau", OracleDbType.Date).Value = model.NgayBatDau;
            cmd.Parameters.Add("p_ngayKetThuc", OracleDbType.Date).Value = model.NgayKetThuc;
            cmd.Parameters.Add("p_diaDiem", OracleDbType.NVarchar2).Value =
                string.IsNullOrWhiteSpace(model.DiaDiem) ? DBNull.Value : model.DiaDiem;

            cmd.Parameters.Add("p_khoaHocId", OracleDbType.Decimal).Direction = ParameterDirection.Output;

            conn.Open();
            cmd.ExecuteNonQuery();

            return ToInt32Safe(cmd.Parameters["p_khoaHocId"].Value);
        }

        public void Update(KhoaHocFormVM model)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_ADMINKHOAHOC.UPDATE_KHOAHOC", conn);
            cmd.BindByName = true;

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_khoaHocId", OracleDbType.Decimal).Value = model.KhoaHocId;
            cmd.Parameters.Add("p_hangId", OracleDbType.Decimal).Value = model.HangId;
            cmd.Parameters.Add("p_tenKhoaHoc", OracleDbType.NVarchar2).Value = model.TenKhoaHoc;
            cmd.Parameters.Add("p_ngayBatDau", OracleDbType.Date).Value = model.NgayBatDau;
            cmd.Parameters.Add("p_ngayKetThuc", OracleDbType.Date).Value = model.NgayKetThuc;
            cmd.Parameters.Add("p_diaDiem", OracleDbType.NVarchar2).Value =
                string.IsNullOrWhiteSpace(model.DiaDiem) ? DBNull.Value : model.DiaDiem;

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public List<dynamic> GetHangs()
        {
            var list = new List<dynamic>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand(
                "SELECT hangId, tenHang FROM HangGplx ORDER BY tenHang", conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new
                {
                    hangId = ToInt32Safe(reader["HANGID"]),
                    tenHang = reader["TENHANG"]?.ToString() ?? string.Empty
                });
            }

            return list;
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