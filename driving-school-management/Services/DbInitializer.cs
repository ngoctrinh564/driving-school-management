using Oracle.ManagedDataAccess.Client;

namespace driving_school_management.Services
{
    public class DbInitializer
    {
        private readonly string _connectionString;

        public DbInitializer(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDb");
        }

        public void Init()
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new OracleCommand("pkg_khoahoc.CAP_NHAT_TRANG_THAI", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}