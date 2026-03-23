using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace driving_school_management.Services
{
    public class LyThuyetService
    {
        private readonly string _conn;

        public LyThuyetService(IConfiguration config)
        {
            _conn = config.GetConnectionString("OracleDb");
        }

        public DataTable GetBoDe(string maHang)
        {
            var dt = new DataTable();

            using var conn = new OracleConnection(_conn);
            using var cmd = new OracleCommand("PR_GET_BODE_BY_HANG", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_MAHANG", OracleDbType.Varchar2).Value = maHang;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();
            using var da = new OracleDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

        public DataTable GetExam(int boDeId)
        {
            var dt = new DataTable();

            using var conn = new OracleConnection(_conn);
            using var cmd = new OracleCommand("PR_GET_EXAM_DETAIL", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_BODEID", OracleDbType.Int32).Value = boDeId;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();
            using var da = new OracleDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

        public DataTable GetRandomExam(string maHang)
        {
            var dt = new DataTable();

            using var conn = new OracleConnection(_conn);
            using var cmd = new OracleCommand("PR_RANDOM_EXAM", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_MAHANG", OracleDbType.Varchar2).Value = maHang;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();
            using var da = new OracleDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

        public DataTable GetHistory(int userId, int boDeId)
        {
            var dt = new DataTable();

            using var conn = new OracleConnection(_conn);
            using var cmd = new OracleCommand("PR_GET_HISTORY_EXAM", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_USERID", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("P_BODEID", OracleDbType.Int32).Value = boDeId;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();
            using var da = new OracleDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

        public DataTable GetLastBaiLamByHang(int userId, string maHang)
        {
            var dt = new DataTable();

            using var conn = new OracleConnection(_conn);
            using var cmd = new OracleCommand("PR_GET_LAST_BAILAM_BY_HANG", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_USERID", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("P_MAHANG", OracleDbType.Varchar2).Value = maHang;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();
            using var da = new OracleDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

        public DataTable GetHangInfo(string maHang)
        {
            var dt = new DataTable();

            using var conn = new OracleConnection(_conn);
            using var cmd = new OracleCommand("PR_GET_HANG_INFO", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_MAHANG", OracleDbType.Varchar2).Value = maHang;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();
            using var da = new OracleDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

        public DataTable GetHocAll(string maHang)
        {
            var dt = new DataTable();

            using var conn = new OracleConnection(_conn);
            using var cmd = new OracleCommand("PR_GET_ALL_QUESTION_FOR_HANG", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_MAHANG", OracleDbType.Varchar2).Value = maHang;
            cmd.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();
            using var da = new OracleDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

        public int SaveBaiLam(int userId, int boDeId, int thoiGian, int soCauSai, bool ketQua)
        {
            using var conn = new OracleConnection(_conn);
            using var cmd = new OracleCommand("PR_SAVE_BAILAM", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_USERID", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("P_BODEID", OracleDbType.Int32).Value = boDeId;
            cmd.Parameters.Add("P_THOIGIAN", OracleDbType.Int32).Value = thoiGian;
            cmd.Parameters.Add("P_SOCAUSAI", OracleDbType.Int32).Value = soCauSai;
            cmd.Parameters.Add("P_KETQUA", OracleDbType.Int32).Value = ketQua ? 1 : 0;

            var pOut = new OracleParameter("P_BAILAMID", OracleDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(pOut);

            conn.Open();
            cmd.ExecuteNonQuery();

            return ((OracleDecimal)pOut.Value).ToInt32();
        }

        public void SaveChiTiet(int baiLamId, int cauHoiId, string? dapAn, bool ketQua)
        {
            using var conn = new OracleConnection(_conn);
            using var cmd = new OracleCommand("PR_SAVE_CT_BAILAM", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("P_BAILAMID", OracleDbType.Int32).Value = baiLamId;
            cmd.Parameters.Add("P_CAUHOIID", OracleDbType.Int32).Value = cauHoiId;
            cmd.Parameters.Add("P_DAPAN", OracleDbType.NVarchar2).Value = (object?)dapAn ?? DBNull.Value;
            cmd.Parameters.Add("P_KETQUA", OracleDbType.Int32).Value = ketQua ? 1 : 0;

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}