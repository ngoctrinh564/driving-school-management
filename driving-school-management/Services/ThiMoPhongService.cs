using driving_school_management.Models.DTOs;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
public interface IThiMoPhongService
{
    Task<DataTable> GetDanhSachBoDeAsync(int? userId);
    Task<DataTable> GetBoDeChiTietAsync(int idBoDe);
    Task<DataTable> GetLichSuHeaderAsync(int idBaiLam);
    Task<DataTable> GetLichSuChiTietAsync(int idBaiLam);
    Task<DataTable> GetKetQuaHeaderAsync(int idBaiLam);
    Task<DataTable> GetKetQuaChiTietAsync(int idBaiLam);
    Task<OracleExecResultDto> ChamBoDeAsync(int idBoDe, string flagsText);
    Task<OracleExecResultDto> LuuKetQuaAsync(int userId, int idBoDe, string flagsText);
    Task<DataTable> GetDeNgauNhienAsync();
    Task<OracleExecResultDto> ChamDeNgauNhienAsync(string selectedIdsText, string flagsText);
}
public class ThiMoPhongService : IThiMoPhongService
{
    private readonly string _connectionString;

    public ThiMoPhongService(IConfiguration configuration)
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

    public async Task<DataTable> GetDanhSachBoDeAsync(int? userId)
    {
        return await ExecuteRefCursorAsync(
            "PROC_MP_DANH_SACH_BODE",
            new OracleParameter("P_USERID", OracleDbType.Int32)
            {
                Value = userId.HasValue ? userId.Value : DBNull.Value
            },
            new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output)
        );
    }

    public async Task<DataTable> GetBoDeChiTietAsync(int idBoDe)
    {
        return await ExecuteRefCursorAsync(
            "PROC_MP_LAY_BODE_CHI_TIET",
            new OracleParameter("P_IDBODE", OracleDbType.Int32) { Value = idBoDe },
            new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output)
        );
    }

    public async Task<DataTable> GetLichSuHeaderAsync(int idBaiLam)
    {
        return await ExecuteRefCursorAsync(
            "PROC_MP_LICH_SU_HEADER",
            new OracleParameter("P_IDBAILAM", OracleDbType.Int32) { Value = idBaiLam },
            new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output)
        );
    }

    public async Task<DataTable> GetLichSuChiTietAsync(int idBaiLam)
    {
        return await ExecuteRefCursorAsync(
            "PROC_MP_LICH_SU_CHI_TIET",
            new OracleParameter("P_IDBAILAM", OracleDbType.Int32) { Value = idBaiLam },
            new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output)
        );
    }

    public async Task<DataTable> GetKetQuaHeaderAsync(int idBaiLam)
    {
        return await ExecuteRefCursorAsync(
            "PROC_MP_KET_QUA_HEADER",
            new OracleParameter("P_IDBAILAM", OracleDbType.Int32) { Value = idBaiLam },
            new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output)
        );
    }

    public async Task<DataTable> GetKetQuaChiTietAsync(int idBaiLam)
    {
        return await ExecuteRefCursorAsync(
            "PROC_MP_KET_QUA_CHI_TIET",
            new OracleParameter("P_IDBAILAM", OracleDbType.Int32) { Value = idBaiLam },
            new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output)
        );
    }

    public async Task<OracleExecResultDto> ChamBoDeAsync(int idBoDe, string flagsText)
    {
        using var conn = CreateConnection();
        using var cmd = new OracleCommand("PROC_MP_CHAM_BODE", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("P_IDBODE", OracleDbType.Int32).Value = idBoDe;
        cmd.Parameters.Add("P_FLAGS_TEXT", OracleDbType.Clob).Value = flagsText ?? string.Empty;

        var pTongDiem = new OracleParameter("O_TONGDIEM", OracleDbType.Int32, ParameterDirection.Output);
        var pDat = new OracleParameter("O_DAT", OracleDbType.Int32, ParameterDirection.Output);

        cmd.Parameters.Add(pTongDiem);
        cmd.Parameters.Add(pDat);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();

        return new OracleExecResultDto
        {
            TongDiem = ((OracleDecimal)pTongDiem.Value).ToInt32(),
            Dat = ((OracleDecimal)pDat.Value).ToInt32() == 1
        };
    }

    public async Task<OracleExecResultDto> LuuKetQuaAsync(int userId, int idBoDe, string flagsText)
    {
        using var conn = CreateConnection();
        using var cmd = new OracleCommand("PROC_MP_LUU_KET_QUA", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("P_USERID", OracleDbType.Int32).Value = userId;
        cmd.Parameters.Add("P_IDBODE", OracleDbType.Int32).Value = idBoDe;
        cmd.Parameters.Add("P_FLAGS_TEXT", OracleDbType.Clob).Value = flagsText ?? string.Empty;

        var pTongDiem = new OracleParameter("O_TONGDIEM", OracleDbType.Int32, ParameterDirection.Output);
        var pDat = new OracleParameter("O_DAT", OracleDbType.Int32, ParameterDirection.Output);
        var pIdBaiLam = new OracleParameter("O_IDBAILAM", OracleDbType.Int32, ParameterDirection.Output);

        cmd.Parameters.Add(pTongDiem);
        cmd.Parameters.Add(pDat);
        cmd.Parameters.Add(pIdBaiLam);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();

        return new OracleExecResultDto
        {
            TongDiem = ((OracleDecimal)pTongDiem.Value).ToInt32(),
            Dat = ((OracleDecimal)pDat.Value).ToInt32() == 1,
            IdBaiLam = ((OracleDecimal)pIdBaiLam.Value).ToInt32()
        };
    }

    public async Task<DataTable> GetDeNgauNhienAsync()
    {
        return await ExecuteRefCursorAsync(
            "PROC_MP_DE_NGAU_NHIEN",
            new OracleParameter("P_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output)
        );
    }

    public async Task<OracleExecResultDto> ChamDeNgauNhienAsync(string selectedIdsText, string flagsText)
    {
        using var conn = CreateConnection();
        using var cmd = new OracleCommand("PROC_MP_CHAM_NGAU_NHIEN", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        cmd.Parameters.Add("P_SELECTED_IDS", OracleDbType.Clob).Value = selectedIdsText ?? string.Empty;
        cmd.Parameters.Add("P_FLAGS_TEXT", OracleDbType.Clob).Value = flagsText ?? string.Empty;

        var pTongDiem = new OracleParameter("O_TONGDIEM", OracleDbType.Int32, ParameterDirection.Output);
        var pDat = new OracleParameter("O_DAT", OracleDbType.Int32, ParameterDirection.Output);

        cmd.Parameters.Add(pTongDiem);
        cmd.Parameters.Add(pDat);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();

        return new OracleExecResultDto
        {
            TongDiem = ((OracleDecimal)pTongDiem.Value).ToInt32(),
            Dat = ((OracleDecimal)pDat.Value).ToInt32() == 1
        };
    }
}