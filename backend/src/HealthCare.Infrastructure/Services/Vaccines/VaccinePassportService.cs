using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Vaccines.DTOs;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HealthCare.Infrastructure.Services.Vaccines;

public class VaccinePassportService : IVaccinePassportService
{
    private static readonly string PrimaryColor = "#4e73df";
    private static readonly string SuccessColor = "#1cc88a";
    private static readonly string TextMuted    = "#858796";

    static VaccinePassportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(VaccinePassportData data)
    {
        var qrPng = GenerateQrCode(data);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(c => ComposeContent(c, data, qrPng));
                page.Footer().Element(ComposeFooter);
            });
        });

        return doc.GeneratePdf();
    }

    // ── QR Code ────────────────────────────────────────────────────────────
    private static byte[] GenerateQrCode(VaccinePassportData data)
    {
        var completed = data.Records.Count(r => r.Status == "completed");
        var content =
            $"HEALTH+ VACCINE PASSPORT\n" +
            $"Ho ten: {data.FullName}\n" +
            $"Nhom mau: {data.BloodType ?? "Chua cap nhat"}\n" +
            $"So mui da tiem: {completed}/{data.Records.Count}\n" +
            $"Ngay cap: {data.GeneratedAt:dd/MM/yyyy HH:mm} UTC";

        using var gen  = new QRCodeGenerator();
        var qrData     = gen.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        var qrCode     = new PngByteQRCode(qrData);
        return qrCode.GetGraphic(pixelsPerModule: 6);
    }

    // ── Header ─────────────────────────────────────────────────────────────
    private static void ComposeHeader(IContainer container)
    {
        container.BorderBottom(2).BorderColor(PrimaryColor).PaddingBottom(12).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("HEALTH+")
                    .FontSize(22).Bold().FontColor(PrimaryColor);
                col.Item().Text("VACCINE PASSPORT")
                    .FontSize(11).FontColor(TextMuted).LetterSpacing(0.1f);
            });

            row.ConstantItem(70).AlignRight().AlignMiddle()
                .Background(PrimaryColor)
                .Width(56).Height(56)
                .AlignCenter().AlignMiddle()
                .Text("H+").FontSize(22).Bold().FontColor("#ffffff");
        });
    }

    // ── Content ────────────────────────────────────────────────────────────
    private static void ComposeContent(IContainer container, VaccinePassportData data, byte[] qrPng)
    {
        container.PaddingTop(16).Column(col =>
        {
            // Patient info card với QR code
            col.Item()
                .Background("#f8f9fc")
                .Border(1).BorderColor("#d1d3e2")
                .Padding(14)
                .Row(infoRow =>
                {
                    // Left: thông tin bệnh nhân
                    infoRow.RelativeItem().Column(info =>
                    {
                        info.Item().Text("THÔNG TIN BỆNH NHÂN")
                            .FontSize(9).Bold().FontColor(PrimaryColor).LetterSpacing(0.05f);

                        info.Item().PaddingTop(10).LabelValue("Họ và tên", data.FullName);
                        info.Item().PaddingTop(6).LabelValue("Email", data.Email);
                        info.Item().PaddingTop(6).LabelValue("Nhóm máu", data.BloodType ?? "Chưa cập nhật");
                        info.Item().PaddingTop(6).LabelValue(
                            "Ngày cấp",
                            data.GeneratedAt.ToString("dd/MM/yyyy HH:mm") + " UTC");

                        var completed = data.Records.Count(r => r.Status == "completed");
                        info.Item().PaddingTop(6).LabelValue(
                            "Số mũi đã tiêm",
                            $"{completed} / {data.Records.Count}");
                    });

                    // Right: QR code
                    infoRow.ConstantItem(100).AlignCenter().AlignMiddle().Column(qrCol =>
                    {
                        qrCol.Item().AlignCenter()
                            .Width(80).Height(80)
                            .Image(qrPng);
                        qrCol.Item().PaddingTop(4).AlignCenter()
                            .Text("Quét để xác minh")
                            .FontSize(7).FontColor(TextMuted);
                    });
                });

            col.Item().PaddingTop(20).Text("LỊCH SỬ TIÊM CHỦNG")
                .FontSize(10).Bold().FontColor(PrimaryColor).LetterSpacing(0.05f);

            col.Item().PaddingTop(8).Element(c => ComposeTable(c, data.Records));
        });
    }

    // ── Table ──────────────────────────────────────────────────────────────
    private static void ComposeTable(IContainer container, IReadOnlyList<VaccineRecordDto> records)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(3);  // Vắc-xin
                cols.ConstantColumn(35); // Mũi
                cols.ConstantColumn(65); // Ngày tiêm
                cols.RelativeColumn(3);  // Cơ sở
                cols.RelativeColumn(2);  // Lô số
                cols.ConstantColumn(65); // Hạn tiêm tiếp
            });

            static IContainer HeaderCell(IContainer c) =>
                c.Background(PrimaryColor).Padding(6).AlignCenter();

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Vắc-xin").FontColor("#ffffff").Bold().FontSize(9);
                header.Cell().Element(HeaderCell).Text("Mũi").FontColor("#ffffff").Bold().FontSize(9);
                header.Cell().Element(HeaderCell).Text("Ngày tiêm").FontColor("#ffffff").Bold().FontSize(9);
                header.Cell().Element(HeaderCell).Text("Cơ sở tiêm").FontColor("#ffffff").Bold().FontSize(9);
                header.Cell().Element(HeaderCell).Text("Lô số").FontColor("#ffffff").Bold().FontSize(9);
                header.Cell().Element(HeaderCell).Text("Mũi tiếp").FontColor("#ffffff").Bold().FontSize(9);
            });

            if (records.Count == 0)
            {
                table.Cell().ColumnSpan(6)
                    .Padding(20).AlignCenter()
                    .Text("Chưa có bản ghi tiêm chủng nào.").FontColor(TextMuted);
                return;
            }

            for (var i = 0; i < records.Count; i++)
            {
                var r  = records[i];
                var bg = i % 2 == 0 ? "#ffffff" : "#f8f9fc";

                IContainer DataCell(IContainer c) =>
                    c.Background(bg).BorderBottom(1).BorderColor("#e3e6f0").Padding(6);

                table.Cell().Element(DataCell).Text(r.VaccineName).FontSize(9);
                table.Cell().Element(DataCell).AlignCenter().Text($"#{r.DoseNumber}").FontSize(9);
                table.Cell().Element(DataCell).AlignCenter()
                    .Text(r.InjectionDate.ToString("dd/MM/yyyy")).FontSize(9);
                table.Cell().Element(DataCell)
                    .Text(r.Facility ?? "—").FontSize(9)
                    .FontColor(r.Facility == null ? TextMuted : "#000000");
                table.Cell().Element(DataCell).AlignCenter()
                    .Text(r.LotNumber ?? "—").FontSize(9)
                    .FontColor(r.LotNumber == null ? TextMuted : "#000000");

                var nextText = r.NextDueDate?.ToString("dd/MM/yyyy") ?? "Hoàn thành";
                string nextColor;
                if (r.Status == "overdue")       nextColor = "#e74a3b";
                else if (r.Status == "upcoming") nextColor = "#f6c23e";
                else if (r.NextDueDate == null)  nextColor = SuccessColor;
                else                             nextColor = "#000000";

                table.Cell().Element(DataCell).AlignCenter()
                    .Text(nextText).FontSize(9).FontColor(nextColor);
            }
        });
    }

    // ── Footer ─────────────────────────────────────────────────────────────
    private static void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor("#d1d3e2").PaddingTop(8).Row(row =>
        {
            row.RelativeItem().Text(t =>
            {
                t.Span("Tài liệu được tạo tự động bởi ").FontSize(8).FontColor(TextMuted);
                t.Span("Health+").FontSize(8).Bold().FontColor(PrimaryColor);
                t.Span(" — Không có giá trị thay thế hồ sơ y tế chính thức.").FontSize(8).FontColor(TextMuted);
            });

            row.ConstantItem(60).AlignRight().Text(t =>
            {
                t.Span("Trang ").FontSize(8).FontColor(TextMuted);
                t.CurrentPageNumber().FontSize(8).FontColor(TextMuted);
                t.Span(" / ").FontSize(8).FontColor(TextMuted);
                t.TotalPages().FontSize(8).FontColor(TextMuted);
            });
        });
    }
}

file static class ContainerExtensions
{
    public static void LabelValue(this IContainer container, string label, string value)
    {
        container.Row(row =>
        {
            row.ConstantItem(75).Text(label + ":").FontSize(9).FontColor("#858796");
            row.RelativeItem().Text(value).FontSize(9).Bold();
        });
    }
}
