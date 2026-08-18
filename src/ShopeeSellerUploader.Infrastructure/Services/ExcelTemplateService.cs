using ClosedXML.Excel;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Infrastructure.Configuration;

namespace ShopeeSellerUploader.Infrastructure.Services;

public sealed class ExcelTemplateService : IExcelTemplateService
{
    private readonly PathProvider _pathProvider;

    public ExcelTemplateService(PathProvider pathProvider)
    {
        _pathProvider = pathProvider;
    }

    public Task<string> EnsureTemplateAsync(CancellationToken cancellationToken = default)
    {
        if (File.Exists(_pathProvider.TemplateFilePath))
        {
            return Task.FromResult(_pathProvider.TemplateFilePath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_pathProvider.TemplateFilePath)!);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Products");
        var headers = new[]
        {
            "ProductCode", "ProductName", "Description", "Category", "Price", "Stock", "Weight",
            "Length", "Width", "Height", "SKU", "Image1", "Image2", "Image3", "Image4",
            "VariationName", "VariationOption", "VariationPrice", "VariationStock"
        };

        for (var index = 0; index < headers.Length; index++)
        {
            sheet.Cell(1, index + 1).Value = headers[index];
            sheet.Cell(1, index + 1).Style.Font.Bold = true;
        }

        var sampleRow = new string[]
        {
            "SKU-001", "Sample Product", "Sample product description", "Women Fashion > T-Shirts", "199.00", "10", "0.35",
            "20", "15", "3", "SKU-001-A", @"C:\Images\product-1.jpg", @"C:\Images\product-2.jpg", @"C:\Images\product-3.jpg", @"C:\Images\product-4.jpg",
            "Color", "Red", "209.00", "5"
        };

        for (var index = 0; index < sampleRow.Length; index++)
        {
            sheet.Cell(2, index + 1).SetValue(sampleRow[index]);
        }

        sheet.Columns().AdjustToContents();
        workbook.SaveAs(_pathProvider.TemplateFilePath);
        return Task.FromResult(_pathProvider.TemplateFilePath);
    }
}
