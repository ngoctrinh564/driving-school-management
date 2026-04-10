using driving_school_management.Models;
using driving_school_management.ViewModels;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace driving_school_management.Services
{
    public interface IAdminKetQuaHocTapService
    {
        Task<List<AdminKetQuaHocTapViewModel>> GetAllAsync();
        Task UpdateKetQuaHocTapAsync(AdminKetQuaHocTapRequest request);
        Task UpdateChiTietKetQuaHocTapAsync(AdminChiTietKetQuaHocTapRequest request);
    }

    public class AdminKetQuaHocTapService : IAdminKetQuaHocTapService
    {
        private readonly string _connectionString;

        public AdminKetQuaHocTapService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDb")
                ?? throw new ArgumentNullException(nameof(configuration), "Không tìm thấy chuỗi kết nối OracleDb");
        }

        public async Task<List<AdminKetQuaHocTapViewModel>> GetAllAsync()
        {
            var result = new List<AdminKetQuaHocTapViewModel>();

            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"SELECT * FROM VW_ADMIN_KETQUAHOCTAP ORDER BY KETQUAHOCTAPID";

            await using var command = new OracleCommand(sql, connection)
            {
                CommandType = CommandType.Text
            };

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var item = new AdminKetQuaHocTapViewModel
                {
                    KetQuaHocTapId = GetIntValue(reader, "KETQUAHOCTAPID"),
                    HoTenHocVien = GetStringValue(reader, "HOTENHOCVIEN") ?? string.Empty,
                    TenHoSo = GetStringValue(reader, "TENHOSO") ?? string.Empty,

                    SoBuoiHoc = GetIntValue(reader, "SOBUOIHOC"),
                    SoBuoiToiThieu = GetIntValue(reader, "SOBUOITOITHIEU"),

                    SoKmHoanThanh = GetDecimalValue(reader, "SOKMHOANTHANH"),
                    KmToiThieu = GetDecimalValue(reader, "KMTOITHIEU"),

                    NhanXet = GetStringValue(reader, "NHANXET"),

                    DuDieuKienThiTotNghiep = GetBoolValue(reader, "DU_DK_THITOTNGHIEP"),
                    DauTotNghiep = GetBoolValue(reader, "DAUTOTNGHIEP"),
                    DuDieuKienThiSatHach = GetBoolValue(reader, "DU_DK_THISATHACH"),

                    LyThuyetKq = GetBoolValue(reader, "LYTHUYETKQ"),
                    SaHinhKq = GetBoolValue(reader, "SAHINHKQ"),
                    DuongTruongKq = GetBoolValue(reader, "DUONGTRUONGKQ"),
                    MoPhongKq = GetBoolValue(reader, "MOPHONGKQ")
                };

                result.Add(item);
            }

            return result;
        }

        public async Task UpdateKetQuaHocTapAsync(AdminKetQuaHocTapRequest request)
        {
            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new OracleCommand("PKG_KETQUAHOCTAP_ADMIN.PRC_CAPNHAT_KETQUAHOCTAP", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("P_KETQUAHOCTAPID", OracleDbType.Int32).Value = request.KetQuaHocTapId;
            command.Parameters.Add("P_SOBUOIHOC", OracleDbType.Int32).Value = request.SoBuoiHoc;
            command.Parameters.Add("P_SOKMHOANTHANH", OracleDbType.Decimal).Value = request.SoKmHoanThanh;
            command.Parameters.Add("P_NHANXET", OracleDbType.NVarchar2).Value =
                string.IsNullOrWhiteSpace(request.NhanXet) ? DBNull.Value : request.NhanXet;

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateChiTietKetQuaHocTapAsync(AdminChiTietKetQuaHocTapRequest request)
        {
            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new OracleCommand("PKG_KETQUAHOCTAP_ADMIN.PRC_CAPNHAT_CHITIET_KQHT", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("P_KETQUAHOCTAPID", OracleDbType.Int32).Value = request.KetQuaHocTapId;
            command.Parameters.Add("P_LYTHUYETKQ", OracleDbType.Int16).Value = request.LyThuyetKq;
            command.Parameters.Add("P_SAHINHKQ", OracleDbType.Int16).Value = request.SaHinhKq;
            command.Parameters.Add("P_DUONGTRUONGKQ", OracleDbType.Int16).Value = request.DuongTruongKq;
            command.Parameters.Add("P_MOPHONGKQ", OracleDbType.Int16).Value = request.MoPhongKq;

            await command.ExecuteNonQueryAsync();
        }

        private static int GetIntValue(OracleDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return 0;

            object value = reader.GetValue(ordinal);

            if (value is OracleDecimal oracleDecimal)
                return oracleDecimal.ToInt32();

            if (value is decimal decimalValue)
                return decimal.ToInt32(decimalValue);

            return Convert.ToInt32(value);
        }

        private static decimal GetDecimalValue(OracleDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return 0;

            object value = reader.GetValue(ordinal);

            if (value is OracleDecimal oracleDecimal)
                return oracleDecimal.Value;

            if (value is decimal decimalValue)
                return decimalValue;

            return Convert.ToDecimal(value);
        }

        private static bool GetBoolValue(OracleDataReader reader, string columnName)
        {
            return GetIntValue(reader, columnName) == 1;
        }

        private static string? GetStringValue(OracleDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal).ToString();
        }
    }

    public class AdminKetQuaHocTapRequest
    {
        public int KetQuaHocTapId { get; set; }
        public int SoBuoiHoc { get; set; }
        public decimal SoKmHoanThanh { get; set; }
        public string? NhanXet { get; set; }
    }

    public class AdminChiTietKetQuaHocTapRequest
    {
        public int KetQuaHocTapId { get; set; }
        public int LyThuyetKq { get; set; }
        public int SaHinhKq { get; set; }
        public int DuongTruongKq { get; set; }
        public int MoPhongKq { get; set; }
    }
}