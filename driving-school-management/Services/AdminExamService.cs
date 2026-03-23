using driving_school_management.Models.DTOs;
using Oracle.ManagedDataAccess.Client;
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

        // ===================== GET LIST KYTHI =====================
        public async Task<List<KyThiDto>> GetKyThiAsync()
        {
            var list = new List<KyThiDto>();

            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_GET_KYTHI", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor)
                              .Direction = ParameterDirection.Output;

                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new KyThiDto
                        {
                            KyThiId = reader["KYTHIID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["KYTHIID"]),
                            TenKyThi = reader["TENKYTHI"]?.ToString(),
                            LoaiKyThi = reader["LOAIKYTHI"]?.ToString(),
                            SoLuongDangKy = reader["SOLUONGDANGKY"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["SOLUONGDANGKY"])
                        });
                    }
                }
            }

            return list;
        }

        // ===================== CREATE KYTHI =====================
        public async Task CreateKyThi(string ten, string loai)
        {
            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_CREATE_KYTHI", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_tenKyThi", OracleDbType.NVarchar2).Value = ten;
                cmd.Parameters.Add("p_loaiKyThi", OracleDbType.NVarchar2).Value = loai;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // ===================== UPDATE KYTHI =====================
        public async Task UpdateKyThi(int id, string ten, string loai)
        {
            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_UPDATE_KYTHI", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = id;
                cmd.Parameters.Add("p_tenKyThi", OracleDbType.NVarchar2).Value = ten;
                cmd.Parameters.Add("p_loaiKyThi", OracleDbType.NVarchar2).Value = loai;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // ===================== DELETE KYTHI =====================
        public async Task DeleteKyThi(int id)
        {
            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_DELETE_KYTHI", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = id;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // ===================== CREATE LICHTHI =====================
        public async Task CreateLichThi(int kyThiId, DateTime thoiGian, string diaDiem)
        {
            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_CREATE_LICHTHI", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = kyThiId;
                cmd.Parameters.Add("p_thoiGianThi", OracleDbType.TimeStamp).Value = thoiGian;
                cmd.Parameters.Add("p_diaDiem", OracleDbType.NVarchar2).Value = diaDiem;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // ===================== GET LICHTHI BY KYTHI =====================
        public async Task<List<LichThiDto>> GetLichThiByKyThi(int kyThiId)
        {
            var list = new List<LichThiDto>();

            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_GET_LICHTHI_BY_KYTHI", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = kyThiId;
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor)
                              .Direction = ParameterDirection.Output;

                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new LichThiDto
                        {
                            LichThiId = reader["LICHTHIID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["LICHTHIID"]),
                            DiaDiem = reader["DIADIEM"]?.ToString(),
                            ThoiGianThi = reader["THOIGIANTHI"] == DBNull.Value
                                ? DateTime.MinValue
                                : Convert.ToDateTime(reader["THOIGIANTHI"])
                        });
                    }
                }
            }

            return list;
        }
    }
}