using ClosedXML.Excel;
using Serilog;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Infrastructure.Services;

public sealed class ExcelProductReader : IExcelProductReader
{
    private readonly ILogger _logger;

    public ExcelProductReader(ILogger logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyList<ProductRecord>> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Excel file not found.", filePath);
        }

        var excelDirectory = Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? AppContext.BaseDirectory;
        var products = new List<ProductRecord>();
        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheet(1);
        var rows = sheet.RangeUsed()?.RowsUsed().Skip(1) ?? Enumerable.Empty<IXLRangeRow>();

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var product = new ProductRecord
            {
                RowNumber = row.RowNumber(),
                ProductCode = row.Cell(1).GetString().Trim(),
                ProductName = row.Cell(2).GetString().Trim(),
                Description = row.Cell(3).GetString().Trim(),
                Category = row.Cell(4).GetString().Trim(),
                Price = row.Cell(5).GetValue<decimal>(),
                Stock = row.Cell(6).GetValue<int>(),
                Weight = row.Cell(7).GetValue<decimal>(),
                Length = row.Cell(8).GetValue<decimal>(),
                Width = row.Cell(9).GetValue<decimal>(),
                Height = row.Cell(10).GetValue<decimal>(),
                SKU = row.Cell(11).GetString().Trim(),
                Image1 = NormalizePath(row.Cell(12).GetString(), excelDirectory),
                Image2 = NormalizePath(row.Cell(13).GetString(), excelDirectory),
                Image3 = NormalizePath(row.Cell(14).GetString(), excelDirectory),
                Image4 = NormalizePath(row.Cell(15).GetString(), excelDirectory),
                VariationName = row.Cell(16).GetString().Trim(),
                VariationOption = row.Cell(17).GetString().Trim(),
                VariationPrice = TryGetNullableDecimal(row.Cell(18).GetString()),
                VariationStock = TryGetNullableInt(row.Cell(19).GetString())
            };

            if (string.IsNullOrWhiteSpace(product.ProductCode) &&
                string.IsNullOrWhiteSpace(product.ProductName) &&
                string.IsNullOrWhiteSpace(product.SKU))
            {
                continue;
            }

            products.Add(product);
        }

        _logger.Information("Loaded {Count} products from {FilePath}", products.Count, filePath);
        return Task.FromResult<IReadOnlyList<ProductRecord>>(products);
    }

    private static string NormalizePath(string value, string excelDirectory)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim()
            .Trim('"')
            .Trim('\'')
            .Replace('/', Path.DirectorySeparatorChar);

        if (normalized.StartsWith("file:///", StringComparison.OrdinalIgnoreCase) &&
            Uri.TryCreate(normalized, UriKind.Absolute, out var uri) &&
            uri.IsFile)
        {
            normalized = uri.LocalPath;
        }

        normalized = Environment.ExpandEnvironmentVariables(normalized);

        if (Path.IsPathRooted(normalized))
        {
            return Path.GetFullPath(normalized);
        }

        return Path.GetFullPath(Path.Combine(excelDirectory, normalized));
    }

    private static decimal? TryGetNullableDecimal(string value) => decimal.TryParse(value, out var parsed) ? parsed : null;
    private static int? TryGetNullableInt(string value) => int.TryParse(value, out var parsed) ? parsed : null;
}
