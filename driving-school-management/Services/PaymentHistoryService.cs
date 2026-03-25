using driving_school_management.Models.DTOs;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace driving_school_management.Services
{
    public class PaymentHistoryService
    {
        private readonly string _connectionString;

        public PaymentHistoryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDb");
        }

        public List<PaymentHistoryDto> GetPaymentHistoryByUser(int userId)
        {
            var result = new List<PaymentHistoryDto>();

            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_PAYMENT_HISTORY_BY_USER", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new PaymentHistoryDto
                        {
                            PhieuId = GetInt32Value(reader["phieuId"]),
                            TenPhieu = GetStringValue(reader["tenPhieu"]),
                            NgayLap = GetNullableDateTimeValue(reader["ngayLap"]),
                            NgayNop = GetNullableDateTimeValue(reader["ngayNop"]),
                            TongTien = GetDecimalValue(reader["tongTien"]),
                            PhuongThuc = GetStringValue(reader["phuongThuc"]),
                            LoaiPhi = GetStringValue(reader["loaiPhi"]),
                            GhiChu = GetStringValue(reader["ghiChu"]),
                            HoSoId = GetInt32Value(reader["hoSoId"]),
                            TenHoSo = GetStringValue(reader["tenHoSo"]),
                            HoTenHocVien = GetStringValue(reader["hoTenHocVien"]),
                            KhoaHocId = GetInt32Value(reader["khoaHocId"]),
                            TenKhoaHoc = GetStringValue(reader["tenKhoaHoc"]),
                            TenHang = GetStringValue(reader["tenHang"]),
                            TrangThaiThanhToan = GetStringValue(reader["trangThaiThanhToan"]),
                            CoTheTaiHoaDon = GetInt32Value(reader["coTheTaiHoaDon"])
                        });
                    }
                }
            }

            return result;
        }

        public PaymentHistoryDetailDto? GetPaymentHistoryDetail(int userId, int phieuId)
        {
            PaymentHistoryDetailDto? result = null;

            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_PAYMENT_HISTORY_DETAIL", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
                cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new PaymentHistoryDetailDto
                        {
                            PhieuId = GetInt32Value(reader["phieuId"]),
                            TenPhieu = GetStringValue(reader["tenPhieu"]),
                            NgayLap = GetNullableDateTimeValue(reader["ngayLap"]),
                            NgayNop = GetNullableDateTimeValue(reader["ngayNop"]),
                            TongTien = GetDecimalValue(reader["tongTien"]),
                            PhuongThuc = GetStringValue(reader["phuongThuc"]),
                            LoaiPhi = GetStringValue(reader["loaiPhi"]),
                            GhiChu = GetStringValue(reader["ghiChu"]),
                            TrangThaiThanhToan = GetStringValue(reader["trangThaiThanhToan"]),

                            HoSoId = GetInt32Value(reader["hoSoId"]),
                            TenHoSo = GetStringValue(reader["tenHoSo"]),
                            TrangThaiHoSo = GetStringValue(reader["trangThaiHoSo"]),

                            HocVienId = GetInt32Value(reader["hocVienId"]),
                            HoTenHocVien = GetStringValue(reader["hoTenHocVien"]),
                            Sdt = GetStringValue(reader["sdt"]),
                            Email = GetStringValue(reader["email"]),

                            KhoaHocId = GetInt32Value(reader["khoaHocId"]),
                            TenKhoaHoc = GetStringValue(reader["tenKhoaHoc"]),
                            DiaDiem = GetStringValue(reader["diaDiem"]),
                            NgayBatDau = GetNullableDateTimeValue(reader["ngayBatDau"]),
                            NgayKetThuc = GetNullableDateTimeValue(reader["ngayKetThuc"]),
                            TrangThaiKhoaHoc = GetStringValue(reader["trangThaiKhoaHoc"]),

                            HangId = GetInt32Value(reader["hangId"]),
                            TenHang = GetStringValue(reader["tenHang"]),
                            LoaiPhuongTien = GetStringValue(reader["loaiPhuongTien"]),
                            HocPhi = GetDecimalValue(reader["hocPhi"])
                        };
                    }
                }
            }

            return result;
        }

        private int GetInt32Value(object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value) return 0;

            if (dbValue is OracleDecimal oracleDecimal)
                return oracleDecimal.ToInt32();

            if (dbValue is decimal decValue)
                return decimal.ToInt32(decValue);

            if (dbValue is int intValue)
                return intValue;

            return int.Parse(dbValue.ToString()!);
        }

        private decimal GetDecimalValue(object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value) return 0;

            if (dbValue is OracleDecimal oracleDecimal)
                return oracleDecimal.Value;

            if (dbValue is decimal decValue)
                return decValue;

            return decimal.Parse(dbValue.ToString()!);
        }

        private string GetStringValue(object dbValue)
        {
            return dbValue == null || dbValue == DBNull.Value ? string.Empty : dbValue.ToString()!;
        }

        private DateTime? GetNullableDateTimeValue(object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value) return null;
            return Convert.ToDateTime(dbValue);
        }
    }
}