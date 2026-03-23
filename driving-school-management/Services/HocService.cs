using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace driving_school_management.Services
{
    public class HocService
    {
        private readonly string _connectionString;

        public HocService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDb");
        }

        public DataTable GetHocDashboard(string maHang, int? userId)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PR_HOC_INDEX", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("P_MAHANG", OracleDbType.Varchar2).Value = maHang;
            cmd.Parameters.Add("P_USERID", OracleDbType.Decimal).Value = (object?)userId ?? DBNull.Value;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using (var whoCmd = new OracleCommand("SELECT USER FROM DUAL", conn))
            {
                var currentUser = whoCmd.ExecuteScalar()?.ToString();
                System.Diagnostics.Debug.WriteLine("ORACLE USER = " + currentUser);
            }

            using (var countCmd = new OracleCommand(@"
        SELECT COUNT(*) 
        FROM CAUHOILYTHUYET 
        WHERE XEMAY = 1 AND CAULIET = 1", conn))
            {
                var count = countCmd.ExecuteScalar()?.ToString();
                System.Diagnostics.Debug.WriteLine("DIRECT COUNT XEMAY=1 AND CAULIET=1 = " + count);
            }

            cmd.ExecuteNonQuery();

            using var reader = ((OracleRefCursor)cmd.Parameters["P_CURSOR"].Value).GetDataReader();
            var dt = new DataTable();
            dt.Load(reader);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                foreach (DataColumn col in dt.Columns)
                {
                    System.Diagnostics.Debug.WriteLine($"{col.ColumnName} = {row[col.ColumnName]}");
                }
            }

            return dt;
        }

        public DataTable GetCauLiet(string maHang)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PR_HOC_CAU_LIET", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_MAHANG", OracleDbType.Varchar2).Value = maHang;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();
            cmd.ExecuteNonQuery();

            using var reader = ((OracleRefCursor)cmd.Parameters["P_CURSOR"].Value).GetDataReader();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public DataTable GetChuY(string maHang)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PR_HOC_CHU_Y", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_MAHANG", OracleDbType.Varchar2).Value = maHang;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();
            cmd.ExecuteNonQuery();

            using var reader = ((OracleRefCursor)cmd.Parameters["P_CURSOR"].Value).GetDataReader();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public DataTable GetFlashCardBienBao(int? userId)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PR_FLASHCARD_BIENBAO", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_USERID", OracleDbType.Decimal).Value = (object?)userId ?? DBNull.Value;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();
            cmd.ExecuteNonQuery();

            using var reader = ((OracleRefCursor)cmd.Parameters["P_CURSOR"].Value).GetDataReader();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public void SaveFlashCard(int userId, int idBienBao, string danhGia)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PR_FLASHCARD_SAVE", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_USERID", OracleDbType.Decimal).Value = userId;
            cmd.Parameters.Add("P_IDBIENBAO", OracleDbType.Decimal).Value = idBienBao;
            cmd.Parameters.Add("P_DANHGIA", OracleDbType.NVarchar2).Value = danhGia;

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public int GetNumberAsInt(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            if (value is Oracle.ManagedDataAccess.Types.OracleDecimal oracleDecimal)
                return oracleDecimal.ToInt32();

            if (value is decimal dec)
                return (int)dec;

            if (value is int i)
                return i;

            if (value is long l)
                return (int)l;

            if (value is short s)
                return s;

            if (value is byte b)
                return b;

            return int.Parse(value.ToString() ?? "0");
        }

        public DataTable GetHangList()
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PR_GET_HANG", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();
            cmd.ExecuteNonQuery();

            using var reader = ((OracleRefCursor)cmd.Parameters["P_CURSOR"].Value).GetDataReader();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }
    }
}