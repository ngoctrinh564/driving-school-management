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

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_PAYMENT_HISTORY.GET_ALL_PAYMENT_HISTORY", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PaymentHistoryDto
                {
                    PhieuId = GetInt32Value(reader["PHIEUID"]),
                    TenPhieu = GetStringValue(reader["TENPHIEU"]),
                    NgayLap = GetNullableDateTimeValue(reader["NGAYLAP"]),
                    NgayNop = GetNullableDateTimeValue(reader["NGAYNOP"]),
                    TongTien = GetDecimalValue(reader["TONGTIEN"]),
                    PhuongThuc = GetStringValue(reader["PHUONGTHUC"]),
                    LoaiPhi = GetStringValue(reader["LOAIPHI"]),
                    GhiChu = GetStringValue(reader["GHICHU"]),
                    HoSoId = GetInt32Value(reader["HOSOID"]),
                    TenHoSo = GetStringValue(reader["TENHOSO"]),
                    HoTenHocVien = GetStringValue(reader["HOTENHOCVIEN"]),
                    KhoaHocId = GetInt32Value(reader["KHOAHOCID"]),
                    TenKhoaHoc = GetStringValue(reader["TENKHOAHOC"]),
                    KyThiId = GetInt32Value(reader["KYTHIID"]),
                    TenKyThi = GetStringValue(reader["TENKYTHI"]),
                    TenHang = GetStringValue(reader["TENHANG"]),
                    TrangThaiThanhToan = GetStringValue(reader["TRANGTHAITHANHTOAN"]),
                    CoTheTaiHoaDon = GetInt32Value(reader["COTHETAIHOADON"])
                });
            }

            return result;
        }

        public PaymentHistoryDetailDto? GetPaymentHistoryDetail(int userId, int phieuId)
        {
            var historyItem = GetPaymentHistoryType(userId, phieuId);
            if (historyItem == null)
                return null;

            if (string.Equals(historyItem.LoaiPhi, "Kỳ thi", StringComparison.OrdinalIgnoreCase))
                return GetExamPaymentHistoryDetail(userId, phieuId);

            return GetCoursePaymentHistoryDetail(userId, phieuId);
        }

        private PaymentHistoryDto? GetPaymentHistoryType(int userId, int phieuId)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("PKG_PAYMENT_HISTORY.GET_ALL_PAYMENT_HISTORY", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var currentPhieuId = GetInt32Value(reader["PHIEUID"]);
                if (currentPhieuId != phieuId)
                    continue;

                return new PaymentHistoryDto
                {
                    PhieuId = currentPhieuId,
                    LoaiPhi = GetStringValue(reader["LOAIPHI"])
                };
            }

            return null;
        }

        private PaymentHistoryDetailDto? GetCoursePaymentHistoryDetail(int userId, int phieuId)
        {
            PaymentHistoryDetailDto? result = null;

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("SP_PAYMENT_HISTORY_DETAIL", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
            cmd.Parameters.Add("p_phieuId", OracleDbType.Int32).Value = phieuId;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            conn.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                result = new PaymentHistoryDetailDto
                {
                    PhieuId = GetInt32Value(reader["PHIEUID"]),
                    TenPhieu = GetStringValue(reader["TENPHIEU"]),
                    NgayLap = GetNullableDateTimeValue(reader["NGAYLAP"]),
                    NgayNop = GetNullableDateTimeValue(reader["NGAYNOP"]),
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

                    KhoaHocId = GetInt32Value(reader["KHOAHOCID"]),
                    TenKhoaHoc = GetStringValue(reader["TENKHOAHOC"]),
                    DiaDiem = GetStringValue(reader["DIADIEM"]),
                    NgayBatDau = GetNullableDateTimeValue(reader["NGAYBATDAU"]),
                    NgayKetThuc = GetNullableDateTimeValue(reader["NGAYKETTHUC"]),
                    TrangThaiKhoaHoc = GetStringValue(reader["TRANGTHAIKHOAHOC"]),

                    HangId = GetInt32Value(reader["HANGID"]),
                    TenHang = GetStringValue(reader["TENHANG"]),
                    LoaiPhuongTien = GetStringValue(reader["LOAIPHUONGTIEN"]),
                    HocPhi = GetDecimalValue(reader["HOCPHI"])
                };
            }

            return result;
        }

        private PaymentHistoryDetailDto? GetExamPaymentHistoryDetail(int userId, int phieuId)
        {
            PaymentHistoryDetailDto? result = null;

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
                result = new PaymentHistoryDetailDto
                {
                    PhieuId = GetInt32Value(reader["PHIEUID"]),
                    TenPhieu = GetStringValue(reader["TENPHIEU"]),
                    NgayLap = GetNullableDateTimeValue(reader["NGAYLAP"]),
                    NgayNop = GetNullableDateTimeValue(reader["NGAYNOP"]),
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
                    ThoiGianThi = GetNullableDateTimeValue(reader["THOIGIANTHI"]),
                    DiaDiemThi = GetStringValue(reader["DIADIEMTHI"]),

                    TenHang = GetStringValue(reader["TENHANG"]),
                    HocPhi = GetDecimalValue(reader["HOCPHI"])
                };
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

            if (dbValue is DateTime dtValue)
                return dtValue;

            return DateTime.Parse(dbValue.ToString()!);
        }
    }
}