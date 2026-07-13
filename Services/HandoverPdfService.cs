using System;
using System.IO;
using ItAssets.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ItAssets.Services
{
    public class HandoverPdfService
    {
        public byte[] GenerateHandoverPdf(Asset asset, string signatureBase64)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(x => ComposeContent(x, asset, signatureBase64));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("IT ASSET HANDOVER FORM").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"Date: {DateTime.Now:d}").FontSize(14);
                });
            });
        }

        private void ComposeContent(IContainer container, Asset asset, string signatureBase64)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(20);

                column.Item().Text("I acknowledge receipt of the following company property. I agree to use it responsibly and return it in good condition when requested or upon termination of employment.").FontSize(12);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(7);
                    });

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Asset Tag").SemiBold();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(asset.AssetTag);

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Device Type").SemiBold();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(asset.DeviceType);

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Make/Model").SemiBold();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(asset.MakeModel);

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Serial Number").SemiBold();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(asset.SerialNumber);

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Assigned User").SemiBold();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(asset.User?.Name ?? "Unassigned");
                    
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("User Email").SemiBold();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(asset.User?.Email ?? "N/A");
                });

                column.Item().PaddingTop(25).Row(row =>
                {
                    row.RelativeItem().Column(signatureColumn =>
                    {
                        signatureColumn.Item().Text("User Signature:").SemiBold();
                        if (!string.IsNullOrEmpty(signatureBase64))
                        {
                            var base64Data = signatureBase64.Substring(signatureBase64.IndexOf(",") + 1);
                            var imageBytes = Convert.FromBase64String(base64Data);
                            signatureColumn.Item().Width(200).Image(imageBytes);
                        }
                        else
                        {
                            signatureColumn.Item().Height(100).Width(200).Border(1).BorderColor(Colors.Grey.Medium);
                        }
                        
                        signatureColumn.Item().PaddingTop(5).Text(asset.User?.Name ?? "_______________________");
                    });
                });
            });
        }
    }
}
