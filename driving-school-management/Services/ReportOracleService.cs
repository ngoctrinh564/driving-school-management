//using System.Data;
//using driving_school_management.ViewModels;
//using Microsoft.Extensions.Configuration;
//using Oracle.ManagedDataAccess.Client;
//using QuestPDF.Fluent;
//using QuestPDF.Helpers;
//using QuestPDF.Infrastructure;

//namespace driving_school_management.Services
//{
//    public class ReportOracleService
//    {
//        private readonly string _connectionString;
//        private readonly string _logoPath;

//        public ReportOracleService(IConfiguration configuration, IWebHostEnvironment env)
//        {
//            _connectionString = configuration.GetConnectionString("OracleDb")
//                ?? throw new InvalidOperationException("Missing connection string: OracleDb");

//            _logoPath = Path.Combine(env.WebRootPath, "images", "logo", "logo-full.png");
//        }

//        public async Task<DashboardReportVm> GetDashboardAsync(DateTime? fromDate, DateTime? toDate)
//        {
//            var model = new DashboardReportVm
//            {
//                FromDate = fromDate,
//                ToDate = toDate
//            };

//            await using var connection = new OracleConnection(_connectionString);
//            await connection.OpenAsync();

//            await LoadOverviewAsync(connection, model, fromDate, toDate);
//            model.RevenueByMonth = await LoadChartPointListAsync(connection, "PKG_ADMIN_REPORT.GET_REVENUE_BY_MONTH", fromDate, toDate);
//            model.NewUsersByMonth = await LoadChartPointListAsync(connection, "PKG_ADMIN_REPORT.GET_NEW_USERS_BY_MONTH", fromDate, toDate);
//            model.NewCoursesByMonth = await LoadChartPointListAsync(connection, "PKG_ADMIN_REPORT.GET_NEW_COURSES_BY_MONTH", fromDate, toDate);
//            model.CoursesByHang = await LoadPieSliceListAsync(connection, "PKG_ADMIN_REPORT.GET_COURSES_BY_HANG", fromDate, toDate);
//            model.NewExamsByMonth = await LoadChartPointListAsync(connection, "PKG_ADMIN_REPORT.GET_NEW_EXAMS_BY_MONTH", fromDate, toDate);
//            model.ExamsByType = await LoadPieSliceListAsync(connection, "PKG_ADMIN_REPORT.GET_EXAMS_BY_TYPE", fromDate, toDate);

//            return model;
//        }

//        private async Task LoadOverviewAsync(
//            OracleConnection connection,
//            DashboardReportVm model,
//            DateTime? fromDate,
//            DateTime? toDate)
//        {
//            await using var command = new OracleCommand("PKG_ADMIN_REPORT.GET_OVERVIEW", connection)
//            {
//                CommandType = CommandType.StoredProcedure
//            };

//            command.Parameters.Add("P_FROM_DATE", OracleDbType.Date).Value = fromDate.HasValue ? fromDate.Value : DBNull.Value;
//            command.Parameters.Add("P_TO_DATE", OracleDbType.Date).Value = toDate.HasValue ? toDate.Value : DBNull.Value;
//            command.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

//            await using var reader = await command.ExecuteReaderAsync();

//            if (await reader.ReadAsync())
//            {
//                model.TongDoanhThu = reader["TONG_DOANH_THU"] == DBNull.Value
//                    ? 0
//                    : decimal.Parse(reader["TONG_DOANH_THU"].ToString()!);

//                model.SoThangCoDoanhThu = reader["SO_THANG_CO_DOANH_THU"] == DBNull.Value
//                    ? 0
//                    : decimal.Parse(reader["SO_THANG_CO_DOANH_THU"].ToString()!);

//                model.DoanhThuTrungBinhThang = reader["DOANH_THU_TRUNG_BINH_THANG"] == DBNull.Value
//                    ? 0
//                    : decimal.Parse(reader["DOANH_THU_TRUNG_BINH_THANG"].ToString()!);

//                model.SoNguoiMoi = reader["SO_NGUOI_MOI"] == DBNull.Value
//                    ? 0
//                    : decimal.Parse(reader["SO_NGUOI_MOI"].ToString()!);

//                model.SoKhoaHocMoi = reader["SO_KHOA_HOC_MOI"] == DBNull.Value
//                    ? 0
//                    : decimal.Parse(reader["SO_KHOA_HOC_MOI"].ToString()!);

//                model.SoKyThiMoi = reader["SO_KY_THI_MOI"] == DBNull.Value
//                    ? 0
//                    : decimal.Parse(reader["SO_KY_THI_MOI"].ToString()!);

//                model.SoHangGplx = reader["SO_HANG_GPLX"] == DBNull.Value
//                    ? 0
//                    : decimal.Parse(reader["SO_HANG_GPLX"].ToString()!);
//            }
//        }

//        private async Task<List<ChartPointVm>> LoadChartPointListAsync(
//            OracleConnection connection,
//            string procedureName,
//            DateTime? fromDate,
//            DateTime? toDate)
//        {
//            var result = new List<ChartPointVm>();

//            await using var command = new OracleCommand(procedureName, connection)
//            {
//                CommandType = CommandType.StoredProcedure
//            };

//            command.Parameters.Add("P_FROM_DATE", OracleDbType.Date).Value = fromDate.HasValue ? fromDate.Value : DBNull.Value;
//            command.Parameters.Add("P_TO_DATE", OracleDbType.Date).Value = toDate.HasValue ? toDate.Value : DBNull.Value;
//            command.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

//            await using var reader = await command.ExecuteReaderAsync();

//            while (await reader.ReadAsync())
//            {
//                result.Add(new ChartPointVm
//                {
//                    Label = reader["LABEL"]?.ToString() ?? string.Empty,
//                    Value = reader["VALUE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["VALUE"])
//                });
//            }

//            return result;
//        }

//        private async Task<List<PieSliceVm>> LoadPieSliceListAsync(
//            OracleConnection connection,
//            string procedureName,
//            DateTime? fromDate,
//            DateTime? toDate)
//        {
//            var result = new List<PieSliceVm>();

//            await using var command = new OracleCommand(procedureName, connection)
//            {
//                CommandType = CommandType.StoredProcedure
//            };

//            command.Parameters.Add("P_FROM_DATE", OracleDbType.Date).Value = fromDate.HasValue ? fromDate.Value : DBNull.Value;
//            command.Parameters.Add("P_TO_DATE", OracleDbType.Date).Value = toDate.HasValue ? toDate.Value : DBNull.Value;
//            command.Parameters.Add("P_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

//            await using var reader = await command.ExecuteReaderAsync();

//            while (await reader.ReadAsync())
//            {
//                result.Add(new PieSliceVm
//                {
//                    Label = reader["LABEL"]?.ToString() ?? string.Empty,
//                    Value = reader["VALUE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["VALUE"])
//                });
//            }

//            return result;
//        }

//        private static string FormatRange(DateTime? fromDate, DateTime? toDate)
//        {
//            if (!fromDate.HasValue && !toDate.HasValue)
//                return "Tất cả";

//            if (fromDate.HasValue && toDate.HasValue)
//                return $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}";

//            if (fromDate.HasValue)
//                return $"Từ {fromDate:dd/MM/yyyy}";

//            return $"Đến {toDate:dd/MM/yyyy}";
//        }

//        private static string TabTitle(string tab) => tab switch
//        {
//            "users" => "BÁO CÁO NGƯỜI DÙNG",
//            "courses" => "BÁO CÁO KHÓA HỌC",
//            "exams" => "BÁO CÁO KỲ THI",
//            _ => "BÁO CÁO TỔNG QUAN"
//        };

//        public byte[] GenerateDashboardPdf(
//            DashboardReportVm model,
//            DateTime? fromDate,
//            DateTime? toDate,
//            string? tab)
//        {
//            QuestPDF.Settings.License = LicenseType.Community;

//            var rangeText = FormatRange(fromDate, toDate);
//            var currentTab = string.IsNullOrWhiteSpace(tab) ? "overview" : tab;
//            var title = TabTitle(currentTab);
//            var hasLogo = File.Exists(_logoPath);

//            var document = Document.Create(container =>
//            {
//                container.Page(page =>
//                {
//                    page.Size(PageSizes.A4);
//                    page.Margin(30);
//                    page.DefaultTextStyle(x => x.FontSize(11));

//                    page.Header().PaddingBottom(10).Row(row =>
//                    {
//                        row.RelativeItem().Column(col =>
//                        {
//                            col.Item().Text(title).FontSize(18).Bold();
//                            col.Item().PaddingTop(2).Text($"Thời gian: {rangeText}").FontSize(10).FontColor(Colors.Grey.Darken2);
//                        });

//                        row.ConstantItem(80).AlignRight().AlignMiddle().Element(e =>
//                        {
//                            if (hasLogo)
//                                e.Image(_logoPath).FitArea();
//                            else
//                                e.Border(1).AlignCenter().AlignMiddle().Text("LOGO");
//                        });
//                    });

//                    page.Content().Column(col =>
//                    {
//                        col.Spacing(10);

//                        col.Item().Text($"Tổng doanh thu: {model.TongDoanhThu:N0} VND");
//                        col.Item().Text($"Doanh thu trung bình tháng: {model.DoanhThuTrungBinhThang:N0} VND");
//                        col.Item().Text($"Người mới: {model.SoNguoiMoi}");
//                        col.Item().Text($"Khóa học mới: {model.SoKhoaHocMoi}");
//                        col.Item().Text($"Kỳ thi mới: {model.SoKyThiMoi}");
//                        col.Item().Text($"Số hạng GPLX: {model.SoHangGplx}");

//                        col.Item().LineHorizontal(1);

//                        switch (currentTab)
//                        {
//                            case "users":
//                                BuildChartTable(col, "Người dùng mới theo tháng", "Tháng", "Người mới", model.NewUsersByMonth, false);
//                                break;
//                            case "courses":
//                                BuildChartTable(col, "Khóa học mới theo tháng", "Tháng", "Số khóa học", model.NewCoursesByMonth, false);
//                                BuildPieList(col, "Phân bố khóa học theo hạng", model.CoursesByHang);
//                                break;
//                            case "exams":
//                                BuildChartTable(col, "Kỳ thi mới theo tháng", "Tháng", "Số kỳ thi", model.NewExamsByMonth, false);
//                                BuildPieList(col, "Phân bố kỳ thi theo loại", model.ExamsByType);
//                                break;
//                            default:
//                                BuildChartTable(col, "Doanh thu theo tháng", "Tháng", "Doanh thu", model.RevenueByMonth, true);
//                                break;
//                        }
//                    });

//                    page.Footer().AlignCenter().Text($"Xuất lúc {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
//                });
//            });

//            return document.GeneratePdf();
//        }

//        private static void BuildChartTable(
//            QuestPDF.Fluent.ColumnDescriptor col,
//            string title,
//            string col1,
//            string col2,
//            List<ChartPointVm> data,
//            bool isMoney)
//        {
//            col.Item().Text(title).Bold().FontSize(13);

//            if (data == null || data.Count == 0)
//            {
//                col.Item().Text("Không có dữ liệu");
//                return;
//            }

//            col.Item().Table(table =>
//            {
//                table.ColumnsDefinition(c =>
//                {
//                    c.RelativeColumn();
//                    c.RelativeColumn();
//                });

//                table.Header(h =>
//                {
//                    h.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text(col1).SemiBold();
//                    h.Cell().Background(Colors.Grey.Lighten3).Padding(6).AlignRight().Text(col2).SemiBold();
//                });

//                foreach (var item in data)
//                {
//                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(item.Label);
//                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight()
//                        .Text(isMoney ? item.Value.ToString("N0") : item.Value.ToString("N0"));
//                }
//            });
//        }

//        private static void BuildPieList(
//            QuestPDF.Fluent.ColumnDescriptor col,
//            string title,
//            List<PieSliceVm> data)
//        {
//            col.Item().Text(title).Bold().FontSize(13);

//            if (data == null || data.Count == 0)
//            {
//                col.Item().Text("Không có dữ liệu");
//                return;
//            }

//            var total = data.Sum(x => x.Value);
//            if (total <= 0) total = 1;

//            foreach (var item in data)
//            {
//                var percent = (item.Value / total) * 100;
//                col.Item().Row(row =>
//                {
//                    row.RelativeItem().Text(item.Label);
//                    row.ConstantItem(100).AlignRight().Text($"{item.Value:N0} ({percent:0.#}%)");
//                });
//            }
//        }
//    }
//}
using System.Data;
using System.Globalization;
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

            _logoPath = Path.Combine(env.WebRootPath, "images", "logo", "logo.png");
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
                    : decimal.Parse(reader["TONG_DOANH_THU"].ToString() ?? "0", CultureInfo.InvariantCulture);

                model.SoThangCoDoanhThu = reader["SO_THANG_CO_DOANH_THU"] == DBNull.Value
                    ? 0
                    : decimal.Parse(reader["SO_THANG_CO_DOANH_THU"].ToString() ?? "0", CultureInfo.InvariantCulture);

                model.DoanhThuTrungBinhThang = reader["DOANH_THU_TRUNG_BINH_THANG"] == DBNull.Value
                    ? 0
                    : decimal.Parse(reader["DOANH_THU_TRUNG_BINH_THANG"].ToString() ?? "0", CultureInfo.InvariantCulture);

                model.SoNguoiMoi = reader["SO_NGUOI_MOI"] == DBNull.Value
                    ? 0
                    : decimal.Parse(reader["SO_NGUOI_MOI"].ToString() ?? "0", CultureInfo.InvariantCulture);

                model.SoKhoaHocMoi = reader["SO_KHOA_HOC_MOI"] == DBNull.Value
                    ? 0
                    : decimal.Parse(reader["SO_KHOA_HOC_MOI"].ToString() ?? "0", CultureInfo.InvariantCulture);

                model.SoKyThiMoi = reader["SO_KY_THI_MOI"] == DBNull.Value
                    ? 0
                    : decimal.Parse(reader["SO_KY_THI_MOI"].ToString() ?? "0", CultureInfo.InvariantCulture);

                model.SoHangGplx = reader["SO_HANG_GPLX"] == DBNull.Value
                    ? 0
                    : decimal.Parse(reader["SO_HANG_GPLX"].ToString() ?? "0", CultureInfo.InvariantCulture);
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
                    Value = reader["VALUE"] == DBNull.Value
                        ? 0
                        : decimal.Parse(reader["VALUE"].ToString() ?? "0", CultureInfo.InvariantCulture)
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
                    Value = reader["VALUE"] == DBNull.Value
                        ? 0
                        : decimal.Parse(reader["VALUE"].ToString() ?? "0", CultureInfo.InvariantCulture)
                });
            }

            return result;
        }

        private static string FormatRange(DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue && !toDate.HasValue)
                return "Tất cả thời gian";

            if (fromDate.HasValue && toDate.HasValue)
                return $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}";

            if (fromDate.HasValue)
                return $"Từ {fromDate:dd/MM/yyyy}";

            return $"Đến {toDate:dd/MM/yyyy}";
        }

        private static string TabTitle(string? tab) => (tab ?? "overview").ToLower() switch
        {
            "users" => "BÁO CÁO THỐNG KÊ NGƯỜI DÙNG",
            "courses" => "BÁO CÁO THỐNG KÊ KHÓA HỌC",
            "exams" => "BÁO CÁO THỐNG KÊ KỲ THI",
            _ => "BÁO CÁO THỐNG KÊ TỔNG QUAN"
        };

        public byte[] GenerateDashboardPdf(
            DashboardReportVm model,
            DateTime? fromDate,
            DateTime? toDate,
            string? tab)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var currentTab = string.IsNullOrWhiteSpace(tab) ? "overview" : tab.ToLower();
            var title = TabTitle(currentTab);
            var rangeText = FormatRange(fromDate, toDate);
            var hasLogo = File.Exists(_logoPath);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(28);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor("#0F172A"));

                    page.Background().Element(bg =>
                    {
                        bg.AlignCenter()
                          .AlignMiddle()
                          .Rotate(-35)
                          .Text("ĐÀO TẠO VÀ CẤP GPLX - ORACLE")
                          .FontSize(34)
                          .Bold()
                          .FontColor(Colors.Grey.Lighten2);
                    });

                    page.Header().Column(header =>
                    {
                        header.Item().Row(row =>
                        {
                            row.RelativeItem().Column(left =>
                            {
                                left.Item().Text("TRUNG TÂM ĐÀO TẠO VÀ CẤP GPLX")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor("#2563EB");

                                left.Item().Text("Hệ thống báo cáo quản trị")
                                    .FontSize(9)
                                    .FontColor("#64748B");
                            });

                            row.ConstantItem(70).Height(50).AlignRight().AlignMiddle().Element(e =>
                            {
                                if (hasLogo)
                                {
                                    e.Image(_logoPath).FitArea();
                                }
                                else
                                {
                                    e.Border(1)
                                     .BorderColor(Colors.Grey.Lighten2)
                                     .AlignCenter()
                                     .AlignMiddle()
                                     .Text("LOGO")
                                     .FontSize(8)
                                     .FontColor(Colors.Grey.Darken1);
                                }
                            });
                        });

                        header.Item().PaddingTop(10).BorderBottom(1).BorderColor("#E2E8F0");

                        header.Item().PaddingTop(10).Column(c =>
                        {
                            c.Item().Text(title)
                                .FontSize(18)
                                .Bold()
                                .FontColor("#0F172A");

                            c.Item().PaddingTop(3).Text($"Khoảng thời gian: {rangeText}")
                                .FontSize(10)
                                .FontColor("#475569");

                            c.Item().PaddingTop(2).Text($"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                                .FontSize(9)
                                .FontColor("#64748B");
                        });
                    });

                    page.Content().PaddingTop(16).Column(content =>
                    {
                        content.Spacing(14);

                        content.Item().Element(c => BuildOverviewCards(c, model));

                        content.Item().Element(c => BuildSummarySection(c, model, currentTab));

                        switch (currentTab)
                        {
                            case "users":
                                BuildUsersSection(content, model);
                                break;
                            case "courses":
                                BuildCoursesSection(content, model);
                                break;
                            case "exams":
                                BuildExamsSection(content, model);
                                break;
                            default:
                                BuildOverviewSection(content, model);
                                break;
                        }
                    });

                    page.Footer().PaddingTop(8).BorderTop(1).BorderColor("#E2E8F0").Row(row =>
                    {
                        row.RelativeItem().Text("ĐÀO TẠO VÀ CẤP GPLX - ORACLE")
                            .FontSize(9)
                            .FontColor("#64748B");

                        row.ConstantItem(80).AlignRight().Text(text =>
                        {
                            text.Span("Trang ").FontSize(9).FontColor("#64748B");
                            text.CurrentPageNumber().FontSize(9).SemiBold();
                            text.Span("/").FontSize(9).FontColor("#64748B");
                            text.TotalPages().FontSize(9).SemiBold();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static void BuildOverviewCards(IContainer container, DashboardReportVm model)
        {
            container.Row(row =>
            {
                row.Spacing(10);

                row.RelativeItem().Element(e => SummaryCard(e, "Tổng doanh thu", $"{model.TongDoanhThu:N0} VND"));
                row.RelativeItem().Element(e => SummaryCard(e, "Người mới", $"{model.SoNguoiMoi:N0}"));
                row.RelativeItem().Element(e => SummaryCard(e, "Khóa học mới", $"{model.SoKhoaHocMoi:N0}"));
                row.RelativeItem().Element(e => SummaryCard(e, "Kỳ thi mới", $"{model.SoKyThiMoi:N0}"));
            });
        }

        private static void SummaryCard(IContainer container, string title, string value)
        {
            container.Border(1)
                     .BorderColor("#E2E8F0")
                     .Background("#FFFFFF")
                     .Padding(10)
                     .Column(col =>
                     {
                         col.Item().Text(title)
                             .FontSize(9)
                             .FontColor("#64748B");

                         col.Item().PaddingTop(4).Text(value)
                             .FontSize(14)
                             .Bold()
                             .FontColor("#0F172A");
                     });
        }

        private static void BuildSummarySection(IContainer container, DashboardReportVm model, string tab)
        {
            container.Border(1)
                     .BorderColor("#E2E8F0")
                     .Background("#F8FAFC")
                     .Padding(12)
                     .Column(col =>
                     {
                         col.Item().Text("Thông tin tổng hợp")
                             .FontSize(12)
                             .Bold()
                             .FontColor("#0F172A");

                         col.Item().PaddingTop(8).Text($"Số tháng có doanh thu: {model.SoThangCoDoanhThu:N0}");
                         col.Item().Text($"Doanh thu trung bình tháng: {model.DoanhThuTrungBinhThang:N0} VND");
                         col.Item().Text($"Tổng số hạng GPLX hiện có: {model.SoHangGplx:N0}");

                         if (tab == "users")
                             col.Item().Text("Nội dung chi tiết: thống kê học viên mới theo tháng.");
                         else if (tab == "courses")
                             col.Item().Text("Nội dung chi tiết: thống kê khóa học mở mới và phân bố theo hạng GPLX.");
                         else if (tab == "exams")
                             col.Item().Text("Nội dung chi tiết: thống kê kỳ thi mới và phân bố loại kỳ thi.");
                         else
                             col.Item().Text("Nội dung chi tiết: tổng hợp doanh thu và chỉ số điều hành chính.");
                     });
        }

        private static void BuildOverviewSection(ColumnDescriptor col, DashboardReportVm model)
        {
            col.Item().Text("1. Doanh thu theo tháng")
                .FontSize(13)
                .Bold();

            col.Item().Element(c => BuildChartTable(
                c,
                "Tháng",
                "Doanh thu (VND)",
                model.RevenueByMonth,
                true));

            col.Item().Text("2. Tổng hợp nhanh")
                .FontSize(13)
                .Bold();

            col.Item().Element(c => BuildMiniInfoTable(
                c,
                new List<(string, string)>
                {
                    ("Tổng doanh thu", $"{model.TongDoanhThu:N0} VND"),
                    ("Số tháng có doanh thu", $"{model.SoThangCoDoanhThu:N0}"),
                    ("Doanh thu trung bình tháng", $"{model.DoanhThuTrungBinhThang:N0} VND"),
                    ("Số hạng GPLX", $"{model.SoHangGplx:N0}")
                }));
        }

        private static void BuildUsersSection(ColumnDescriptor col, DashboardReportVm model)
        {
            col.Item().Text("1. Người dùng mới theo tháng")
                .FontSize(13)
                .Bold();

            col.Item().Element(c => BuildChartTable(
                c,
                "Tháng",
                "Số người mới",
                model.NewUsersByMonth,
                false));

            col.Item().Text("2. Tổng hợp người dùng")
                .FontSize(13)
                .Bold();

            col.Item().Element(c => BuildMiniInfoTable(
                c,
                new List<(string, string)>
                {
                    ("Tổng người dùng mới", $"{model.SoNguoiMoi:N0}")
                }));
        }

        private static void BuildCoursesSection(ColumnDescriptor col, DashboardReportVm model)
        {
            col.Item().Text("1. Khóa học mới theo tháng")
                .FontSize(13)
                .Bold();

            col.Item().Element(c => BuildChartTable(
                c,
                "Tháng",
                "Số khóa học",
                model.NewCoursesByMonth,
                false));

            col.Item().Text("2. Phân bố khóa học theo hạng GPLX")
                .FontSize(13)
                .Bold();

            col.Item().Element(c => BuildPieListTable(
                c,
                "Hạng GPLX",
                "Số lượng",
                model.CoursesByHang));

            col.Item().Text("3. Tổng hợp khóa học")
                .FontSize(13)
                .Bold();

            col.Item().Element(c => BuildMiniInfoTable(
                c,
                new List<(string, string)>
                {
                    ("Tổng khóa học mới", $"{model.SoKhoaHocMoi:N0}")
                }));
        }

        private static void BuildExamsSection(ColumnDescriptor col, DashboardReportVm model)
        {
            col.Item().Text("1. Kỳ thi mới theo tháng")
                .FontSize(13)
                .Bold();

            col.Item().Element(c => BuildChartTable(
                c,
                "Tháng",
                "Số kỳ thi",
                model.NewExamsByMonth,
                false));

            col.Item().Text("2. Phân bố kỳ thi theo loại")
                .FontSize(13)
                .Bold();

            col.Item().Element(c => BuildPieListTable(
                c,
                "Loại kỳ thi",
                "Số lượng",
                model.ExamsByType));

            col.Item().Text("3. Tổng hợp kỳ thi")
                .FontSize(13)
                .Bold();

            col.Item().Element(c => BuildMiniInfoTable(
                c,
                new List<(string, string)>
                {
                    ("Tổng kỳ thi mới", $"{model.SoKyThiMoi:N0}")
                }));
        }

        private static void BuildChartTable(
            IContainer container,
            string col1,
            string col2,
            List<ChartPointVm> data,
            bool isMoney)
        {
            if (data == null || data.Count == 0)
            {
                container.Element(c => EmptyBlock(c, "Không có dữ liệu"));
                return;
            }

            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text(col1);
                    h.Cell().Element(HeaderCell).AlignRight().Text(col2);
                });

                foreach (var item in data)
                {
                    table.Cell().Element(BodyCell).Text(item.Label);
                    table.Cell().Element(BodyCell).AlignRight()
                        .Text(isMoney ? item.Value.ToString("N0") : item.Value.ToString("N0"));
                }
            });
        }

        private static void BuildPieListTable(
            IContainer container,
            string col1,
            string col2,
            List<PieSliceVm> data)
        {
            if (data == null || data.Count == 0)
            {
                container.Element(c => EmptyBlock(c, "Không có dữ liệu"));
                return;
            }

            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.ConstantColumn(100);
                });

                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text(col1);
                    h.Cell().Element(HeaderCell).AlignRight().Text(col2);
                });

                foreach (var item in data)
                {
                    table.Cell().Element(BodyCell).Text(item.Label);
                    table.Cell().Element(BodyCell).AlignRight().Text(item.Value.ToString("N0"));
                }
            });
        }

        private static void BuildMiniInfoTable(
            IContainer container,
            List<(string Label, string Value)> items)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                foreach (var item in items)
                {
                    table.Cell().Element(BodyCell).Text(item.Label);
                    table.Cell().Element(BodyCell).AlignRight().Text(item.Value);
                }
            });
        }

        private static void EmptyBlock(IContainer container, string text)
        {
            container.Border(1)
                     .BorderColor("#E2E8F0")
                     .Background("#F8FAFC")
                     .Padding(16)
                     .AlignCenter()
                     .AlignMiddle()
                     .Text(text)
                     .FontColor("#64748B")
                     .SemiBold();
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .Background("#F1F5F9")
                .BorderBottom(1)
                .BorderColor("#E2E8F0")
                .PaddingVertical(8)
                .PaddingHorizontal(10)
                .DefaultTextStyle(x => x.SemiBold().FontColor("#0F172A"));
        }

        private static IContainer BodyCell(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor("#E2E8F0")
                .PaddingVertical(8)
                .PaddingHorizontal(10)
                .DefaultTextStyle(x => x.FontColor("#0F172A"));
        }
    }
}