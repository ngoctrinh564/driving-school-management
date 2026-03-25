using driving_school_management.Models.DTOs;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace driving_school_management.Services
{
    public class PaymentService
    {
        private readonly string _connectionString;

        public PaymentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDb");
        }

        public PaymentStartDto? StartPayment(int userId, int hoSoId, int khoaHocId)
        {
            PaymentStartDto? result = null;

            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_PAYMENT_START", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
                cmd.Parameters.Add("p_hoSoId", OracleDbType.Int32).Value = hoSoId;
                cmd.Parameters.Add("p_khoaHocId", OracleDbType.Int32).Value = khoaHocId;
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new PaymentStartDto
                        {
                            IsValid = GetInt32Value(reader["isValid"]),
                            Message = reader["message"] == DBNull.Value ? string.Empty : reader["message"].ToString()!,
                            PhieuId = GetInt32Value(reader["phieuId"]),
                            KhoaHocId = GetInt32Value(reader["khoaHocId"]),
                            TenKhoaHoc = reader["tenKhoaHoc"] == DBNull.Value ? string.Empty : reader["tenKhoaHoc"].ToString()!,
                            HoSoId = GetInt32Value(reader["hoSoId"]),
                            HoTenHocVien = reader["hoTenHocVien"] == DBNull.Value ? string.Empty : reader["hoTenHocVien"].ToString()!,
                            TenHang = reader["tenHang"] == DBNull.Value ? string.Empty : reader["tenHang"].ToString()!,
                            SoTien = GetDecimalValue(reader["soTien"]),
                            NgayLap = reader["ngayLap"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayLap"]),
                            NoiDungMacDinh = reader["noiDungMacDinh"] == DBNull.Value ? string.Empty : reader["noiDungMacDinh"].ToString()!,
                            PhuongThuc = reader["phuongThuc"] == DBNull.Value ? string.Empty : reader["phuongThuc"].ToString()!
                        };
                    }
                }
            }

            return result;
        }

        public int ChoosePaymentMethod(int userId, int phieuId, string method, string noiDung)
        {
            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_PAYMENT_CHOOSE_METHOD", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
                cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;
                cmd.Parameters.Add("p_method", OracleDbType.NVarchar2).Value = method;
                cmd.Parameters.Add("p_noiDung", OracleDbType.NVarchar2).Value =
                    string.IsNullOrWhiteSpace(noiDung) ? DBNull.Value : noiDung;

                var resultParam = cmd.Parameters.Add("o_result", OracleDbType.Int32);
                resultParam.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();

                return GetInt32Value(resultParam.Value);
            }
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
        // ============================================================
        // THANH TOÁN VNPAY
        // ============================================================
        public PaymentVnPayDto? GetVnPayInfo(int userId, int phieuId)
        {
            PaymentVnPayDto? result = null;

            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_PAYMENT_GET_VNPAY_INFO", conn))
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
                        result = new PaymentVnPayDto
                        {
                            PhieuId = GetInt32Value(reader["phieuId"]),
                            TenPhieu = reader["tenPhieu"] == DBNull.Value ? string.Empty : reader["tenPhieu"].ToString()!,
                            NgayLap = reader["ngayLap"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayLap"]),
                            TongTien = GetDecimalValue(reader["tongTien"]),
                            NgayNop = reader["ngayNop"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayNop"]),
                            PhuongThuc = reader["phuongThuc"] == DBNull.Value ? string.Empty : reader["phuongThuc"].ToString()!,

                            HoSoId = GetInt32Value(reader["hoSoId"]),
                            GhiChu = reader["ghiChu"] == DBNull.Value ? string.Empty : reader["ghiChu"].ToString()!,
                            TenHoSo = reader["tenHoSo"] == DBNull.Value ? string.Empty : reader["tenHoSo"].ToString()!,
                            HoTenHocVien = reader["hoTenHocVien"] == DBNull.Value ? string.Empty : reader["hoTenHocVien"].ToString()!,

                            KhoaHocId = GetInt32Value(reader["khoaHocId"]),
                            TenKhoaHoc = reader["tenKhoaHoc"] == DBNull.Value ? string.Empty : reader["tenKhoaHoc"].ToString()!,
                            TenHang = reader["tenHang"] == DBNull.Value ? string.Empty : reader["tenHang"].ToString()!
                        };
                    }
                }
            }

            return result;
        }

        public int MarkVnPaySuccess(int phieuId)
        {
            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_PAYMENT_VNPAY_SUCCESS", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;

                var output = cmd.Parameters.Add("o_result", OracleDbType.Int32);
                output.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();

                return GetInt32Value(output.Value);
            }
        }

        public int MarkVnPayFail(int phieuId)
        {
            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_PAYMENT_VNPAY_FAIL", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;

                var output = cmd.Parameters.Add("o_result", OracleDbType.Int32);
                output.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();

                return GetInt32Value(output.Value);
            }
        }
        // ============================================================
        // THANH TOÁN PAYPAL
        // ============================================================
        public PaymentGatewayDto? GetPayPalInfo(int userId, int phieuId)
        {
            PaymentGatewayDto? result = null;

            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_PAYMENT_GET_PAYPAL_INFO", conn))
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
                        result = new PaymentGatewayDto
                        {
                            PhieuId = GetInt32Value(reader["phieuId"]),
                            TenPhieu = reader["tenPhieu"] == DBNull.Value ? string.Empty : reader["tenPhieu"].ToString()!,
                            NgayLap = reader["ngayLap"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayLap"]),
                            TongTien = GetDecimalValue(reader["tongTien"]),
                            NgayNop = reader["ngayNop"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayNop"]),
                            PhuongThuc = reader["phuongThuc"] == DBNull.Value ? string.Empty : reader["phuongThuc"].ToString()!,
                            HoSoId = GetInt32Value(reader["hoSoId"]),
                            GhiChu = reader["ghiChu"] == DBNull.Value ? string.Empty : reader["ghiChu"].ToString()!,
                            TenHoSo = reader["tenHoSo"] == DBNull.Value ? string.Empty : reader["tenHoSo"].ToString()!,
                            HoTenHocVien = reader["hoTenHocVien"] == DBNull.Value ? string.Empty : reader["hoTenHocVien"].ToString()!,
                            KhoaHocId = GetInt32Value(reader["khoaHocId"]),
                            TenKhoaHoc = reader["tenKhoaHoc"] == DBNull.Value ? string.Empty : reader["tenKhoaHoc"].ToString()!,
                            TenHang = reader["tenHang"] == DBNull.Value ? string.Empty : reader["tenHang"].ToString()!
                        };
                    }
                }
            }

            return result;
        }
        public int MarkPayPalSuccess(int phieuId)
        {
            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_PAYMENT_PAYPAL_SUCCESS", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;

                var output = cmd.Parameters.Add("o_result", OracleDbType.Int32);
                output.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();

                return GetInt32Value(output.Value);
            }
        }
        public int MarkPayPalFail(int phieuId)
        {
            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_PAYMENT_PAYPAL_FAIL", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;

                var output = cmd.Parameters.Add("o_result", OracleDbType.Int32);
                output.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();

                return GetInt32Value(output.Value);
            }
        }
        public PaymentGatewayDto? GetPayPalInfoFromPhieuIdForReturn(int phieuId)
        {
            PaymentGatewayDto? result = null;

            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand("SP_PAYMENT_GET_PAYPAL_INFO_BY_PHIEU", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;
                cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new PaymentGatewayDto
                        {
                            PhieuId = GetInt32Value(reader["phieuId"]),
                            TenPhieu = reader["tenPhieu"] == DBNull.Value ? string.Empty : reader["tenPhieu"].ToString()!,
                            NgayLap = reader["ngayLap"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayLap"]),
                            TongTien = GetDecimalValue(reader["tongTien"]),
                            NgayNop = reader["ngayNop"] == DBNull.Value ? null : Convert.ToDateTime(reader["ngayNop"]),
                            PhuongThuc = reader["phuongThuc"] == DBNull.Value ? string.Empty : reader["phuongThuc"].ToString()!,
                            HoSoId = GetInt32Value(reader["hoSoId"]),
                            GhiChu = reader["ghiChu"] == DBNull.Value ? string.Empty : reader["ghiChu"].ToString()!,
                            TenHoSo = reader["tenHoSo"] == DBNull.Value ? string.Empty : reader["tenHoSo"].ToString()!,
                            HoTenHocVien = reader["hoTenHocVien"] == DBNull.Value ? string.Empty : reader["hoTenHocVien"].ToString()!,
                            KhoaHocId = GetInt32Value(reader["khoaHocId"]),
                            TenKhoaHoc = reader["tenKhoaHoc"] == DBNull.Value ? string.Empty : reader["tenKhoaHoc"].ToString()!,
                            TenHang = reader["tenHang"] == DBNull.Value ? string.Empty : reader["tenHang"].ToString()!
                        };
                    }
                }
            }

            return result;
        }
    }
}