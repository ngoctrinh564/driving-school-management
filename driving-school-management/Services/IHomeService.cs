using driving_school_management.Models;
using driving_school_management.ViewModels;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace driving_school_management.Services
{
    public interface IHomeService
    {
        Task<HomeDashboardViewModel> GetHomeDashboardAsync(int? userId);
    }
    public class HomeService : IHomeService
    {
        private readonly IConfiguration _configuration;

        public HomeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<HomeDashboardViewModel> GetHomeDashboardAsync(int? userId)
        {
            var model = new HomeDashboardViewModel
            {
                IsLoggedIn = userId.HasValue
            };

            await using var conn = new OracleConnection(_configuration.GetConnectionString("OracleDb"));
            await using var cmd = new OracleCommand("SP_HOME_DASHBOARD", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (userId.HasValue)
                cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId.Value;
            else
                cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = DBNull.Value;

            cmd.Parameters.Add("p_total_courses", OracleDbType.Decimal).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_total_exams", OracleDbType.Decimal).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_total_licenses", OracleDbType.Decimal).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_featured_courses", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_my_profiles", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_my_exam_results", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            model.TotalCourses = ((OracleDecimal)cmd.Parameters["p_total_courses"].Value).ToInt32();
            model.TotalExams = ((OracleDecimal)cmd.Parameters["p_total_exams"].Value).ToInt32();
            model.TotalLicenses = ((OracleDecimal)cmd.Parameters["p_total_licenses"].Value).ToInt32();

            using (var reader = ((OracleRefCursor)cmd.Parameters["p_featured_courses"].Value).GetDataReader())
            {
                while (reader.Read())
                {
                    model.FeaturedCourses.Add(new HomeCourseItem
                    {
                        KhoaHocId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        TenKhoaHoc = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        TenHang = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        NgayBatDau = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                        NgayKetThuc = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                        DiaDiem = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        TrangThai = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        HocPhi = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7)
                    });
                }
            }

            using (var reader = ((OracleRefCursor)cmd.Parameters["p_my_profiles"].Value).GetDataReader())
            {
                while (reader.Read())
                {
                    model.MyProfiles.Add(new HomeProfileItem
                    {
                        HoSoId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        TenHoSo = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        TenHang = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        NgayDangKy = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                        TrangThai = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                    });
                }
            }

            using (var reader = ((OracleRefCursor)cmd.Parameters["p_my_exam_results"].Value).GetDataReader())
            {
                while (reader.Read())
                {
                    model.MyExamResults.Add(new HomeExamResultItem
                    {
                        TenBaiThi = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        KetQuaDatDuoc = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        TongDiem = reader.IsDBNull(2) ? null : reader.GetFloat(2),
                        TenKyThi = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                    });
                }
            }

            return model;
        }
    }

}