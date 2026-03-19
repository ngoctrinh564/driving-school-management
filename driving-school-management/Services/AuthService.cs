using Microsoft.CodeAnalysis.Scripting;
using Oracle.ManagedDataAccess.Client;
using Org.BouncyCastle.Crypto.Generators;
using System.Data;

namespace driving_school_management.Services
{
    public class AuthService
    {
        private readonly string _connectionString;

        public AuthService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("OracleDb");
        }

        // ================= LOGIN =================
        public async Task<LoginResult?> Login(string username, string password)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PROC_LOGIN", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_username", OracleDbType.NVarchar2).Value = username;

            cmd.Parameters.Add("o_userId", OracleDbType.Decimal).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("o_roleId", OracleDbType.Decimal).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("o_username", OracleDbType.NVarchar2, 100).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("o_password", OracleDbType.NVarchar2, 255).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("o_isActive", OracleDbType.Int32).Direction = ParameterDirection.Output;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            if (cmd.Parameters["o_userId"].Value == DBNull.Value)
                return null;

            var dbPassword = cmd.Parameters["o_password"].Value.ToString();

            if (!BCrypt.Net.BCrypt.Verify(password, dbPassword))
                return null;

            return new LoginResult
            {
                UserId = ((Oracle.ManagedDataAccess.Types.OracleDecimal)cmd.Parameters["o_userId"].Value).ToInt32(),
                RoleId = ((Oracle.ManagedDataAccess.Types.OracleDecimal)cmd.Parameters["o_roleId"].Value).ToInt32(),
                Username = cmd.Parameters["o_username"].Value.ToString(),
                IsActive = ((Oracle.ManagedDataAccess.Types.OracleDecimal)cmd.Parameters["o_isActive"].Value).ToInt32() == 1
            };
        }

        // ================= REGISTER =================
        public async Task<int> Register(string username, string password)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PROC_REGISTER", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_username", OracleDbType.NVarchar2).Value = username;
            cmd.Parameters.Add("p_password", OracleDbType.NVarchar2).Value = BCrypt.Net.BCrypt.HashPassword(password);
            cmd.Parameters.Add("p_roleId", OracleDbType.Int32).Value = 2;

            cmd.Parameters.Add("o_result", OracleDbType.Int32).Direction = ParameterDirection.Output;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return Convert.ToInt32(cmd.Parameters["o_result"].Value);
        }

        // ================= RESET PASSWORD =================
        public async Task<int> ResetPassword(string username, string newPassword)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PROC_RESET_PASSWORD", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_username", OracleDbType.NVarchar2).Value = username;
            cmd.Parameters.Add("p_newPassword", OracleDbType.NVarchar2).Value = BCrypt.Net.BCrypt.HashPassword(newPassword);

            cmd.Parameters.Add("o_result", OracleDbType.Int32).Direction = ParameterDirection.Output;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return Convert.ToInt32(cmd.Parameters["o_result"].Value);
        }
    }

    public class LoginResult
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string Username { get; set; }
        public bool IsActive { get; set; }
    }
}