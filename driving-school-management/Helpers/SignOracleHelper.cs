using driving_school_management.ViewModels;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace driving_school_management.Helpers
{
    public class SignOracleHelper
    {
        private readonly string _connectionString;

        public SignOracleHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDb")!;
        }

        public List<SignVM> GetAll()
        {
            var list = new List<SignVM>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PROC_BIENBAO_GET_ALL", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new SignVM
                {
                    IDBIENBAO = Convert.ToInt32(reader["IDBIENBAO"]),
                    TENBIENBAO = reader["TENBIENBAO"]?.ToString() ?? "",
                    YNGHIA = reader["YNGHIA"]?.ToString() ?? "",
                    HINHANH = reader["HINHANH"]?.ToString() ?? ""
                });
            }

            return list;
        }

        public SignVM? GetById(int idBienBao)
        {
            SignVM? item = null;

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PROC_BIENBAO_GET_BY_ID", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_IDBIENBAO", OracleDbType.Int32).Value = idBienBao;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                item = new SignVM
                {
                    IDBIENBAO = Convert.ToInt32(reader["IDBIENBAO"]),
                    TENBIENBAO = reader["TENBIENBAO"]?.ToString() ?? "",
                    YNGHIA = reader["YNGHIA"]?.ToString() ?? "",
                    HINHANH = reader["HINHANH"]?.ToString() ?? ""
                };
            }

            return item;
        }

        public int Insert(SignVM vm)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PROC_BIENBAO_INSERT", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_TENBIENBAO", OracleDbType.NVarchar2).Value = vm.TENBIENBAO;
            cmd.Parameters.Add("P_YNGHIA", OracleDbType.NVarchar2).Value = vm.YNGHIA;
            cmd.Parameters.Add("P_HINHANH", OracleDbType.Varchar2).Value = (object?)vm.HINHANH ?? DBNull.Value;
            cmd.Parameters.Add("P_NEW_ID", OracleDbType.Int32).Direction = ParameterDirection.Output;

            conn.Open();
            cmd.ExecuteNonQuery();

            return Convert.ToInt32(cmd.Parameters["P_NEW_ID"].Value.ToString());
        }

        public void Update(SignVM vm)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PROC_BIENBAO_UPDATE", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_IDBIENBAO", OracleDbType.Int32).Value = vm.IDBIENBAO;
            cmd.Parameters.Add("P_TENBIENBAO", OracleDbType.NVarchar2).Value = vm.TENBIENBAO;
            cmd.Parameters.Add("P_YNGHIA", OracleDbType.NVarchar2).Value = vm.YNGHIA;
            cmd.Parameters.Add("P_HINHANH", OracleDbType.Varchar2).Value = (object?)vm.HINHANH ?? DBNull.Value;

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void Delete(int idBienBao)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PROC_BIENBAO_DELETE", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_IDBIENBAO", OracleDbType.Int32).Value = idBienBao;

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}