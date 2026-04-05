using driving_school_management.ViewModels;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace driving_school_management.Helpers
{
    public class FlashCardOracleHelper
    {
        private readonly string _connectionString;

        public FlashCardOracleHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDb")!;
        }

        public List<FlashCardSignSummaryVM> GetSummary()
        {
            var list = new List<FlashCardSignSummaryVM>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("GET_FLASHCARD_SUMMARY", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new FlashCardSignSummaryVM
                {
                    IdBienBao = Convert.ToInt32(reader["IDBIENBAO"]),
                    TenBienBao = reader["TENBIENBAO"]?.ToString() ?? "",
                    Ynghia = reader["YNGHIA"]?.ToString() ?? "",
                    HinhAnh = reader["HINHANH"]?.ToString() ?? "",
                    SoDanhGia = Convert.ToInt32(reader["SODANHGIA"])
                });
            }

            return list;
        }

        public FlashCardDetailVM? GetBySignId(int idBienBao)
        {
            FlashCardDetailVM? vm = null;

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("GET_FLASHCARD_BY_SIGN", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_IDBIENBAO", OracleDbType.Int32).Value = idBienBao;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();
            return vm;
        }
    }
}