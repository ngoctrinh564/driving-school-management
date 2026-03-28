using driving_school_management.Models.DTOs;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

public class HoSoService
{
    private readonly string _conn;
    private readonly IWebHostEnvironment _env;

    public HoSoService(IConfiguration config, IWebHostEnvironment env)
    {
        _conn = config.GetConnectionString("OracleDb");
        _env = env;
    }

    private static int GetInt32(object value)
    {
        if (value == DBNull.Value)
            throw new InvalidOperationException("Giá trị NULL không thể chuyển sang int.");

        if (value is OracleDecimal oracleDecimal)
            return oracleDecimal.ToInt32();

        if (value is decimal dec)
            return decimal.ToInt32(dec);

        if (value is int intVal)
            return intVal;

        return Convert.ToInt32(value);
    }

    private static int? GetNullableInt32(object value)
    {
        if (value == DBNull.Value || value == null)
            return null;

        if (value is OracleDecimal oracleDecimal)
            return oracleDecimal.ToInt32();

        if (value is decimal dec)
            return decimal.ToInt32(dec);

        if (value is int intVal)
            return intVal;

        return Convert.ToInt32(value);
    }

    private static decimal? GetNullableDecimal(object value)
    {
        if (value == DBNull.Value || value == null)
            return null;

        if (value is OracleDecimal oracleDecimal)
            return oracleDecimal.Value;

        if (value is decimal dec)
            return dec;

        return Convert.ToDecimal(value);
    }

    private static DateTime? GetNullableDate(object value)
    {
        return value == DBNull.Value ? null : (DateTime?)value;
    }

    private static string? GetNullableString(object value)
    {
        return value == DBNull.Value ? null : value?.ToString();
    }

    public List<MyHoSoCardDto> GetMyHoSo(int userId)
    {
        var list = new List<MyHoSoCardDto>();

        using (var conn = new OracleConnection(_conn))
        {
            conn.Open();

            using (var cmd = new OracleCommand("SP_GET_MY_HOSO", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new MyHoSoCardDto
                        {
                            HoSoId = GetInt32(reader["hoSoId"]),
                            HoTen = GetNullableString(reader["hoTen"]),
                            AvatarUrl = GetNullableString(reader["avatarUrl"]),
                            TenHoSo = GetNullableString(reader["tenHoSo"]),
                            TenHang = GetNullableString(reader["tenHang"]),
                            NgayDangKy = Convert.ToDateTime(reader["ngayDangKy"]),
                            TrangThai = GetNullableString(reader["trangThai"]),
                            SoThangConLai = GetInt32(reader["soThangConLai"]),
                            SoNgayConLai = GetInt32(reader["soNgayConLai"])
                        });
                    }
                }
            }
        }

        return list;
    }

    public HoSoDetailDto? GetDetailByUser(int hoSoId, int userId)
    {
        HoSoDetailDto? result = null;

        using (var conn = new OracleConnection(_conn))
        {
            conn.Open();

            using (var cmd = new OracleCommand("SP_GET_MY_HOSO_DETAIL", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_hoSoId", OracleDbType.Int32).Value = hoSoId;
                cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
                cmd.Parameters.Add("p_info", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("p_images", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                using (var reader = ((OracleRefCursor)cmd.Parameters["p_info"].Value).GetDataReader())
                {
                    if (reader.Read())
                    {
                        result = new HoSoDetailDto
                        {
                            HoSoId = GetInt32(reader["hoSoId"]),
                            HoTen = GetNullableString(reader["hoTen"]),
                            SoCmndCccd = GetNullableString(reader["soCmndCccd"]),
                            NamSinh = GetNullableDate(reader["namSinh"]),
                            GioiTinh = GetNullableString(reader["gioiTinh"]),
                            Sdt = GetNullableString(reader["sdt"]),
                            Email = GetNullableString(reader["email"]),
                            AvatarUrl = GetNullableString(reader["avatarUrl"]),
                            TenHoSo = GetNullableString(reader["tenHoSo"]),
                            LoaiHoSo = GetNullableString(reader["loaiHoSo"]),
                            NgayDangKy = GetNullableDate(reader["ngayDangKy"]),
                            TrangThai = GetNullableString(reader["trangThai"]),
                            GhiChu = GetNullableString(reader["ghiChu"]),
                            TenHang = GetNullableString(reader["tenHang"]),
                            HieuLuc = GetNullableString(reader["hieuLuc"]),
                            ThoiHan = GetNullableDate(reader["thoiHan"]),
                            KhamMat = GetNullableString(reader["khamMat"]),
                            HuyetAp = GetNullableString(reader["huyetAp"]),
                            ChieuCao = GetNullableDecimal(reader["chieuCao"]),
                            CanNang = GetNullableDecimal(reader["canNang"])
                        };
                    }
                }

                if (result != null)
                {
                    using (var reader = ((OracleRefCursor)cmd.Parameters["p_images"].Value).GetDataReader())
                    {
                        while (reader.Read())
                        {
                            var urlAnh = GetNullableString(reader["urlAnh"]);
                            if (!string.IsNullOrWhiteSpace(urlAnh))
                            {
                                result.Images.Add(urlAnh);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    public List<HangGplxOptionDto> GetHangGplxOptions()
    {
        var list = new List<HangGplxOptionDto>();

        using (var conn = new OracleConnection(_conn))
        {
            conn.Open();

            using (var cmd = new OracleCommand("SP_GET_HANG_GPLX", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new HangGplxOptionDto
                        {
                            HangId = GetInt32(reader["hangId"]),
                            MaHang = GetNullableString(reader["maHang"]) ?? string.Empty,
                            TenHang = GetNullableString(reader["tenHang"]) ?? string.Empty
                        });
                    }
                }
            }
        }

        return list;
    }

    public string GetHocVienNameByUserId(int userId)
    {
        using (var conn = new OracleConnection(_conn))
        {
            conn.Open();

            using (var cmd = new OracleCommand("SP_GET_HOCVIEN_NAME_BY_USER", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
                cmd.Parameters.Add("p_hoTen", OracleDbType.NVarchar2, 200).Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                return cmd.Parameters["p_hoTen"].Value == DBNull.Value
                    ? string.Empty
                    : cmd.Parameters["p_hoTen"].Value?.ToString() ?? string.Empty;
            }
        }
    }

    public (string AvatarUrl, bool CanCreate, List<string> MissingFields) CheckCreateHoSoCondition(int userId)
    {
        string avatarUrl = string.Empty;
        bool canCreate = false;
        var missingFields = new List<string>();

        using (var conn = new OracleConnection(_conn))
        {
            conn.Open();

            using (var cmd = new OracleCommand("SP_CHECK_CREATE_HOSO_CONDITION", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
                cmd.Parameters.Add("p_avatarUrl", OracleDbType.NVarchar2, 1000).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("p_canCreate", OracleDbType.Int32).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("p_missingFields", OracleDbType.NVarchar2, 2000).Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                avatarUrl = cmd.Parameters["p_avatarUrl"].Value == DBNull.Value
                    ? string.Empty
                    : cmd.Parameters["p_avatarUrl"].Value?.ToString() ?? string.Empty;

                if (cmd.Parameters["p_canCreate"].Value != DBNull.Value)
                {
                    canCreate = ((OracleDecimal)cmd.Parameters["p_canCreate"].Value).ToInt32() == 1;
                }

                var rawMissing = cmd.Parameters["p_missingFields"].Value == DBNull.Value
                    ? string.Empty
                    : cmd.Parameters["p_missingFields"].Value?.ToString() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(rawMissing))
                {
                    missingFields = rawMissing
                        .Split('|', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();
                }
            }
        }

        return (avatarUrl, canCreate, missingFields);
    }

    public MyHoSoPageDto GetMyHoSoPage(int userId)
    {
        var result = new MyHoSoPageDto();

        result.Cards = GetMyHoSo(userId);

        foreach (var card in result.Cards)
        {
            var detail = GetDetailByUser(card.HoSoId, userId);
            if (detail != null)
            {
                detail.SoThangConLai = card.SoThangConLai;
                detail.SoNgayConLai = card.SoNgayConLai;

                if (string.IsNullOrWhiteSpace(detail.AvatarUrl))
                {
                    detail.AvatarUrl = "/images/avatar/default.png";
                }

                result.Details.Add(detail);
            }
        }

        var createCondition = CheckCreateHoSoCondition(userId);

        result.HocVienAvatarUrl = string.IsNullOrWhiteSpace(createCondition.AvatarUrl)
            ? "/images/avatar/default.png"
            : createCondition.AvatarUrl;

        result.CanCreateHoSo = createCondition.CanCreate;
        result.MissingFields = createCondition.MissingFields;

        return result;
    }

    public async Task<CreateHoSoResultDto> CreateHoSoAsync(int userId, CreateHoSoDto model)
    {
        var result = new CreateHoSoResultDto();

        using (var conn = new OracleConnection(_conn))
        {
            await conn.OpenAsync();

            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    int? hoSoId = null;
                    int? khamSucKhoeId = null;
                    string message = string.Empty;

                    using (var cmd = new OracleCommand("SP_CREATE_HOSO", conn))
                    {
                        cmd.Transaction = transaction;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
                        cmd.Parameters.Add("p_hangId", OracleDbType.Int32).Value = model.HangId;
                        cmd.Parameters.Add("p_loaiHoSo", OracleDbType.NVarchar2).Value = model.LoaiHoSo;
                        cmd.Parameters.Add("p_ghiChu", OracleDbType.NVarchar2).Value =
                            string.IsNullOrWhiteSpace(model.GhiChu) ? DBNull.Value : model.GhiChu;

                        cmd.Parameters.Add("p_hieuLuc", OracleDbType.NVarchar2).Value =
                            string.IsNullOrWhiteSpace(model.HieuLuc) ? DBNull.Value : model.HieuLuc;
                        cmd.Parameters.Add("p_thoiHan", OracleDbType.Date).Value =
                            model.ThoiHan.HasValue ? model.ThoiHan.Value : DBNull.Value;
                        cmd.Parameters.Add("p_khamMat", OracleDbType.NVarchar2).Value =
                            string.IsNullOrWhiteSpace(model.KhamMat) ? DBNull.Value : model.KhamMat;
                        cmd.Parameters.Add("p_huyetAp", OracleDbType.NVarchar2).Value =
                            string.IsNullOrWhiteSpace(model.HuyetAp) ? DBNull.Value : model.HuyetAp;
                        cmd.Parameters.Add("p_chieuCao", OracleDbType.Decimal).Value =
                            model.ChieuCao.HasValue ? model.ChieuCao.Value : DBNull.Value;
                        cmd.Parameters.Add("p_canNang", OracleDbType.Decimal).Value =
                            model.CanNang.HasValue ? model.CanNang.Value : DBNull.Value;

                        cmd.Parameters.Add("p_hoSoId", OracleDbType.Int32).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("p_khamSucKhoeId", OracleDbType.Int32).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("p_message", OracleDbType.NVarchar2, 500).Direction = ParameterDirection.Output;

                        await cmd.ExecuteNonQueryAsync();

                        message = cmd.Parameters["p_message"].Value?.ToString() ?? string.Empty;

                        var hoSoParam = cmd.Parameters["p_hoSoId"].Value;
                        if (hoSoParam != null && hoSoParam != DBNull.Value)
                        {
                            if (hoSoParam is OracleDecimal odHoSo && !odHoSo.IsNull)
                            {
                                hoSoId = odHoSo.ToInt32();
                            }
                            else if (hoSoParam is decimal decHoSo)
                            {
                                hoSoId = decimal.ToInt32(decHoSo);
                            }
                        }

                        var khamParam = cmd.Parameters["p_khamSucKhoeId"].Value;
                        if (khamParam != null && khamParam != DBNull.Value)
                        {
                            if (khamParam is OracleDecimal odKham && !odKham.IsNull)
                            {
                                khamSucKhoeId = odKham.ToInt32();
                            }
                            else if (khamParam is decimal decKham)
                            {
                                khamSucKhoeId = decimal.ToInt32(decKham);
                            }
                        }
                    }

                    if (!hoSoId.HasValue || !khamSucKhoeId.HasValue)
                    {
                        transaction.Rollback();
                        result.Success = false;
                        result.Message = message;
                        return result;
                    }

                    if (model.AnhGkskFiles != null && model.AnhGkskFiles.Count > 0)
                    {
                        var rootFolder = Path.Combine(_env.WebRootPath, "images", "healths");
                        if (!Directory.Exists(rootFolder))
                        {
                            Directory.CreateDirectory(rootFolder);
                        }

                        foreach (var file in model.AnhGkskFiles)
                        {
                            if (file == null || file.Length <= 0)
                            {
                                continue;
                            }

                            var ext = Path.GetExtension(file.FileName);
                            var fileName = $"{Guid.NewGuid():N}{ext}";
                            var fullPath = Path.Combine(rootFolder, fileName);
                            var dbUrl = $"/images/healths/{fileName}";

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            using (var cmdImg = new OracleCommand("SP_ADD_ANH_GKSK", conn))
                            {
                                cmdImg.Transaction = transaction;
                                cmdImg.CommandType = CommandType.StoredProcedure;

                                cmdImg.Parameters.Add("p_khamSucKhoeId", OracleDbType.Int32).Value = khamSucKhoeId.Value;
                                cmdImg.Parameters.Add("p_urlAnh", OracleDbType.NVarchar2).Value = dbUrl;
                                cmdImg.Parameters.Add("p_message", OracleDbType.NVarchar2, 200).Direction = ParameterDirection.Output;

                                await cmdImg.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    transaction.Commit();

                    result.Success = true;
                    result.HoSoId = hoSoId;
                    result.KhamSucKhoeId = khamSucKhoeId;
                    result.Message = message;
                    return result;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}