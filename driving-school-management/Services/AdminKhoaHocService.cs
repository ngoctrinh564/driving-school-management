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

        public (List<KhoaHocVM>, int) GetList(string keyword, int? hangId, string trangThai, int page)
        {
            var list = new List<KhoaHocVM>();
            int total = 0;

            using (var conn = new OracleConnection(_connectionString))
            {
                using (var cmd = new OracleCommand("PKG_KHOAHOC.GET_LIST_KHOAHOC", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_keyword", OracleDbType.NVarchar2).Value = (object)keyword ?? DBNull.Value;
                    cmd.Parameters.Add("p_hangId", OracleDbType.Int32).Value = (object)hangId ?? DBNull.Value;
                    cmd.Parameters.Add("p_trangThai", OracleDbType.NVarchar2).Value = (object)trangThai ?? DBNull.Value;
                    cmd.Parameters.Add("p_page", OracleDbType.Int32).Value = page;
                    cmd.Parameters.Add("p_pageSize", OracleDbType.Int32).Value = 10;

                    cmd.Parameters.Add("p_total", OracleDbType.Int32).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new KhoaHocVM
                            {
                                STT = ((OracleDecimal)reader["STT"]).ToInt32(),
                                TenKhoaHoc = reader["tenKhoaHoc"].ToString(),
                                TenHang = reader["tenHang"].ToString(),
                                NgayBatDau = Convert.ToDateTime(reader["ngayBatDau"]),
                                NgayKetThuc = Convert.ToDateTime(reader["ngayKetThuc"]),
                                TrangThai = reader["trangThai"].ToString()
                            });
                        }
                    }

                    total = ((OracleDecimal)cmd.Parameters["p_total"].Value).ToInt32();
                }
            }

            return (list, total);
        }
    }
}
