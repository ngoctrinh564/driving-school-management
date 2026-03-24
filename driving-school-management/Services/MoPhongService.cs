using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace driving_school_management.Services
{
    public interface IMoPhongService
    {
        Task<DataTable> GetOnTapMoPhongAsync();
    }
    public class MoPhongService : IMoPhongService
    {
        private readonly string _connectionString;

        public MoPhongService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDB");
        }

        private OracleConnection CreateConnection()
        {
            return new OracleConnection(_connectionString);
        }

        private async Task<DataTable> ExecuteRefCursorAsync(string procName, params OracleParameter[] parameters)
        {
            using var conn = CreateConnection();
            using var cmd = new OracleCommand(procName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            foreach (var p in parameters)
                cmd.Parameters.Add(p);

            await conn.OpenAsync();

            var dt = new DataTable();
            using var adapter = new OracleDataAdapter(cmd);
            adapter.Fill(dt);

            return dt;
        }

        public async Task<DataTable> GetOnTapMoPhongAsync()
        {
            return await ExecuteRefCursorAsync(
                "PROC_MP_ONTAP_INDEX",
                new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output)
            );
        }
    }
}