using driving_school_management.Models;
using driving_school_management.ViewModels;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

public class AdminDashboardService
{
    private readonly string _connectionString = string.Empty;

    public AdminDashboardService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("OracleDb") ?? string.Empty;
    }

    public async Task<DashboardVM> GetDashboard()
    {
        DashboardVM vm = new DashboardVM();

        using OracleConnection conn = new OracleConnection(_connectionString);
        using OracleCommand cmd = new OracleCommand();

        cmd.Connection = conn;
        cmd.CommandText = "PROC_ADMIN_DASHBOARD";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("O_TOTALUSER", OracleDbType.Decimal, ParameterDirection.Output);
        cmd.Parameters.Add("O_TOTALHOSO", OracleDbType.Decimal, ParameterDirection.Output);
        cmd.Parameters.Add("O_TOTALGPLX", OracleDbType.Decimal, ParameterDirection.Output);
        cmd.Parameters.Add("O_TOTALBAITHI", OracleDbType.Decimal, ParameterDirection.Output);
        cmd.Parameters.Add("O_USERACTIVE", OracleDbType.Decimal, ParameterDirection.Output);
        cmd.Parameters.Add("O_USERINACTIVE", OracleDbType.Decimal, ParameterDirection.Output);
        cmd.Parameters.Add("O_HOSODADUYET", OracleDbType.Decimal, ParameterDirection.Output);
        cmd.Parameters.Add("O_HOSODANGXULY", OracleDbType.Decimal, ParameterDirection.Output);
        cmd.Parameters.Add("O_RECENTUSERS", OracleDbType.RefCursor, ParameterDirection.Output);
        cmd.Parameters.Add("O_CHARTMONTH", OracleDbType.RefCursor, ParameterDirection.Output);
        cmd.Parameters.Add("O_CHARTHANGGPLX", OracleDbType.RefCursor, ParameterDirection.Output);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();

        vm.TotalUser = ConvertOracleDecimalToInt(cmd.Parameters["O_TOTALUSER"].Value);
        vm.TotalHoSo = ConvertOracleDecimalToInt(cmd.Parameters["O_TOTALHOSO"].Value);
        vm.TotalGPLX = ConvertOracleDecimalToInt(cmd.Parameters["O_TOTALGPLX"].Value);
        vm.TotalBaiThi = ConvertOracleDecimalToInt(cmd.Parameters["O_TOTALBAITHI"].Value);
        vm.UserActive = ConvertOracleDecimalToInt(cmd.Parameters["O_USERACTIVE"].Value);
        vm.UserInactive = ConvertOracleDecimalToInt(cmd.Parameters["O_USERINACTIVE"].Value);
        vm.HoSoDaDuyet = ConvertOracleDecimalToInt(cmd.Parameters["O_HOSODADUYET"].Value);
        vm.HoSoDangXuLy = ConvertOracleDecimalToInt(cmd.Parameters["O_HOSODANGXULY"].Value);

        using (OracleDataReader readerUsers = ((OracleRefCursor)cmd.Parameters["O_RECENTUSERS"].Value).GetDataReader())
        {
            while (await readerUsers.ReadAsync())
            {
                vm.RecentUsers.Add(new UserVM
                {
                    UserId = Convert.ToInt32(readerUsers["USERID"]),
                    Username = readerUsers["USERNAME"]?.ToString() ?? string.Empty,
                    IsActive = Convert.ToInt32(readerUsers["ISACTIVE"]) == 1
                });
            }
        }

        using (OracleDataReader readerMonth = ((OracleRefCursor)cmd.Parameters["O_CHARTMONTH"].Value).GetDataReader())
        {
            while (await readerMonth.ReadAsync())
            {
                vm.Thang.Add("Tháng " + (readerMonth["THANG"]?.ToString() ?? "0"));
                vm.SoHoSoTheoThang.Add(readerMonth["SO_HO_SO"] == DBNull.Value ? 0 : Convert.ToInt32(readerMonth["SO_HO_SO"]));
                vm.DoanhThuTheoThang.Add(readerMonth["DOANH_THU"] == DBNull.Value ? 0 : Convert.ToDecimal(readerMonth["DOANH_THU"]));
            }
        }

        using (OracleDataReader readerHang = ((OracleRefCursor)cmd.Parameters["O_CHARTHANGGPLX"].Value).GetDataReader())
        {
            while (await readerHang.ReadAsync())
            {
                vm.HangLabels.Add(readerHang["TENHANG"]?.ToString() ?? string.Empty);
                vm.HangData.Add(readerHang["SO_LUONG"] == DBNull.Value ? 0 : Convert.ToInt32(readerHang["SO_LUONG"]));
            }
        }

        return vm;
    }

    private int ConvertOracleDecimalToInt(object value)
    {
        if (value == null || value == DBNull.Value)
            return 0;

        if (value is OracleDecimal oracleDecimal)
            return oracleDecimal.IsNull ? 0 : oracleDecimal.ToInt32();

        return Convert.ToInt32(value);
    }
}