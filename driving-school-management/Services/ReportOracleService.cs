using System.Data;
using driving_school_management.ViewModels;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace driving_school_management.Services
{
    public class ReportOracleService
    {
        private readonly string _connectionString;
        private readonly string _logoPath;

        public ReportOracleService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _connectionString = configuration.GetConnectionString("OracleDb")
                ?? throw new InvalidOperationException("Missing connection string: OracleDb");

            _logoPath = Path.Combine(env.WebRootPath, "images", "logo", "logo-full.png");
        }

        public async Task<DashboardReportVm> GetDashboardAsync(DateTime? fromDate, DateTime? toDate)
        {
            var model = new DashboardReportVm
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            await LoadOverviewAsync(connection, model, fromDate, toDate);
            model.RevenueByMonth = await LoadChartPointListAsync(connection, "PKG_ADMIN_REPORT.GET_REVENUE_BY_MONTH", fromDate, toDate);
            model.NewUsersByMonth = await LoadChartPointListAsync(connection, "PKG_ADMIN_REPORT.GET_NEW_USERS_BY_MONTH", fromDate, toDate);
            model.NewCoursesByMonth = await LoadChartPointListAsync(connection, "PKG_ADMIN_REPORT.GET_NEW_COURSES_BY_MONTH", fromDate, toDate);
            model.CoursesByHang = await LoadPieSliceListAsync(connection, "PKG_ADMIN_REPORT.GET_COURSES_BY_HANG", fromDate, toDate);
            model.NewExamsByMonth = await LoadChartPointListAsync(connection, "PKG_ADMIN_REPORT.GET_NEW_EXAMS_BY_MONTH", fromDate, toDate);
            model.ExamsByType = await LoadPieSliceListAsync(connection, "PKG_ADMIN_REPORT.GET_EXAMS_BY_TYPE", fromDate, toDate);

            return model;
        }

        private async Task LoadOverviewAsync(
            OracleConnection connection,
            DashboardReportVm model,
            DateTime? fromDate,
            DateTime? toDate)
        {
            await using var command = new OracleCommand("PKG_ADMIN_REPORT.GET_OVERVIEW", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("P_FROM_DATE", OracleDbType.Date).Value = fromDate.HasValue ? fromDate.Value : DBNull.Value;
            command.Parameters.Add("P_TO_DATE", OracleDbType.Date).Value = toDate.HasValue ? toDate.Value : DBNull.Value;
            command.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                model.TongDoanhThu = reader["TONG_DOANH_THU"] == DBNull.Value
                    ? 0
                    : decimal.Parse(reader["TONG_DOANH_THU"].ToString()!);

                model.SoThangCoDoanhThu = reader["SO_THANG_CO_DOANH_THU"] == DBNull.Value
                    ? 0
                    : decimal.Parse(reader["SO_THANG_CO_DOANH_THU"].ToString()!);

                model.DoanhThuTrungBinhThang = reader["DOANH_THU_TRUNG_BINH_THANG"] == DBNull.Value
                    ? 0
                    : decimal.Parse(reader["DOANH_THU_TRUNG_BINH_THANG"].ToString()!);

                model.SoNguoiMoi = reader["SO_NGUOI_MOI"] == DBNull.Value
                    ? 0
                    : decimal.Parse(reader["SO_NGUOI_MOI"].ToString()!);

                model.SoKhoaHocMoi = reader["SO_KHOA_HOC_MOI"] == DBNull.Value
                    ? 0
                    : decimal.Parse(reader["SO_KHOA_HOC_MOI"].ToString()!);

                model.SoKyThiMoi = reader["SO_KY_THI_MOI"] == DBNull.Value
                    ? 0
                    : decimal.Parse(reader["SO_KY_THI_MOI"].ToString()!);

                model.SoHangGplx = reader["SO_HANG_GPLX"] == DBNull.Value
                    ? 0
                    : decimal.Parse(reader["SO_HANG_GPLX"].ToString()!);
            }
        }

        private async Task<List<ChartPointVm>> LoadChartPointListAsync(
            OracleConnection connection,
            string procedureName,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var result = new List<ChartPointVm>();

            await using var command = new OracleCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("P_FROM_DATE", OracleDbType.Date).Value = fromDate.HasValue ? fromDate.Value : DBNull.Value;
            command.Parameters.Add("P_TO_DATE", OracleDbType.Date).Value = toDate.HasValue ? toDate.Value : DBNull.Value;
            command.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new ChartPointVm
                {
                    Label = reader["LABEL"]?.ToString() ?? string.Empty,
                    Value = reader["VALUE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["VALUE"])
                });
            }

            return result;
        }

        private async Task<List<PieSliceVm>> LoadPieSliceListAsync(
            OracleConnection connection,
            string procedureName,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var result = new List<PieSliceVm>();

            await using var command = new OracleCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("P_FROM_DATE", OracleDbType.Date).Value = fromDate.HasValue ? fromDate.Value : DBNull.Value;
            command.Parameters.Add("P_TO_DATE", OracleDbType.Date).Value = toDate.HasValue ? toDate.Value : DBNull.Value;
            command.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new PieSliceVm
                {
                    Label = reader["LABEL"]?.ToString() ?? string.Empty,
                    Value = reader["VALUE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["VALUE"])
                });
            }

            return result;
        }

        private static string FormatRange(DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue && !toDate.HasValue)
                return "Tất cả";

            if (fromDate.HasValue && toDate.HasValue)
                return $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}";

            if (fromDate.HasValue)
                return $"Từ {fromDate:dd/MM/yyyy}";

            return $"Đến {toDate:dd/MM/yyyy}";
        }

        private static string TabTitle(string tab) => tab switch
        {
            "users" => "BÁO CÁO NGƯỜI DÙNG",
            "courses" => "BÁO CÁO KHÓA HỌC",
            "exams" => "BÁO CÁO KỲ THI",
            _ => "BÁO CÁO TỔNG QUAN"
        };

        public byte[] GenerateDashboardPdf(
            DashboardReportVm model,
            DateTime? fromDate,
            DateTime? toDate,
            string? tab)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var rangeText = FormatRange(fromDate, toDate);
            var currentTab = string.IsNullOrWhiteSpace(tab) ? "overview" : tab;
            var title = TabTitle(currentTab);
            var hasLogo = File.Exists(_logoPath);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(title).FontSize(18).Bold();
                            col.Item().PaddingTop(2).Text($"Thời gian: {rangeText}").FontSize(10).FontColor(Colors.Grey.Darken2);
                        });

                        row.ConstantItem(80).AlignRight().AlignMiddle().Element(e =>
                        {
                            if (hasLogo)
                                e.Image(_logoPath).FitArea();
                            else
                                e.Border(1).AlignCenter().AlignMiddle().Text("LOGO");
                        });
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Tổng doanh thu: {model.TongDoanhThu:N0} VND");
                        col.Item().Text($"Doanh thu trung bình tháng: {model.DoanhThuTrungBinhThang:N0} VND");
                        col.Item().Text($"Người mới: {model.SoNguoiMoi}");
                        col.Item().Text($"Khóa học mới: {model.SoKhoaHocMoi}");
                        col.Item().Text($"Kỳ thi mới: {model.SoKyThiMoi}");
                        col.Item().Text($"Số hạng GPLX: {model.SoHangGplx}");

                        col.Item().LineHorizontal(1);

                        switch (currentTab)
                        {
                            case "users":
                                BuildChartTable(col, "Người dùng mới theo tháng", "Tháng", "Người mới", model.NewUsersByMonth, false);
                                break;
                            case "courses":
                                BuildChartTable(col, "Khóa học mới theo tháng", "Tháng", "Số khóa học", model.NewCoursesByMonth, false);
                                BuildPieList(col, "Phân bố khóa học theo hạng", model.CoursesByHang);
                                break;
                            case "exams":
                                BuildChartTable(col, "Kỳ thi mới theo tháng", "Tháng", "Số kỳ thi", model.NewExamsByMonth, false);
                                BuildPieList(col, "Phân bố kỳ thi theo loại", model.ExamsByType);
                                break;
                            default:
                                BuildChartTable(col, "Doanh thu theo tháng", "Tháng", "Doanh thu", model.RevenueByMonth, true);
                                break;
                        }
                    });

                    page.Footer().AlignCenter().Text($"Xuất lúc {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                });
            });

            return document.GeneratePdf();
        }

        private static void BuildChartTable(
            QuestPDF.Fluent.ColumnDescriptor col,
            string title,
            string col1,
            string col2,
            List<ChartPointVm> data,
            bool isMoney)
        {
            col.Item().Text(title).Bold().FontSize(13);

            if (data == null || data.Count == 0)
            {
                col.Item().Text("Không có dữ liệu");
                return;
            }

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                table.Header(h =>
                {
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text(col1).SemiBold();
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(6).AlignRight().Text(col2).SemiBold();
                });

                foreach (var item in data)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(item.Label);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight()
                        .Text(isMoney ? item.Value.ToString("N0") : item.Value.ToString("N0"));
                }
            });
        }

        private static void BuildPieList(
            QuestPDF.Fluent.ColumnDescriptor col,
            string title,
            List<PieSliceVm> data)
        {
            col.Item().Text(title).Bold().FontSize(13);

            if (data == null || data.Count == 0)
            {
                col.Item().Text("Không có dữ liệu");
                return;
            }

            var total = data.Sum(x => x.Value);
            if (total <= 0) total = 1;

            foreach (var item in data)
            {
                var percent = (item.Value / total) * 100;
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text(item.Label);
                    row.ConstantItem(100).AlignRight().Text($"{item.Value:N0} ({percent:0.#}%)");
                });
            }
        }
    }
}