using driving_school_management.Models.DTOs;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Globalization;

namespace driving_school_management.Services
{
    public class ExamPaymentService
    {
        private readonly string _connectionString;

        public ExamPaymentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDb");
        }

        public ExamPaymentStartDto StartExamPayment(int userId, int kyThiId)
        {
            var result = new ExamPaymentStartDto();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_EXAM_PAYMENT.START_PAYMENT_EXAM", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_kyThiId", OracleDbType.Int32).Value = kyThiId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var item = new PaymentGatewayDto
                {
                    PhieuId = GetInt32Value(reader["PHIEUID"]),
                    TenPhieu = GetStringValue(reader["TENPHIEU"]),
                    NgayLap = GetDateTimeValue(reader["NGAYLAP"]),
                    TongTien = GetDecimalValue(reader["TONGTIEN"]),
                    NgayNop = GetDateTimeValue(reader["NGAYNOP"]),
                    PhuongThuc = GetStringValue(reader["PHUONGTHUC"]),
                    HoSoId = GetInt32Value(reader["HOSOID"]),
                    GhiChu = GetStringValue(reader["GHICHU"]),
                    TenHoSo = GetStringValue(reader["TENHOSO"]),
                    HoTenHocVien = GetStringValue(reader["HOTENHOCVIEN"]),
                    KhoaHocId = 0,
                    TenKhoaHoc = NormalizeText(GetStringValue(reader["TENKYTHI"])),
                    TenHang = NormalizeText(GetStringValue(reader["TENHANG"]))
                };

                result.PhieuList.Add(item);
                result.TongTien += item.TongTien;
            }

            return result;
        }

        private string NormalizeText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return System.Text.RegularExpressions.Regex.Replace(value.Trim(), @"\s+", " ");
        }

        public int ChoosePaymentMethodForMany(int userId, List<int> phieuIds, string method, string noiDung)
        {
            if (phieuIds == null || phieuIds.Count == 0)
                return 0;

            int successCount = 0;

            using var conn = new OracleConnection(_connectionString);
            conn.Open();

            foreach (var phieuId in phieuIds.Distinct())
            {
                using var cmd = new OracleCommand("SP_PAYMENT_CHOOSE_METHOD", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;

                cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
                cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;
                cmd.Parameters.Add("p_method", OracleDbType.NVarchar2).Value = method;
                cmd.Parameters.Add("p_noiDung", OracleDbType.NVarchar2).Value =
                    string.IsNullOrWhiteSpace(noiDung) ? DBNull.Value : noiDung;

                var output = cmd.Parameters.Add("o_result", OracleDbType.Int32);
                output.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                if (GetInt32Value(output.Value) == 1)
                    successCount++;
            }

            return successCount;
        }

        // =========================
        // VNPAY
        // =========================
        public List<PaymentGatewayDto> GetVnPayExamInfo(int userId, List<int> phieuIds)
        {
            var result = new List<PaymentGatewayDto>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_EXAM_PAYMENT.GET_VNPAY_EXAM_INFO", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_phieuIds", OracleDbType.NVarchar2).Value = string.Join(",", phieuIds.Distinct());
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PaymentGatewayDto
                {
                    PhieuId = GetInt32Value(reader["PHIEUID"]),
                    TenPhieu = GetStringValue(reader["TENPHIEU"]),
                    NgayLap = GetDateTimeValue(reader["NGAYLAP"]),
                    TongTien = GetDecimalValue(reader["TONGTIEN"]),
                    NgayNop = GetDateTimeValue(reader["NGAYNOP"]),
                    PhuongThuc = GetStringValue(reader["PHUONGTHUC"]),
                    HoSoId = GetInt32Value(reader["HOSOID"]),
                    GhiChu = GetStringValue(reader["GHICHU"]),
                    TenHoSo = GetStringValue(reader["TENHOSO"]),
                    HoTenHocVien = GetStringValue(reader["HOTENHOCVIEN"]),
                    KhoaHocId = 0,
                    TenKhoaHoc = GetStringValue(reader["TENKYTHI"]),
                    TenHang = GetStringValue(reader["TENHANG"])
                });
            }

            return result;
        }

        public int MarkVnPayExamSuccess(List<int> phieuIds)
        {
            if (phieuIds == null || phieuIds.Count == 0)
                return 0;

            int successCount = 0;

            using var conn = new OracleConnection(_connectionString);
            conn.Open();

            foreach (var phieuId in phieuIds.Distinct())
            {
                using var cmd = new OracleCommand("PKG_EXAM_PAYMENT.MARK_EXAM_PAYMENT_SUCCESS", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;

                cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;
                cmd.Parameters.Add("p_method", OracleDbType.NVarchar2).Value = "VNPAY";

                var output = cmd.Parameters.Add("o_result", OracleDbType.Int32);
                output.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                if (GetInt32Value(output.Value) == 1)
                    successCount++;
            }

            return successCount;
        }

        // =========================
        // PAYPAL
        // =========================
        public List<PaymentGatewayDto> GetPayPalExamInfo(int userId, List<int> phieuIds)
        {
            var result = new List<PaymentGatewayDto>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_EXAM_PAYMENT.GET_PAYPAL_EXAM_INFO", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_phieuIds", OracleDbType.NVarchar2).Value = string.Join(",", phieuIds.Distinct());
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PaymentGatewayDto
                {
                    PhieuId = GetInt32Value(reader["PHIEUID"]),
                    TenPhieu = GetStringValue(reader["TENPHIEU"]),
                    NgayLap = GetDateTimeValue(reader["NGAYLAP"]),
                    TongTien = GetDecimalValue(reader["TONGTIEN"]),
                    NgayNop = GetDateTimeValue(reader["NGAYNOP"]),
                    PhuongThuc = GetStringValue(reader["PHUONGTHUC"]),
                    HoSoId = GetInt32Value(reader["HOSOID"]),
                    GhiChu = GetStringValue(reader["GHICHU"]),
                    TenHoSo = GetStringValue(reader["TENHOSO"]),
                    HoTenHocVien = GetStringValue(reader["HOTENHOCVIEN"]),
                    KhoaHocId = 0,
                    TenKhoaHoc = GetStringValue(reader["TENKYTHI"]),
                    TenHang = GetStringValue(reader["TENHANG"])
                });
            }

            return result;
        }

        public List<PaymentGatewayDto> GetPayPalExamInfoByPhieuIds(List<int> phieuIds)
        {
            var result = new List<PaymentGatewayDto>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_EXAM_PAYMENT.GET_PAYPAL_EXAM_INFO_BY_IDS", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_phieuIds", OracleDbType.NVarchar2).Value = string.Join(",", phieuIds.Distinct());
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PaymentGatewayDto
                {
                    PhieuId = GetInt32Value(reader["PHIEUID"]),
                    TenPhieu = GetStringValue(reader["TENPHIEU"]),
                    NgayLap = GetDateTimeValue(reader["NGAYLAP"]),
                    TongTien = GetDecimalValue(reader["TONGTIEN"]),
                    NgayNop = GetDateTimeValue(reader["NGAYNOP"]),
                    PhuongThuc = GetStringValue(reader["PHUONGTHUC"]),
                    HoSoId = GetInt32Value(reader["HOSOID"]),
                    GhiChu = GetStringValue(reader["GHICHU"]),
                    TenHoSo = GetStringValue(reader["TENHOSO"]),
                    HoTenHocVien = GetStringValue(reader["HOTENHOCVIEN"]),
                    KhoaHocId = 0,
                    TenKhoaHoc = GetStringValue(reader["TENKYTHI"]),
                    TenHang = GetStringValue(reader["TENHANG"])
                });
            }

            return result;
        }

        public int MarkPayPalExamSuccess(List<int> phieuIds)
        {
            if (phieuIds == null || phieuIds.Count == 0)
                return 0;

            int successCount = 0;

            using var conn = new OracleConnection(_connectionString);
            conn.Open();

            foreach (var phieuId in phieuIds.Distinct())
            {
                using var cmd = new OracleCommand("PKG_EXAM_PAYMENT.MARK_EXAM_PAYMENT_SUCCESS", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;

                cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;
                cmd.Parameters.Add("p_method", OracleDbType.NVarchar2).Value = "PAYPAL";

                var output = cmd.Parameters.Add("o_result", OracleDbType.Int32);
                output.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                if (GetInt32Value(output.Value) == 1)
                    successCount++;
            }

            return successCount;
        }

        // =========================
        // MOMO
        // =========================
        public List<PaymentGatewayDto> GetMomoExamInfo(int userId, List<int> phieuIds)
        {
            var result = new List<PaymentGatewayDto>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_EXAM_PAYMENT.GET_MOMO_EXAM_INFO", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_phieuIds", OracleDbType.NVarchar2).Value = string.Join(",", phieuIds.Distinct());
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PaymentGatewayDto
                {
                    PhieuId = GetInt32Value(reader["PHIEUID"]),
                    TenPhieu = GetStringValue(reader["TENPHIEU"]),
                    NgayLap = GetDateTimeValue(reader["NGAYLAP"]),
                    TongTien = GetDecimalValue(reader["TONGTIEN"]),
                    NgayNop = GetDateTimeValue(reader["NGAYNOP"]),
                    PhuongThuc = GetStringValue(reader["PHUONGTHUC"]),
                    HoSoId = GetInt32Value(reader["HOSOID"]),
                    GhiChu = GetStringValue(reader["GHICHU"]),
                    TenHoSo = GetStringValue(reader["TENHOSO"]),
                    HoTenHocVien = GetStringValue(reader["HOTENHOCVIEN"]),
                    KhoaHocId = 0,
                    TenKhoaHoc = GetStringValue(reader["TENKYTHI"]),
                    TenHang = GetStringValue(reader["TENHANG"])
                });
            }

            return result;
        }

        public List<PaymentGatewayDto> GetMomoExamInfoByPhieuIds(List<int> phieuIds)
        {
            var result = new List<PaymentGatewayDto>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_EXAM_PAYMENT.GET_MOMO_EXAM_INFO_BY_IDS", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_phieuIds", OracleDbType.NVarchar2).Value = string.Join(",", phieuIds.Distinct());
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PaymentGatewayDto
                {
                    PhieuId = GetInt32Value(reader["PHIEUID"]),
                    TenPhieu = GetStringValue(reader["TENPHIEU"]),
                    NgayLap = GetDateTimeValue(reader["NGAYLAP"]),
                    TongTien = GetDecimalValue(reader["TONGTIEN"]),
                    NgayNop = GetDateTimeValue(reader["NGAYNOP"]),
                    PhuongThuc = GetStringValue(reader["PHUONGTHUC"]),
                    HoSoId = GetInt32Value(reader["HOSOID"]),
                    GhiChu = GetStringValue(reader["GHICHU"]),
                    TenHoSo = GetStringValue(reader["TENHOSO"]),
                    HoTenHocVien = GetStringValue(reader["HOTENHOCVIEN"]),
                    KhoaHocId = 0,
                    TenKhoaHoc = GetStringValue(reader["TENKYTHI"]),
                    TenHang = GetStringValue(reader["TENHANG"])
                });
            }

            return result;
        }

        public int MarkMomoExamSuccess(List<int> phieuIds)
        {
            if (phieuIds == null || phieuIds.Count == 0)
                return 0;

            int successCount = 0;

            using var conn = new OracleConnection(_connectionString);
            conn.Open();

            foreach (var phieuId in phieuIds.Distinct())
            {
                using var cmd = new OracleCommand("PKG_EXAM_PAYMENT.MARK_EXAM_PAYMENT_SUCCESS", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;

                cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;
                cmd.Parameters.Add("p_method", OracleDbType.NVarchar2).Value = "MOMO";

                var output = cmd.Parameters.Add("o_result", OracleDbType.Int32);
                output.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                if (GetInt32Value(output.Value) == 1)
                    successCount++;
            }

            return successCount;
        }
        // =========================
        // CHUNG
        // =========================
        public int MarkExamPaymentFail(List<int> phieuIds)
        {
            if (phieuIds == null || phieuIds.Count == 0)
                return 0;

            int successCount = 0;

            using var conn = new OracleConnection(_connectionString);
            conn.Open();

            foreach (var phieuId in phieuIds.Distinct())
            {
                using var cmd = new OracleCommand("PKG_EXAM_PAYMENT.MARK_EXAM_PAYMENT_FAIL", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.BindByName = true;

                cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;

                var output = cmd.Parameters.Add("o_result", OracleDbType.Int32);
                output.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                if (GetInt32Value(output.Value) == 1)
                    successCount++;
            }

            return successCount;
        }

        public ExamPaymentInvoiceDto? GetInvoiceDetail(int userId, int phieuId)
        {
            ExamPaymentInvoiceDto? result = null;

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_EXAM_PAYMENT.GET_INVOICE_DETAIL", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                result = new ExamPaymentInvoiceDto
                {
                    PhieuId = GetInt32Value(reader["PHIEUID"]),
                    TenPhieu = GetStringValue(reader["TENPHIEU"]),
                    NgayLap = GetDateTimeValue(reader["NGAYLAP"]),
                    NgayNop = GetDateTimeValue(reader["NGAYNOP"]),
                    TongTien = GetDecimalValue(reader["TONGTIEN"]),
                    PhuongThuc = GetStringValue(reader["PHUONGTHUC"]),
                    LoaiPhi = GetStringValue(reader["LOAIPHI"]),
                    GhiChu = GetStringValue(reader["GHICHU"]),
                    TrangThaiThanhToan = GetStringValue(reader["TRANGTHAITHANHTOAN"]),
                    HoSoId = GetInt32Value(reader["HOSOID"]),
                    TenHoSo = GetStringValue(reader["TENHOSO"]),
                    TrangThaiHoSo = GetStringValue(reader["TRANGTHAIHOSO"]),
                    HocVienId = GetInt32Value(reader["HOCVIENID"]),
                    HoTenHocVien = GetStringValue(reader["HOTENHOCVIEN"]),
                    Sdt = GetStringValue(reader["SDT"]),
                    Email = GetStringValue(reader["EMAIL"]),
                    KyThiId = GetInt32Value(reader["KYTHIID"]),
                    TenKyThi = GetStringValue(reader["TENKYTHI"]),
                    LoaiKyThi = GetStringValue(reader["LOAIKYTHI"]),
                    ThoiGianThi = GetDateTimeValue(reader["THOIGIANTHI"]),
                    DiaDiemThi = GetStringValue(reader["DIADIEMTHI"]),
                    TenHang = GetStringValue(reader["TENHANG"]),
                    HocPhi = GetDecimalValue(reader["HOCPHI"])
                };
            }

            return result;
        }

        public List<ExamPaymentHistoryDto> GetPaymentHistoryByUser(int userId)
        {
            var result = new List<ExamPaymentHistoryDto>();

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_EXAM_PAYMENT.GET_PAYMENT_HISTORY_BY_USER", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new ExamPaymentHistoryDto
                {
                    PhieuId = GetInt32Value(reader["PHIEUID"]),
                    TenPhieu = GetStringValue(reader["TENPHIEU"]),
                    NgayLap = GetDateTimeValue(reader["NGAYLAP"]),
                    NgayNop = GetDateTimeValue(reader["NGAYNOP"]),
                    TongTien = GetDecimalValue(reader["TONGTIEN"]),
                    PhuongThuc = GetStringValue(reader["PHUONGTHUC"]),
                    LoaiPhi = GetStringValue(reader["LOAIPHI"]),
                    GhiChu = GetStringValue(reader["GHICHU"]),
                    HoSoId = GetInt32Value(reader["HOSOID"]),
                    TenHoSo = GetStringValue(reader["TENHOSO"]),
                    HoTenHocVien = GetStringValue(reader["HOTENHOCVIEN"]),
                    KyThiId = GetInt32Value(reader["KYTHIID"]),
                    TenKyThi = GetStringValue(reader["TENKYTHI"]),
                    LoaiKyThi = GetStringValue(reader["LOAIKYTHI"]),
                    TenHang = GetStringValue(reader["TENHANG"]),
                    TrangThaiThanhToan = GetStringValue(reader["TRANGTHAITHANHTOAN"]),
                    CoTheTaiHoaDon = GetInt32Value(reader["COTHETAIHOADON"])
                });
            }

            return result;
        }

        public ExamPaymentHistoryDetailDto? GetPaymentHistoryDetail(int userId, int phieuId)
        {
            ExamPaymentHistoryDetailDto? result = null;

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_EXAM_PAYMENT.GET_PAYMENT_HISTORY_DETAIL", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                result = new ExamPaymentHistoryDetailDto
                {
                    PhieuId = GetInt32Value(reader["PHIEUID"]),
                    TenPhieu = GetStringValue(reader["TENPHIEU"]),
                    NgayLap = GetDateTimeValue(reader["NGAYLAP"]),
                    NgayNop = GetDateTimeValue(reader["NGAYNOP"]),
                    TongTien = GetDecimalValue(reader["TONGTIEN"]),
                    PhuongThuc = GetStringValue(reader["PHUONGTHUC"]),
                    LoaiPhi = GetStringValue(reader["LOAIPHI"]),
                    GhiChu = GetStringValue(reader["GHICHU"]),
                    TrangThaiThanhToan = GetStringValue(reader["TRANGTHAITHANHTOAN"]),
                    HoSoId = GetInt32Value(reader["HOSOID"]),
                    TenHoSo = GetStringValue(reader["TENHOSO"]),
                    TrangThaiHoSo = GetStringValue(reader["TRANGTHAIHOSO"]),
                    HocVienId = GetInt32Value(reader["HOCVIENID"]),
                    HoTenHocVien = GetStringValue(reader["HOTENHOCVIEN"]),
                    Sdt = GetStringValue(reader["SDT"]),
                    Email = GetStringValue(reader["EMAIL"]),
                    KyThiId = GetInt32Value(reader["KYTHIID"]),
                    TenKyThi = GetStringValue(reader["TENKYTHI"]),
                    LoaiKyThi = GetStringValue(reader["LOAIKYTHI"]),
                    ThoiGianThi = GetDateTimeValue(reader["THOIGIANTHI"]),
                    DiaDiemThi = GetStringValue(reader["DIADIEMTHI"]),
                    TenHang = GetStringValue(reader["TENHANG"]),
                    HocPhi = GetDecimalValue(reader["HOCPHI"])
                };
            }

            return result;
        }

        private int GetInt32Value(object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value)
                return 0;

            if (dbValue is OracleDecimal oracleDecimal)
                return oracleDecimal.ToInt32();

            if (dbValue is decimal decValue)
                return decimal.ToInt32(decValue);

            if (dbValue is int intValue)
                return intValue;

            if (dbValue is long longValue)
                return (int)longValue;

            if (dbValue is short shortValue)
                return shortValue;

            return int.TryParse(dbValue.ToString(), out var parsed) ? parsed : 0;
        }

        private decimal GetDecimalValue(object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value)
                return 0m;

            if (dbValue is OracleDecimal oracleDecimal)
                return oracleDecimal.Value;

            if (dbValue is decimal decValue)
                return decValue;

            if (dbValue is int intValue)
                return intValue;

            if (dbValue is long longValue)
                return longValue;

            var raw = dbValue.ToString();
            if (string.IsNullOrWhiteSpace(raw))
                return 0m;

            return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0m;
        }

        private string GetStringValue(object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value)
                return string.Empty;

            if (dbValue is OracleString oracleString)
                return oracleString.IsNull ? string.Empty : oracleString.Value;

            return dbValue.ToString() ?? string.Empty;
        }

        private DateTime? GetDateTimeValue(object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value)
                return null;

            if (dbValue is DateTime dt)
                return dt;

            if (dbValue is OracleTimeStamp oracleTimeStamp)
                return oracleTimeStamp.Value;

            if (dbValue is OracleDate oracleDate)
                return oracleDate.Value;

            return DateTime.TryParse(dbValue.ToString(), out var parsed) ? parsed : null;
        }
    }
}