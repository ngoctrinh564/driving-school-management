using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using driving_school_management.ViewModels;

namespace driving_school_management.Services
{
    public class AuthService : IAuthService
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

            var dbPassword = cmd.Parameters["o_password"].Value?.ToString();

            if (string.IsNullOrEmpty(dbPassword))
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, dbPassword))
                return null;

            return new LoginResult
            {
                UserId = ((OracleDecimal)cmd.Parameters["o_userId"].Value).ToInt32(),
                RoleId = ((OracleDecimal)cmd.Parameters["o_roleId"].Value).ToInt32(),
                Username = cmd.Parameters["o_username"].Value?.ToString() ?? "",
                IsActive = ((OracleDecimal)cmd.Parameters["o_isActive"].Value).ToInt32() == 1
            };
        }

        // ================= REGISTER =================
        public int Register(string username, string password, string email, int roleId)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PROC_REGISTER", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_username", OracleDbType.NVarchar2).Value = username;
            cmd.Parameters.Add("p_password", OracleDbType.NVarchar2).Value = hashedPassword;
            cmd.Parameters.Add("p_email", OracleDbType.NVarchar2).Value = email;
            cmd.Parameters.Add("p_roleId", OracleDbType.Int32).Value = roleId;
            cmd.Parameters.Add("o_result", OracleDbType.Int32).Direction = ParameterDirection.Output;

            conn.Open();
            cmd.ExecuteNonQuery();

            return Convert.ToInt32(cmd.Parameters["o_result"].Value.ToString());
        }

        // ================= GET USER PROFILE =================
        public UserProfileVM? GetUserProfile(int userId)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PROC_GET_USER_PROFILE", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new UserProfileVM
            {
                UserId = Convert.ToInt32(reader["USERID"]),
                HocVienId = Convert.ToInt32(reader["HOCVIENID"]),
                Username = reader["USERNAME"]?.ToString() ?? "",
                Email = reader["EMAIL"]?.ToString() ?? "",
                HoTen = reader["HOTEN"]?.ToString() ?? "",
                SoCmndCccd = reader["SOCMNDCCCD"]?.ToString() ?? "",
                NamSinh = reader["NAMSINH"] == DBNull.Value ? null : Convert.ToDateTime(reader["NAMSINH"]),
                GioiTinh = reader["GIOITINH"]?.ToString() ?? "",
                Sdt = reader["SDT"]?.ToString() ?? "",
                AvatarUrl = reader["AVATARURL"]?.ToString() ?? ""
            };
        }

        // ================= UPDATE USER PROFILE =================
        public int UpdateUserProfile(EditUserVM model)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PROC_UPDATE_USER_PROFILE", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = model.UserId;
            cmd.Parameters.Add("p_username", OracleDbType.NVarchar2).Value = model.Username;
            cmd.Parameters.Add("p_email", OracleDbType.NVarchar2).Value = model.Email;
            cmd.Parameters.Add("p_hoTen", OracleDbType.NVarchar2).Value = string.IsNullOrWhiteSpace(model.HoTen) ? DBNull.Value : model.HoTen;
            cmd.Parameters.Add("p_soCmndCccd", OracleDbType.NVarchar2).Value = string.IsNullOrWhiteSpace(model.SoCmndCccd) ? DBNull.Value : model.SoCmndCccd;
            cmd.Parameters.Add("p_namSinh", OracleDbType.Date).Value = model.NamSinh.HasValue ? model.NamSinh.Value : DBNull.Value;
            cmd.Parameters.Add("p_gioiTinh", OracleDbType.NVarchar2).Value = string.IsNullOrWhiteSpace(model.GioiTinh) ? DBNull.Value : model.GioiTinh;
            cmd.Parameters.Add("p_sdt", OracleDbType.NVarchar2).Value = string.IsNullOrWhiteSpace(model.Sdt) ? DBNull.Value : model.Sdt;
            cmd.Parameters.Add("p_avatarUrl", OracleDbType.NVarchar2).Value = string.IsNullOrWhiteSpace(model.AvatarUrl) ? DBNull.Value : model.AvatarUrl;

            cmd.Parameters.Add("o_result", OracleDbType.Int32).Direction = ParameterDirection.Output;

            conn.Open();
            cmd.ExecuteNonQuery();

            return Convert.ToInt32(cmd.Parameters["o_result"].Value.ToString());
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

        public async Task<bool> IsProfileCompleted(int userId)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("BEGIN :result := FUNC_CHECK_USER_PROFILE(:p_userId); END;", conn);

            cmd.Parameters.Add("result", OracleDbType.Int32).Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            var result = (OracleDecimal)cmd.Parameters["result"].Value;
            return result.ToInt32() == 1;
        }
    }

    public class LoginResult
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string Username { get; set; } = "";
        public bool IsActive { get; set; }
    }
}