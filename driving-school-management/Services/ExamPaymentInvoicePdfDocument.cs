using driving_school_management.Models.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace driving_school_management.Services
{
    public class ExamPaymentInvoicePdfDocument : IDocument
    {
        private readonly ExamPaymentInvoiceDto _model;
        private readonly DateTime _exportedAt;
        private readonly string _brandLogoPath;
        private readonly string? _paymentLogoPath;

        public ExamPaymentInvoicePdfDocument(
            ExamPaymentInvoiceDto model,
            DateTime exportedAt,
            string brandLogoPath,
            string? paymentLogoPath)
        {
            _model = model;
            _exportedAt = exportedAt;
            _brandLogoPath = brandLogoPath;
            _paymentLogoPath = paymentLogoPath;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10).FontColor(Colors.Grey.Darken4));

                page.Background().AlignCenter().AlignMiddle().Rotate(-32).Text("TRUNG TÂM ĐÀO TẠO GPLX - ORACLE")
                    .FontFamily("Arial")
                    .FontSize(36)
                    .FontColor(Colors.Grey.Lighten3)
                    .Bold();

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(90).Height(52).AlignMiddle().Image(_brandLogoPath, ImageScaling.FitArea);

                    row.RelativeItem().PaddingLeft(12).Column(c =>
                    {
                        c.Item().Text("TRUNG TÂM ĐÀO TẠO GPLX")
                            .FontSize(18).Bold().FontColor("#0F172A");
                        c.Item().Text("HÓA ĐƠN THANH TOÁN KỲ THI")
                            .FontSize(13).SemiBold().FontColor("#475569");
                        c.Item().Text($"Thời gian xuất hóa đơn: {_exportedAt:dd/MM/yyyy HH:mm:ss}")
                            .FontSize(9).FontColor("#64748B");
                    });

                    row.ConstantItem(150).AlignRight().Column(c =>
                    {
                        c.Item().Text($"Mã phiếu: #{_model.PhieuId}")
                            .FontSize(12).Bold().AlignRight().FontColor("#111827");
                        c.Item().Text($"Ngày thanh toán: {(_model.NgayNop.HasValue ? _model.NgayNop.Value.ToString("dd/MM/yyyy HH:mm:ss") : "Chưa thanh toán")}")
                            .FontSize(9).AlignRight().FontColor("#64748B");
                    });
                });

                col.Item().PaddingTop(12).LineHorizontal(1).LineColor("#E5E7EB");
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingTop(18).Column(col =>
            {
                col.Spacing(16);

                col.Item().Row(row =>
                {
                    row.RelativeItem(7).Element(ComposeHeroBlock);
                    row.RelativeItem(4).Element(ComposePaymentSummaryBlock);
                });

                col.Item().Text("THÔNG TIN THANH TOÁN")
                    .FontSize(11).Bold().FontColor("#64748B");

                col.Item().Element(x => ComposeInfoGrid(x, new List<(string Label, string Value)>
                {
                    ("Mã phiếu", $"#{_model.PhieuId}"),
                    ("Tên phiếu", _model.TenPhieu),
                    ("Loại phí", _model.LoaiPhi),
                    ("Phương thức thanh toán", _model.PhuongThuc),
                    ("Tổng tiền", $"{_model.TongTien:N0} VNĐ"),
                    ("Trạng thái thanh toán", _model.TrangThaiThanhToan),
                    ("Ngày lập phiếu", _model.NgayLap?.ToString("dd/MM/yyyy HH:mm:ss") ?? ""),
                    ("Ngày thanh toán", _model.NgayNop?.ToString("dd/MM/yyyy HH:mm:ss") ?? ""),
                    ("Ghi chú / Nội dung chuyển khoản", _model.GhiChu)
                }, 2));

                col.Item().Text("THÔNG TIN HỌC VIÊN VÀ HỒ SƠ")
                    .FontSize(11).Bold().FontColor("#64748B");

                col.Item().Element(x => ComposeInfoGrid(x, new List<(string Label, string Value)>
                {
                    ("Học viên", _model.HoTenHocVien),
                    ("Mã học viên", _model.HocVienId.ToString()),
                    ("Số điện thoại", _model.Sdt),
                    ("Email", _model.Email),
                    ("Mã hồ sơ", _model.HoSoId.ToString()),
                    ("Tên hồ sơ", _model.TenHoSo),
                    ("Trạng thái hồ sơ", _model.TrangThaiHoSo)
                }, 2));

                col.Item().Text("THÔNG TIN KỲ THI")
                    .FontSize(11).Bold().FontColor("#64748B");

                col.Item().Element(x => ComposeInfoGrid(x, new List<(string Label, string Value)>
                {
                    ("Kỳ thi", _model.TenKyThi),
                    ("Mã kỳ thi", _model.KyThiId.ToString()),
                    ("Loại kỳ thi", _model.LoaiKyThi),
                    ("Hạng", _model.TenHang),
                    ("Thời gian thi", _model.ThoiGianThi?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Chưa cập nhật"),
                    ("Địa điểm thi", _model.DiaDiemThi),
                    ("Học phí gốc", $"{_model.HocPhi:N0} VNĐ")
                }, 2));
            });
        }

        private void ComposeHeroBlock(IContainer container)
        {
            container
                .Background("#F8FAFC")
                .Border(1)
                .BorderColor("#E5E7EB")
                .Padding(20)
                .Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text("Thông tin quan trọng")
                        .FontSize(10).Bold().FontColor("#64748B");

                    col.Item().Text(_model.TenKyThi)
                        .FontSize(21).Bold().FontColor("#0F172A");

                    col.Item().Row(row =>
                    {
                        row.AutoItem().Element(x => ComposeMiniTag(x, _model.TenHang));
                        row.AutoItem().PaddingLeft(8).Element(x => ComposeMiniTag(x, _model.LoaiKyThi));
                        row.AutoItem().PaddingLeft(8).Element(x => ComposeMiniTag(x, _model.TrangThaiThanhToan));
                    });

                    col.Item().PaddingTop(4).Text($"Học viên: {_model.HoTenHocVien}")
                        .FontSize(11).SemiBold().FontColor("#111827");

                    col.Item().Text($"Hồ sơ: {_model.TenHoSo}")
                        .FontSize(10).FontColor("#475569");
                });
        }

        private void ComposePaymentSummaryBlock(IContainer container)
        {
            container
                .Background("#0F172A")
                .Padding(20)
                .Column(col =>
                {
                    col.Spacing(12);

                    if (!string.IsNullOrWhiteSpace(_paymentLogoPath))
                    {
                        col.Item().Height(28).AlignLeft().Image(_paymentLogoPath!, ImageScaling.FitHeight);
                    }

                    col.Item().Text("Tổng thanh toán")
                        .FontSize(10).FontColor("#CBD5E1").Bold();

                    col.Item().Text($"{_model.TongTien:N0} VNĐ")
                        .FontSize(24).Bold().FontColor(Colors.White);

                    col.Item().Text($"Trạng thái: {_model.TrangThaiThanhToan}")
                        .FontSize(11).SemiBold().FontColor("#E2E8F0");

                    col.Item().Text($"Phương thức: {_model.PhuongThuc}")
                        .FontSize(11).SemiBold().FontColor("#E2E8F0");

                    col.Item().Text($"Ngày thanh toán: {(_model.NgayNop.HasValue ? _model.NgayNop.Value.ToString("dd/MM/yyyy HH:mm:ss") : "Chưa thanh toán")}")
                        .FontSize(9).FontColor("#CBD5E1");
                });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor("#E5E7EB");

                col.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Text("TRUNG TÂM ĐÀO TẠO GPLX - ORACLE")
                        .FontSize(9)
                        .FontColor("#64748B");

                    row.RelativeItem()
                        .AlignRight()
                        .DefaultTextStyle(x => x.FontSize(9).FontColor("#64748B"))
                        .Text(text =>
                        {
                            text.Span("Trang ");
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                });
            });
        }

        private void ComposeInfoGrid(IContainer container, List<(string Label, string Value)> items, int columns)
        {
            container.Grid(grid =>
            {
                grid.Columns(columns);
                grid.HorizontalSpacing(12);
                grid.VerticalSpacing(12);

                foreach (var item in items)
                {
                    grid.Item().Element(x => ComposeInfoCard(x, item.Label, item.Value));
                }
            });
        }

        private void ComposeInfoCard(IContainer container, string label, string value)
        {
            container
                .Background("#FCFCFD")
                .Border(1)
                .BorderColor("#E5E7EB")
                .Padding(14)
                .Column(col =>
                {
                    col.Spacing(6);
                    col.Item().Text(label)
                        .FontSize(9)
                        .Bold()
                        .FontColor("#64748B");
                    col.Item().Text(string.IsNullOrWhiteSpace(value) ? "-" : value)
                        .FontSize(11)
                        .SemiBold()
                        .FontColor("#111827");
                });
        }

        private void ComposeMiniTag(IContainer container, string? text)
        {
            container
                .Background("#FFFFFF")
                .Border(1)
                .BorderColor("#E5E7EB")
                .PaddingVertical(5)
                .PaddingHorizontal(10)
                .Text(string.IsNullOrWhiteSpace(text) ? "-" : text)
                .FontSize(9)
                .SemiBold()
                .FontColor("#334155");
        }
    }
}