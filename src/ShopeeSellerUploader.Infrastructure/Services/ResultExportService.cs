using ClosedXML.Excel;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Infrastructure.Services;

public sealed class ResultExportService : IResultExportService
{
    public Task ExportAsync(string filePath, IEnumerable<ProductProcessResult> results, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Results");

        var headers = new[] { "RowNumber", "ProductCode", "ProductName", "Status", "Message", "Attempts", "WasResumed", "UpdatedAt" };
        for (var index = 0; index < headers.Length; index++)
        {
            sheet.Cell(1, index + 1).Value = headers[index];
            sheet.Cell(1, index + 1).Style.Font.Bold = true;
        }

        var rowIndex = 2;
        foreach (var result in results)
        {
            cancellationToken.ThrowIfCancellationRequested();
            sheet.Cell(rowIndex, 1).Value = result.RowNumber;
            sheet.Cell(rowIndex, 2).Value = result.ProductCode;
            sheet.Cell(rowIndex, 3).Value = result.ProductName;
            sheet.Cell(rowIndex, 4).Value = result.Status.ToString();
            sheet.Cell(rowIndex, 5).Value = result.Message;
            sheet.Cell(rowIndex, 6).Value = result.Attempts;
            sheet.Cell(rowIndex, 7).Value = result.WasResumed;
            sheet.Cell(rowIndex, 8).Value = result.UpdatedAt.LocalDateTime;
            rowIndex++;
        }

        sheet.Columns().AdjustToContents();
        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }
}
