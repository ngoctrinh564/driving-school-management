using driving_school_management.ViewModels;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

public class AdminDashboardService
{
    private readonly string _connectionString;

    public AdminDashboardService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("OracleDb");
    }

    public async Task<DashboardVM> GetDashboard()
    {
        var vm = new DashboardVM();

        using var conn = new OracleConnection(_connectionString);
        using var cmd = new OracleCommand("PROC_ADMIN_DASHBOARD", conn);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.Add("o_totalUser", OracleDbType.Decimal).Direction = ParameterDirection.Output;
        cmd.Parameters.Add("o_totalHoSo", OracleDbType.Decimal).Direction = ParameterDirection.Output;
        cmd.Parameters.Add("o_totalGPLX", OracleDbType.Decimal).Direction = ParameterDirection.Output;
        cmd.Parameters.Add("o_totalBaiThi", OracleDbType.Decimal).Direction = ParameterDirection.Output;
        cmd.Parameters.Add("o_userActive", OracleDbType.Decimal).Direction = ParameterDirection.Output;
        cmd.Parameters.Add("o_userInactive", OracleDbType.Decimal).Direction = ParameterDirection.Output;
        cmd.Parameters.Add("o_recentUsers", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();

        vm.TotalUser = ((OracleDecimal)cmd.Parameters["o_totalUser"].Value).ToInt32();
        vm.TotalHoSo = ((OracleDecimal)cmd.Parameters["o_totalHoSo"].Value).ToInt32();
        vm.TotalGPLX = ((OracleDecimal)cmd.Parameters["o_totalGPLX"].Value).ToInt32();
        vm.TotalBaiThi = ((OracleDecimal)cmd.Parameters["o_totalBaiThi"].Value).ToInt32();
        vm.UserActive = ((OracleDecimal)cmd.Parameters["o_userActive"].Value).ToInt32();
        vm.UserInactive = ((OracleDecimal)cmd.Parameters["o_userInactive"].Value).ToInt32();

        var reader = ((OracleRefCursor)cmd.Parameters["o_recentUsers"].Value).GetDataReader();
        vm.RecentUsers = new List<UserVM>();
        while (await reader.ReadAsync())
        {
            vm.RecentUsers.Add(new UserVM
            {
                UserId = reader.GetInt32(0),
                Username = reader.GetString(1),
                IsActive = reader.GetInt32(2) == 1
            });
        }

        return vm;
    }
}