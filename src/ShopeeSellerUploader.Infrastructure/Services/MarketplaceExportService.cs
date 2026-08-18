using ClosedXML.Excel;
using ShopeeSellerUploader.Contracts.Configuration;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Core.Utilities;
using ShopeeSellerUploader.Infrastructure.Configuration;
using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace ShopeeSellerUploader.Infrastructure.Services;

public sealed class MarketplaceExportService : IMarketplaceExportService
{
    private const string ShopeeFormSheetName = "à¹à¸šà¸šà¸Ÿà¸­à¸£à¹Œà¸¡à¸à¸²à¸£à¸¥à¸‡à¸ªà¸´à¸™à¸„à¹‰à¸²";
    private const int ShopeeDataStartRow = 7;
    private const string ShopeeSizeChartSheetName = "รายการตารางขนาดสินค้า";
    private const int ShopeeSizeChartDataStartRow = 7;
    private const int LazadaDataStartRow = 6;
    private const int LazadaVisibleDataStartRow = 5;
    private const string TikTokSheetName = "Template";
    private const int TikTokHeaderRow = 1;
    private const int TikTokDataStartRow = 6;
    private const string LazadaIndexSheetName = "INDEX";
    private const string LazadaStatusSheetName = "à¸ªà¸–à¸²à¸™à¸°";
    private const string LazadaGlobalHideSheetName = "global_hide";

    private sealed record VariationExportEntry(string Option, decimal? Price, int? Stock, string ImageUrl);

    private static readonly string[] ShopeeFieldKeys =
    [
        "ps_category|0|0",
        "ps_product_name|1|0",
        "ps_product_description|1|0",
        "ps_maximum_purchase_quantity|0|0",
        "ps_maximum_purchase_quantity_start_date|0|0",
        "ps_maximum_purchase_quantity_time_period|0|0",
        "ps_maximum_purchase_quantity_end_date|0|0",
        "ps_minimum_purchase_quantity|0|0",
        "ps_sku_parent_short|0|0",
        "et_title_variation_integration_no|0|0",
        "et_title_variation_1|0|0",
        "et_title_option_for_variation_1|0|0",
        "et_title_image_per_variation|0|3",
        "et_title_variation_2|0|0",
        "et_title_option_for_variation_2|0|0",
        "ps_price|1|1",
        "ps_stock|0|1",
        "ps_sku_short|0|0",
        "ps_new_size_chart|0|1",
        "et_title_size_chart|0|3",
        "ps_gtin_code|0|0",
        "ps_item_cover_image|0|3",
        "ps_item_image_1|0|3",
        "ps_item_image_2|0|3",
        "ps_item_image_3|0|3",
        "ps_item_image_4|0|3",
        "ps_item_image_5|0|3",
        "ps_item_image_6|0|3",
        "ps_item_image_7|0|3",
        "ps_item_image_8|0|3",
        "ps_weight|0|1",
        "ps_length|0|1",
        "ps_width|0|1",
        "ps_height|0|1",
        "channel_id.7000|0|0",
        "ps_product_pre_order_dts|0|1",
        "et_title_reason|0|0"
    ];

    private static readonly string[] ShopeeTemplateMetadata =
    [
        "basic",
        "8dddd2f7d90a3b8728b28316f98263b6",
        "0",
        "72444654"
    ];

    private static readonly HashSet<string> ShopeeFieldKeySet = ShopeeFieldKeys
        .Select(NormalizeHeader)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static readonly string[] TikTokFieldKeys =
    [
        "category",
        "brand",
        "product_name",
        "product_description",
        "main_image",
        "image_2",
        "image_3",
        "image_4",
        "image_5",
        "image_6",
        "image_7",
        "image_8",
        "image_9",
        "property_name_1",
        "property_value_1",
        "property_1_image",
        "property_name_2",
        "property_value_2",
        "parcel_weight",
        "parcel_length",
        "parcel_width",
        "parcel_height",
        "delivery",
        "price",
        "pre_order_time",
        "quantity",
        "seller_sku",
        "size_chart"
    ];

    private readonly PathProvider _pathProvider;
    private readonly AppSettings _settings;

    public MarketplaceExportService(PathProvider pathProvider, AppSettings settings)
    {
        _pathProvider = pathProvider;
        _settings = settings;
    }

    public Task<string> ExportAsync(
        MarketplaceType marketplace,
        IEnumerable<ProductItem> products,
        string outputFilePath,
        IReadOnlyDictionary<string, CategoryMapping>? categoryMappings = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var productList = ExpandProductsForExport(marketplace, products).ToList();
            if (productList.Count == 0)
            {
                throw new InvalidOperationException("At least one product must be selected.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)!);
            var templatePath = ResolveTemplatePath(marketplace);
            var preparedTemplatePath = PrepareTemplatePath(templatePath, marketplace);

            try
            {
                if (marketplace == MarketplaceType.Lazada && File.Exists(preparedTemplatePath))
                {
                    File.Copy(preparedTemplatePath, outputFilePath, true);
                    WriteLazadaPreservingTemplate(outputFilePath, productList, categoryMappings, cancellationToken);
                    return outputFilePath;
                }

                if (marketplace == MarketplaceType.TikTok && File.Exists(preparedTemplatePath))
                {
                    File.Copy(preparedTemplatePath, outputFilePath, true);
                    WriteTikTokPreservingTemplate(outputFilePath, productList, categoryMappings, cancellationToken);
                    return outputFilePath;
                }

                using var workbook = File.Exists(preparedTemplatePath)
                    ? new XLWorkbook(preparedTemplatePath)
                    : new XLWorkbook();

                if (workbook.Worksheets.Count == 0)
                {
                    workbook.Worksheets.Add(GetDefaultWorksheetName(marketplace));
                }

                switch (marketplace)
                {
                    case MarketplaceType.Shopee:
                        WriteShopee(workbook, productList, categoryMappings, cancellationToken);
                        break;
                    case MarketplaceType.Lazada:
                        WriteLazada(workbook, productList, categoryMappings, cancellationToken);
                        break;
                    case MarketplaceType.TikTok:
                        WriteTikTok(workbook, productList, categoryMappings, cancellationToken);
                        break;
                    default:
                        throw new InvalidOperationException($"Marketplace '{marketplace}' is not supported.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                workbook.SaveAs(outputFilePath);
                return outputFilePath;
            }
            finally
            {
                if (!string.Equals(preparedTemplatePath, templatePath, StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(preparedTemplatePath))
                {
                    File.Delete(preparedTemplatePath);
                }
            }
        }, cancellationToken);
    }

    private static IEnumerable<ProductItem> ExpandProductsForExport(
        MarketplaceType marketplace,
        IEnumerable<ProductItem> products)
    {
        if (marketplace == MarketplaceType.Shopee)
        {
            foreach (var product in products)
            {
                yield return product;
            }

            yield break;
        }

        foreach (var product in products)
        {
            var variationEntries = ParseVariationEntries(product.VariationOption);
            if (variationEntries.Count <= 1)
            {
                yield return product;
                continue;
            }

            var isColorVariation = IsColorVariation(product.VariationName);
            foreach (var entry in variationEntries)
            {
                yield return CloneForVariation(product, entry, isColorVariation);
            }
        }
    }

    private static List<VariationExportEntry> ParseVariationEntries(string? value)
    {
        var text = value ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var lines = text
            .Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (lines.Count > 1 || text.Contains('|'))
        {
            return lines
                .Select(ParseVariationEntryLine)
                .Where(static entry => !string.IsNullOrWhiteSpace(entry.Option))
                .DistinctBy(static entry => entry.Option, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return text
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(static item => new VariationExportEntry(item, null, null, string.Empty))
            .ToList();
    }

    private static VariationExportEntry ParseVariationEntryLine(string line)
    {
        var parts = line.Split('|', StringSplitOptions.TrimEntries);
        var option = parts.ElementAtOrDefault(0)?.Trim() ?? string.Empty;
        var price = TryParseDecimal(parts.ElementAtOrDefault(1));
        var stock = TryParseInt(parts.ElementAtOrDefault(2));
        var imageUrl = parts.ElementAtOrDefault(3)?.Trim() ?? string.Empty;
        return new VariationExportEntry(option, price, stock, imageUrl);
    }

    private static decimal? TryParseDecimal(string? value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ||
               decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out parsed)
            ? parsed
            : null;
    }

    private static int? TryParseInt(string? value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ||
               int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out parsed)
            ? parsed
            : null;
    }

    private static bool IsColorVariation(string? variationName)
    {
        if (string.IsNullOrWhiteSpace(variationName))
        {
            return false;
        }

        var trimmed = variationName.Trim();
        return trimmed.Contains("สี", StringComparison.OrdinalIgnoreCase) ||
               trimmed.Contains("color", StringComparison.OrdinalIgnoreCase);
    }

    private static ProductItem CloneForVariation(ProductItem source, VariationExportEntry entry, bool isColorVariation)
    {
        var cloned = CloneProduct(source);
        cloned.VariationOption = entry.Option;
        cloned.SKU = BuildVariantSku(source.SKU, entry.Option);
        cloned.ProductCode = string.IsNullOrWhiteSpace(source.ProductCode) ? "PRODUCT" : source.ProductCode.Trim();
        cloned.VariationPrice = entry.Price ?? source.VariationPrice;
        cloned.VariationStock = entry.Stock ?? source.VariationStock;
        cloned.VariationImageUrl = entry.ImageUrl;

        if (isColorVariation)
        {
            cloned.ColorFamily = entry.Option;
        }

        return cloned;
    }

    private static string BuildVariantSku(string sku, string variationOption)
    {
        var baseSku = string.IsNullOrWhiteSpace(sku) ? "SKU" : sku.Trim();
        var suffix = BuildSkuSuffix(variationOption);
        return string.IsNullOrWhiteSpace(suffix) ? baseSku : $"{baseSku}-{suffix}";
    }

    private static string BuildParentSku(string productCode, string variationOption)
    {
        var baseCode = string.IsNullOrWhiteSpace(productCode) ? "PRODUCT" : productCode.Trim();
        var suffix = BuildSkuSuffix(variationOption);
        return string.IsNullOrWhiteSpace(suffix) ? baseCode : $"{baseCode}-{suffix}";
    }

    private static string BuildSkuSuffix(string value)
    {
        var input = value.Trim();
        var builder = new StringBuilder();
        foreach (var character in input)
        {
            if (character <= 127 && char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToUpperInvariant(character));
            }
            else if (char.IsWhiteSpace(character) || char.IsPunctuation(character) || char.IsSeparator(character))
            {
                if (builder.Length > 0 && builder[^1] != '-')
                {
                    builder.Append('-');
                }
            }
        }

        var suffix = builder.ToString().Trim('-');
        if (!string.IsNullOrWhiteSpace(suffix))
        {
            return suffix;
        }

        // Shopee accepts ASCII SKUs more reliably, so fall back to a stable hash
        // when the option text does not contain ASCII letters or digits.
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        return $"OPT-{hash[..8]}";
    }

    private static ProductItem CloneProduct(ProductItem source)
    {
        return new ProductItem
        {
            Id = source.Id,
            ProductCode = source.ProductCode,
            ProductName = source.ProductName,
            Description = source.Description,
            Category = source.Category,
            ShopeeCategoryCode = source.ShopeeCategoryCode,
            Price = source.Price,
            Stock = source.Stock,
            Weight = source.Weight,
            Length = source.Length,
            Width = source.Width,
            Height = source.Height,
            SKU = source.SKU,
            Image1 = source.Image1,
            Image2 = source.Image2,
            Image3 = source.Image3,
            Image4 = source.Image4,
            ShopeeImage1Url = source.ShopeeImage1Url,
            ShopeeImage2Url = source.ShopeeImage2Url,
            ShopeeImage3Url = source.ShopeeImage3Url,
            ShopeeImage4Url = source.ShopeeImage4Url,
            LazadaImage1Url = source.LazadaImage1Url,
            LazadaImage2Url = source.LazadaImage2Url,
            LazadaImage3Url = source.LazadaImage3Url,
            LazadaImage4Url = source.LazadaImage4Url,
            VariationName = source.VariationName,
            VariationOption = source.VariationOption,
            VariationPrice = source.VariationPrice,
            VariationStock = source.VariationStock,
            VariationImageUrl = source.VariationImageUrl,
            Brand = source.Brand,
            BabyMaterial = source.BabyMaterial,
            CountryOfOrigin = source.CountryOfOrigin,
            WarrantyType = source.WarrantyType,
            WarrantyPeriod = source.WarrantyPeriod,
            ColorFamily = source.ColorFamily,
            DangerousGoods = source.DangerousGoods,
            DeliveryStandard = source.DeliveryStandard,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    private void WriteShopee(
        XLWorkbook workbook,
        IReadOnlyList<ProductItem> products,
        IReadOnlyDictionary<string, CategoryMapping>? categoryMappings,
        CancellationToken cancellationToken)
    {
        var worksheet = FindShopeeWorksheet(workbook);
        NormalizeShopeeWorksheetSchema(worksheet);
        NormalizeShopeeTemplateArtifacts(workbook, worksheet);
        var headers = ReadShopeeHeaders(worksheet);

        if (headers.Count == 0)
        {
            headers = ShopeeFieldKeys
                .Select((value, index) => new KeyValuePair<int, string>(index + 1, value))
                .ToDictionary(static pair => pair.Key, static pair => pair.Value);
        }

        for (var rowIndex = 0; rowIndex < products.Count; rowIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var rowNumber = ShopeeDataStartRow + rowIndex;
            foreach (var header in headers)
            {
                worksheet.Cell(rowNumber, header.Key).SetValue(MapValue(products[rowIndex], header.Value, categoryMappings));
            }
        }
    }

    private static void NormalizeShopeeWorksheetSchema(IXLWorksheet worksheet)
    {
        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
        for (var column = lastColumn; column >= 1; column--)
        {
            var headerValue = worksheet.Cell(1, column).GetString().Trim();
            if (string.IsNullOrWhiteSpace(headerValue))
            {
                continue;
            }

            if (!ShopeeFieldKeySet.Contains(NormalizeHeader(headerValue)))
            {
                worksheet.Column(column).Delete();
            }
        }

        for (var index = 0; index < ShopeeFieldKeys.Length; index++)
        {
            worksheet.Cell(1, index + 1).SetValue(ShopeeFieldKeys[index]);
        }
    }

    private static void NormalizeShopeeTemplateArtifacts(XLWorkbook workbook, IXLWorksheet worksheet)
    {
        ClearWorksheetRows(worksheet, ShopeeDataStartRow);

        for (var column = 0; column < ShopeeTemplateMetadata.Length; column++)
        {
            worksheet.Cell(2, column + 1).SetValue(ShopeeTemplateMetadata[column]);
        }

        var sizeChartWorksheet = FindShopeeSizeChartWorksheet(workbook);
        if (sizeChartWorksheet is null)
        {
            return;
        }

        ClearWorksheetRows(sizeChartWorksheet, ShopeeSizeChartDataStartRow);
    }

    private static void ClearWorksheetRows(IXLWorksheet worksheet, int startRow)
    {
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        if (lastRow < startRow)
        {
            return;
        }

        worksheet.Rows(startRow, lastRow).Delete();
    }

    private void WriteLazada(
        XLWorkbook workbook,
        IReadOnlyList<ProductItem> products,
        IReadOnlyDictionary<string, CategoryMapping>? categoryMappings,
        CancellationToken cancellationToken)
    {
        var grouped = products.GroupBy(product => ResolveLazadaSheetName(workbook, product, categoryMappings));
        foreach (var group in grouped)
        {
            var worksheet = workbook.Worksheet(group.Key);
            var headers = ReadLazadaHeaders(workbook, worksheet);
            var defaultValues = ReadLazadaDefaultValues(workbook, worksheet);
            var rowIndex = 0;

            foreach (var product in group)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var header in headers)
                {
                    var cell = worksheet.Cell(LazadaDataStartRow + rowIndex, header.Key);
                    var mappedValue = MapValue(product, header.Value, categoryMappings);
                    if (string.IsNullOrWhiteSpace(mappedValue) &&
                        defaultValues.TryGetValue(header.Key, out var defaultValue) &&
                        !string.IsNullOrWhiteSpace(defaultValue))
                    {
                        mappedValue = defaultValue;
                    }

                    if (string.IsNullOrWhiteSpace(mappedValue) && !string.IsNullOrWhiteSpace(cell.GetString()))
                    {
                        continue;
                    }

                    cell.SetValue(mappedValue);
                }

                rowIndex++;
            }
        }
    }

    private void WriteLazadaPreservingTemplate(
        string outputFilePath,
        IReadOnlyList<ProductItem> products,
        IReadOnlyDictionary<string, CategoryMapping>? categoryMappings,
        CancellationToken cancellationToken)
    {
        using var archive = ZipFile.Open(outputFilePath, ZipArchiveMode.Update);
        var workbookDocument = LoadXmlEntry(archive, "xl/workbook.xml");
        var workbookRelationships = LoadXmlEntry(archive, "xl/_rels/workbook.xml.rels");
        var worksheetPaths = ReadWorksheetPaths(workbookDocument, workbookRelationships);
        var visibleWorksheetNames = worksheetPaths.Keys
            .Where(static name =>
                !name.EndsWith("_hide", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(name, LazadaIndexSheetName, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(name, LazadaStatusSheetName, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(name, LazadaGlobalHideSheetName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var grouped = products.GroupBy(product => ResolveLazadaSheetName(visibleWorksheetNames, product, categoryMappings));
        foreach (var group in grouped)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var worksheetName = group.Key;
            var worksheetPath = worksheetPaths[worksheetName];
            var hiddenWorksheetName = $"{worksheetName}_hide";
            worksheetPaths.TryGetValue(hiddenWorksheetName, out var hiddenWorksheetPath);

            var worksheetDocument = LoadXmlEntry(archive, worksheetPath);
            var hiddenWorksheetDocument = hiddenWorksheetPath is null ? null : LoadXmlEntry(archive, hiddenWorksheetPath);
            var headers = hiddenWorksheetDocument is null
                ? new Dictionary<int, string>()
                : ReadXmlHeaders(hiddenWorksheetDocument, 3);
            var defaultValues = hiddenWorksheetDocument is null
                ? new Dictionary<int, string>()
                : ReadXmlDefaultValues(hiddenWorksheetDocument, 5);

            var rowIndex = 0;
            foreach (var product in group)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var header in headers)
                {
                    var mappedValue = MapValue(product, header.Value, categoryMappings);
                    if (string.IsNullOrWhiteSpace(mappedValue) &&
                        defaultValues.TryGetValue(header.Key, out var defaultValue) &&
                        !string.IsNullOrWhiteSpace(defaultValue))
                    {
                        mappedValue = defaultValue;
                    }

                    if (string.IsNullOrWhiteSpace(mappedValue))
                    {
                        continue;
                    }

                    SetXmlCellValue(
                        worksheetDocument,
                        LazadaVisibleDataStartRow + rowIndex,
                        header.Key,
                        header.Value,
                        mappedValue);
                }

                rowIndex++;
            }

            SaveXmlEntry(archive, worksheetPath, worksheetDocument);
        }
    }

    private void WriteTikTokPreservingTemplate(
        string outputFilePath,
        IReadOnlyList<ProductItem> products,
        IReadOnlyDictionary<string, CategoryMapping>? categoryMappings,
        CancellationToken cancellationToken)
    {
        using var archive = ZipFile.Open(outputFilePath, ZipArchiveMode.Update);
        var workbookDocument = LoadXmlEntry(archive, "xl/workbook.xml");
        var workbookRelationships = LoadXmlEntry(archive, "xl/_rels/workbook.xml.rels");
        var sharedStringsDocument = LoadOptionalXmlEntry(archive, "xl/sharedStrings.xml");
        var worksheetPaths = ReadWorksheetPaths(workbookDocument, workbookRelationships);

        if (!worksheetPaths.TryGetValue(TikTokSheetName, out var worksheetPath))
        {
            throw new InvalidOperationException("TikTok template sheet was not found.");
        }

        var worksheetDocument = LoadXmlEntry(archive, worksheetPath);
        var headers = ReadXmlHeaders(worksheetDocument, TikTokHeaderRow, sharedStringsDocument);
        if (headers.Count == 0)
        {
            throw new InvalidOperationException("TikTok template headers are missing.");
        }

        var lastColumn = Math.Max(headers.Keys.Max(), worksheetDocument.Root?
            .Element(XName.Get("sheetData", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"))?
            .Elements(XName.Get("row", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"))
            .SelectMany(static row => row.Elements(XName.Get("c", "http://schemas.openxmlformats.org/spreadsheetml/2006/main")))
            .Select(cell => GetColumnNumberFromCellReference(cell.Attribute("r")?.Value ?? string.Empty))
            .DefaultIfEmpty(headers.Keys.Max())
            .Max() ?? headers.Keys.Max());

        ClearXmlCellsInRange(worksheetDocument, TikTokDataStartRow, lastColumn);

        for (var rowIndex = 0; rowIndex < products.Count; rowIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var rowNumber = TikTokDataStartRow + rowIndex;
            foreach (var header in headers)
            {
                SetXmlCellValue(
                    worksheetDocument,
                    rowNumber,
                    header.Key,
                    header.Value,
                    MapTikTokValue(products[rowIndex], header.Value, categoryMappings));
            }
        }

        SetWorksheetDimension(worksheetDocument, TikTokHeaderRow, lastColumn, TikTokDataStartRow + products.Count - 1);
        SaveXmlEntry(archive, worksheetPath, worksheetDocument);
    }

    private string ResolveTemplatePath(MarketplaceType marketplace)
    {
        string[] patterns;
        if (marketplace == MarketplaceType.Shopee)
        {
            patterns = ["Shopee_mass_upload_*_basic_template.xlsx", "*Shopee*.xlsx", "*shoppee*.xlsx"];
        }
        else if (marketplace == MarketplaceType.TikTok)
        {
            patterns = ["Tiktoksellercenter_batchupload_*_template.xlsx", "*tiktok*template*.xlsx", "*TikTok*.xlsx"];
        }
        else
        {
            patterns = ["*lazada*.xlsx", "*Lazada*.xlsx"];
        }

        var candidateDirectories = GetTemplateSearchDirectories(marketplace)
            .Where(static directory => directory.Exists)
            .ToList();

        foreach (var directory in candidateDirectories)
        {
            foreach (var pattern in patterns)
            {
                var file = directory
                    .GetFiles(pattern)
                    .OrderByDescending(static file => file.LastWriteTimeUtc)
                    .FirstOrDefault();

                if (file is not null)
                {
                    return file.FullName;
                }
            }
        }

        return Path.Combine(
            _pathProvider.TemplateRootDirectory,
            marketplace switch
            {
                MarketplaceType.Shopee => "ShopeeTemplate.xlsx",
                MarketplaceType.TikTok => "TikTokTemplate.xlsx",
                _ => "LazadaTemplate.xlsx"
            });
    }

    private IEnumerable<DirectoryInfo> GetTemplateSearchDirectories(MarketplaceType marketplace)
    {
        yield return new DirectoryInfo(_pathProvider.TemplateRootDirectory);

        if (marketplace is MarketplaceType.Shopee or MarketplaceType.TikTok)
        {
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!string.IsNullOrWhiteSpace(downloadsPath))
            {
                yield return new DirectoryInfo(downloadsPath);
            }
        }
    }

    private static string PrepareTemplatePath(string templatePath, MarketplaceType marketplace)
    {
        if (marketplace != MarketplaceType.Lazada || !File.Exists(templatePath))
        {
            return templatePath;
        }

        using (var sourceArchive = ZipFile.OpenRead(templatePath))
        {
            var stylesEntry = sourceArchive.GetEntry("xl/styles.xml");
            if (stylesEntry is null)
            {
                return templatePath;
            }

            using var reader = new StreamReader(stylesEntry.Open());
            var stylesXml = reader.ReadToEnd();
            if (!stylesXml.Contains("numFmtId=\"-1\"", StringComparison.Ordinal))
            {
                return templatePath;
            }
        }

        var sanitizedTemplatePath = Path.Combine(
            Path.GetTempPath(),
            $"lazada-template-sanitized-{Guid.NewGuid():N}.xlsx");

        File.Copy(templatePath, sanitizedTemplatePath, true);

        using var archive = ZipFile.Open(sanitizedTemplatePath, ZipArchiveMode.Update);
        var entry = archive.GetEntry("xl/styles.xml");
        if (entry is null)
        {
            return sanitizedTemplatePath;
        }

        string xmlContent;
        using (var reader = new StreamReader(entry.Open()))
        {
            xmlContent = reader.ReadToEnd();
        }

        xmlContent = xmlContent.Replace("numFmtId=\"-1\"", "numFmtId=\"0\"", StringComparison.Ordinal);

        entry.Delete();
        var newEntry = archive.CreateEntry("xl/styles.xml");
        using var writer = new StreamWriter(newEntry.Open());
        writer.Write(xmlContent);

        return sanitizedTemplatePath;
    }

    private static string GetDefaultWorksheetName(MarketplaceType marketplace)
    {
        return marketplace switch
        {
            MarketplaceType.Shopee => "Shopee",
            MarketplaceType.Lazada => "Lazada",
            MarketplaceType.TikTok => TikTokSheetName,
            _ => marketplace.ToString()
        };
    }

    private void WriteTikTok(
        XLWorkbook workbook,
        IReadOnlyList<ProductItem> products,
        IReadOnlyDictionary<string, CategoryMapping>? categoryMappings,
        CancellationToken cancellationToken)
    {
        var worksheet = FindTikTokWorksheet(workbook);
        var headers = ReadHeadersFromRow(worksheet, TikTokHeaderRow);
        if (headers.Count == 0)
        {
            headers = TikTokFieldKeys
                .Select((header, index) => new KeyValuePair<int, string>(index + 1, header))
                .ToDictionary();
        }

        ClearTikTokWorksheetData(worksheet, headers.Keys.Max(), products.Count);

        for (var rowIndex = 0; rowIndex < products.Count; rowIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var rowNumber = TikTokDataStartRow + rowIndex;
            foreach (var header in headers)
            {
                worksheet.Cell(rowNumber, header.Key)
                    .SetValue(MapTikTokValue(products[rowIndex], header.Value, categoryMappings));
            }
        }

        if (headers.Count == TikTokFieldKeys.Length)
        {
            for (var index = 0; index < TikTokFieldKeys.Length; index++)
            {
                worksheet.Cell(TikTokHeaderRow, index + 1).SetValue(TikTokFieldKeys[index]);
            }
        }
    }

    private static XDocument LoadXmlEntry(ZipArchive archive, string entryPath)
    {
        var entry = archive.GetEntry(entryPath)
            ?? throw new InvalidOperationException($"Missing archive entry: {entryPath}");

        using var stream = entry.Open();
        return XDocument.Load(stream);
    }

    private static XDocument? LoadOptionalXmlEntry(ZipArchive archive, string entryPath)
    {
        var entry = archive.GetEntry(entryPath);
        if (entry is null)
        {
            return null;
        }

        using var stream = entry.Open();
        return XDocument.Load(stream);
    }

    private static void SaveXmlEntry(ZipArchive archive, string entryPath, XDocument document)
    {
        var existingEntry = archive.GetEntry(entryPath)
            ?? throw new InvalidOperationException($"Missing archive entry: {entryPath}");

        existingEntry.Delete();
        var newEntry = archive.CreateEntry(entryPath);
        using var stream = newEntry.Open();
        document.Save(stream);
    }

    private static Dictionary<string, string> ReadWorksheetPaths(XDocument workbookDocument, XDocument relationshipsDocument)
    {
        XNamespace workbookNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        XNamespace relNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        XNamespace packageRelNs = "http://schemas.openxmlformats.org/package/2006/relationships";

        var relationshipTargets = relationshipsDocument.Root?
            .Elements(packageRelNs + "Relationship")
            .Where(static element => element.Attribute("Id") is not null && element.Attribute("Target") is not null)
            .ToDictionary(
                static element => element.Attribute("Id")!.Value,
                static element => NormalizeWorksheetTargetPath(element.Attribute("Target")!.Value),
                StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var worksheets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sheet in workbookDocument.Root?.Element(workbookNs + "sheets")?.Elements(workbookNs + "sheet") ?? [])
        {
            var name = sheet.Attribute("name")?.Value;
            var relationshipId = sheet.Attribute(relNs + "id")?.Value;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(relationshipId))
            {
                continue;
            }

            if (relationshipTargets.TryGetValue(relationshipId, out var worksheetPath))
            {
                worksheets[name] = worksheetPath;
            }
        }

        return worksheets;
    }

    private static string NormalizeWorksheetTargetPath(string target)
    {
        var normalized = target.Replace('\\', '/');
        if (normalized.StartsWith("/xl/", StringComparison.OrdinalIgnoreCase))
        {
            return normalized.TrimStart('/');
        }

        if (normalized.StartsWith("xl/", StringComparison.OrdinalIgnoreCase))
        {
            return normalized;
        }

        return $"xl/{normalized.TrimStart('/')}";
    }

    private static Dictionary<int, string> ReadXmlHeaders(XDocument worksheetDocument, int rowNumber, XDocument? sharedStringsDocument = null)
    {
        XNamespace worksheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var headers = new Dictionary<int, string>();
        var row = worksheetDocument.Root?
            .Element(worksheetNs + "sheetData")?
            .Elements(worksheetNs + "row")
            .FirstOrDefault(element => string.Equals(element.Attribute("r")?.Value, rowNumber.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        if (row is null)
        {
            return headers;
        }

        foreach (var cell in row.Elements(worksheetNs + "c"))
        {
            var cellReference = cell.Attribute("r")?.Value;
            if (string.IsNullOrWhiteSpace(cellReference))
            {
                continue;
            }

            var header = ReadCellText(cell, sharedStringsDocument);
            if (!string.IsNullOrWhiteSpace(header))
            {
                headers[GetColumnNumberFromCellReference(cellReference)] = header.Trim();
            }
        }

        return headers;
    }

    private static Dictionary<int, string> ReadXmlDefaultValues(XDocument worksheetDocument, int rowNumber, XDocument? sharedStringsDocument = null)
    {
        XNamespace worksheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var defaults = new Dictionary<int, string>();
        var row = worksheetDocument.Root?
            .Element(worksheetNs + "sheetData")?
            .Elements(worksheetNs + "row")
            .FirstOrDefault(element => string.Equals(element.Attribute("r")?.Value, rowNumber.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        if (row is null)
        {
            return defaults;
        }

        foreach (var cell in row.Elements(worksheetNs + "c"))
        {
            var cellReference = cell.Attribute("r")?.Value;
            var metadata = ReadCellText(cell, sharedStringsDocument).Trim();
            if (string.IsNullOrWhiteSpace(cellReference) || string.IsNullOrWhiteSpace(metadata))
            {
                continue;
            }

            try
            {
                var root = JsonNode.Parse(metadata);
                var rawDefaultValue = root?["defaultValue"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(rawDefaultValue))
                {
                    continue;
                }

                var defaultNode = JsonNode.Parse(rawDefaultValue);
                var value = defaultNode?["value"]?.GetValue<string>()?.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    defaults[GetColumnNumberFromCellReference(cellReference)] = value;
                }
            }
            catch
            {
                // Ignore malformed metadata and continue using mapped values only.
            }
        }

        return defaults;
    }

    private static string ReadCellText(XElement cell, XDocument? sharedStringsDocument = null)
    {
        XNamespace worksheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var cellType = cell.Attribute("t")?.Value;
        if (string.Equals(cellType, "s", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(cell.Element(worksheetNs + "v")?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sharedStringIndex))
        {
            return ReadSharedString(sharedStringsDocument, sharedStringIndex);
        }

        return cell.Element(worksheetNs + "is")?.Value
            ?? cell.Element(worksheetNs + "v")?.Value
            ?? string.Empty;
    }

    private static string ReadSharedString(XDocument? sharedStringsDocument, int sharedStringIndex)
    {
        if (sharedStringsDocument?.Root is null || sharedStringIndex < 0)
        {
            return string.Empty;
        }

        XNamespace worksheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var item = sharedStringsDocument.Root.Elements(worksheetNs + "si").ElementAtOrDefault(sharedStringIndex);
        if (item is null)
        {
            return string.Empty;
        }

        var textRuns = item.Descendants(worksheetNs + "t").Select(static element => element.Value);
        var combined = string.Concat(textRuns);
        if (!string.IsNullOrWhiteSpace(combined))
        {
            return combined;
        }

        return item.Value;
    }

    private static void SetXmlCellValue(
        XDocument worksheetDocument,
        int rowNumber,
        int columnNumber,
        string header,
        string value)
    {
        XNamespace worksheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var sheetData = worksheetDocument.Root?.Element(worksheetNs + "sheetData")
            ?? throw new InvalidOperationException("Worksheet is missing sheetData.");

        var row = sheetData.Elements(worksheetNs + "row")
            .FirstOrDefault(element => string.Equals(element.Attribute("r")?.Value, rowNumber.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        if (row is null)
        {
            row = new XElement(worksheetNs + "row", new XAttribute("r", rowNumber.ToString(CultureInfo.InvariantCulture)));
            var insertAfter = sheetData.Elements(worksheetNs + "row")
                .LastOrDefault(element => int.TryParse(element.Attribute("r")?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var existingRowNumber) && existingRowNumber < rowNumber);

            if (insertAfter is null)
            {
                sheetData.AddFirst(row);
            }
            else
            {
                insertAfter.AddAfterSelf(row);
            }
        }

        var cellReference = $"{GetColumnName(columnNumber)}{rowNumber}";
        var cell = row.Elements(worksheetNs + "c")
            .FirstOrDefault(element => string.Equals(element.Attribute("r")?.Value, cellReference, StringComparison.OrdinalIgnoreCase));
        if (cell is null)
        {
            cell = new XElement(worksheetNs + "c", new XAttribute("r", cellReference));
            var insertAfter = row.Elements(worksheetNs + "c")
                .LastOrDefault(element => CompareCellReferences(element.Attribute("r")?.Value, cellReference) < 0);

            if (insertAfter is null)
            {
                row.AddFirst(cell);
            }
            else
            {
                insertAfter.AddAfterSelf(cell);
            }
        }

        cell.SetAttributeValue("t", null);
        cell.Elements().Remove();

        if (ShouldWriteNumericValue(header) && decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var numericValue))
        {
            cell.Add(new XElement(worksheetNs + "v", numericValue.ToString(CultureInfo.InvariantCulture)));
            return;
        }

        cell.SetAttributeValue("t", "inlineStr");
        cell.Add(new XElement(worksheetNs + "is", new XElement(worksheetNs + "t", value)));
    }

    private static bool ShouldWriteNumericValue(string header)
    {
        return NormalizeHeader(header) switch
        {
            "sku.packageweight" => true,
            "sku.package_weight" => true,
            "sku.price" => true,
            "sku.packagelength" => true,
            "sku.package_length" => true,
            "sku.packagewidth" => true,
            "sku.package_width" => true,
            "sku.packageheight" => true,
            "sku.package_height" => true,
            "sku.multiwarehouseqty.dropshipping" => true,
            "price" => true,
            "quantity" => true,
            "packageweight(kg)" => true,
            "parcel_weight" => true,
            "parcel_length" => true,
            "parcel_width" => true,
            "parcel_height" => true,
            "length" => true,
            "width" => true,
            "height" => true,
            _ => false
        };
    }

    private static void ClearXmlCellsInRange(XDocument worksheetDocument, int startRow, int maxColumn)
    {
        XNamespace worksheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var sheetData = worksheetDocument.Root?.Element(worksheetNs + "sheetData")
            ?? throw new InvalidOperationException("Worksheet is missing sheetData.");

        foreach (var row in sheetData.Elements(worksheetNs + "row"))
        {
            if (!int.TryParse(row.Attribute("r")?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rowNumber) ||
                rowNumber < startRow)
            {
                continue;
            }

            row.Elements(worksheetNs + "c")
                .Where(cell => GetColumnNumberFromCellReference(cell.Attribute("r")?.Value ?? string.Empty) <= maxColumn)
                .Remove();
        }
    }

    private static void SetWorksheetDimension(XDocument worksheetDocument, int startRow, int maxColumn, int endRow)
    {
        XNamespace worksheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var dimension = worksheetDocument.Root?.Element(worksheetNs + "dimension");
        if (dimension is null)
        {
            return;
        }

        var endCell = $"{GetColumnName(maxColumn)}{Math.Max(startRow, endRow)}";
        dimension.SetAttributeValue("ref", $"A3:{endCell}");
    }

    private static int CompareCellReferences(string? left, string right)
    {
        if (string.IsNullOrWhiteSpace(left))
        {
            return -1;
        }

        return GetColumnNumberFromCellReference(left).CompareTo(GetColumnNumberFromCellReference(right));
    }

    private static int GetColumnNumberFromCellReference(string cellReference)
    {
        var letters = new string(cellReference
            .TakeWhile(static character => char.IsLetter(character))
            .ToArray());

        var columnNumber = 0;
        foreach (var character in letters.ToUpperInvariant())
        {
            columnNumber = (columnNumber * 26) + (character - 'A' + 1);
        }

        return columnNumber;
    }

    private static string GetColumnName(int columnNumber)
    {
        var name = string.Empty;
        while (columnNumber > 0)
        {
            columnNumber--;
            name = (char)('A' + (columnNumber % 26)) + name;
            columnNumber /= 26;
        }

        return name;
    }

    private static IXLWorksheet FindShopeeWorksheet(XLWorkbook workbook)
    {
        var exactMatch = workbook.Worksheets.FirstOrDefault(ws =>
            string.Equals(ws.Name, "แบบฟอร์มการลงสินค้า", StringComparison.OrdinalIgnoreCase));
        if (exactMatch is not null)
        {
            return exactMatch;
        }

        var headerMatch = workbook.Worksheets.FirstOrDefault(static ws =>
        {
            var firstRowValues = ws.Row(1)
                .CellsUsed()
                .Select(static cell => cell.GetString().Trim())
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .ToArray();

            if (firstRowValues.Length == 0)
            {
                return false;
            }

            var normalized = firstRowValues
                .Select(NormalizeHeader)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return normalized.Contains(NormalizeHeader("ps_category|0|0")) &&
                   normalized.Contains(NormalizeHeader("ps_product_name|1|0")) &&
                   normalized.Contains(NormalizeHeader("ps_sku_short|0|0"));
        });

        if (headerMatch is not null)
        {
            return headerMatch;
        }

        return workbook.Worksheets.FirstOrDefault(static ws =>
                   ws.Name.Contains("แบบฟอร์ม", StringComparison.OrdinalIgnoreCase) ||
                   ws.Name.Contains("สินค้า", StringComparison.OrdinalIgnoreCase))
               ?? workbook.Worksheet(1);
    }

    private static IXLWorksheet? FindShopeeSizeChartWorksheet(XLWorkbook workbook)
    {
        var exactMatch = workbook.Worksheets.FirstOrDefault(ws =>
            string.Equals(ws.Name, ShopeeSizeChartSheetName, StringComparison.OrdinalIgnoreCase));
        if (exactMatch is not null)
        {
            return exactMatch;
        }

        return workbook.Worksheets.FirstOrDefault(static ws =>
            ws.Name.Contains("ตารางขนาด", StringComparison.OrdinalIgnoreCase) ||
            ws.Name.Contains("size", StringComparison.OrdinalIgnoreCase));
    }

    private static Dictionary<int, string> ReadShopeeHeaders(IXLWorksheet worksheet)
    {
        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 1;
        var headers = new Dictionary<int, string>();
        for (var column = 1; column <= lastColumn; column++)
        {
            var value = worksheet.Cell(1, column).GetString().Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (ShopeeFieldKeySet.Contains(NormalizeHeader(value)))
            {
                headers[column] = value;
            }
        }

        return headers;
    }

    private static Dictionary<int, string> ReadHeaders(IXLWorksheet worksheet)
    {
        var headers = new Dictionary<int, string>();
        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 1;
        for (var column = 1; column <= lastColumn; column++)
        {
            var header = worksheet.Cell(1, column).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(header))
            {
                headers[column] = header;
            }
        }

        return headers;
    }

    private static Dictionary<int, string> ReadHeadersFromRow(IXLWorksheet worksheet, int rowNumber)
    {
        var headers = new Dictionary<int, string>();
        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 1;
        for (var column = 1; column <= lastColumn; column++)
        {
            var header = worksheet.Cell(rowNumber, column).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(header))
            {
                headers[column] = header;
            }
        }

        return headers;
    }

    private static Dictionary<int, string> ReadLazadaHeaders(XLWorkbook workbook, IXLWorksheet worksheet)
    {
        var hiddenSheetName = $"{worksheet.Name}_hide";
        var hiddenWorksheet = workbook.Worksheets.FirstOrDefault(ws =>
            string.Equals(ws.Name, hiddenSheetName, StringComparison.OrdinalIgnoreCase));

        if (hiddenWorksheet is not null)
        {
            var hiddenHeaders = ReadHeadersFromRow(hiddenWorksheet, 3);
            if (hiddenHeaders.Count > 0)
            {
                return hiddenHeaders;
            }
        }

        return ReadHeaders(worksheet);
    }

    private static Dictionary<int, string> ReadLazadaDefaultValues(XLWorkbook workbook, IXLWorksheet worksheet)
    {
        var hiddenSheetName = $"{worksheet.Name}_hide";
        var hiddenWorksheet = workbook.Worksheets.FirstOrDefault(ws =>
            string.Equals(ws.Name, hiddenSheetName, StringComparison.OrdinalIgnoreCase));

        if (hiddenWorksheet is null)
        {
            return [];
        }

        var defaults = new Dictionary<int, string>();
        var lastColumn = hiddenWorksheet.LastColumnUsed()?.ColumnNumber() ?? 1;
        for (var column = 1; column <= lastColumn; column++)
        {
            var metadata = hiddenWorksheet.Cell(5, column).GetString().Trim();
            if (string.IsNullOrWhiteSpace(metadata))
            {
                continue;
            }

            try
            {
                var root = JsonNode.Parse(metadata);
                var rawDefaultValue = root?["defaultValue"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(rawDefaultValue))
                {
                    continue;
                }

                var defaultNode = JsonNode.Parse(rawDefaultValue);
                var value = defaultNode?["value"]?.GetValue<string>()?.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    defaults[column] = value;
                }
            }
            catch
            {
                // Ignore malformed metadata and continue using mapped values only.
            }
        }

        return defaults;
    }

    private static string ResolveLazadaSheetName(
        XLWorkbook workbook,
        ProductItem product,
        IReadOnlyDictionary<string, CategoryMapping>? categoryMappings)
    {
        if (categoryMappings is not null &&
            categoryMappings.TryGetValue(product.Category, out var mapping) &&
            !string.IsNullOrWhiteSpace(mapping.LazadaSheetName) &&
            workbook.Worksheets.Any(ws => string.Equals(ws.Name, mapping.LazadaSheetName, StringComparison.OrdinalIgnoreCase)))
        {
            return workbook.Worksheet(mapping.LazadaSheetName).Name;
        }

        var matched = workbook.Worksheets.FirstOrDefault(ws =>
            !ws.Name.EndsWith("_hide", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(ws.Name, LazadaIndexSheetName, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(ws.Name, LazadaStatusSheetName, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(ws.Name, LazadaGlobalHideSheetName, StringComparison.OrdinalIgnoreCase) &&
            (product.Category.Contains(ws.Name, StringComparison.OrdinalIgnoreCase) ||
             ws.Name.Contains(product.Category, StringComparison.OrdinalIgnoreCase)));

        if (matched is not null)
        {
            return matched.Name;
        }

        return workbook.Worksheets.First(ws =>
            !ws.Name.EndsWith("_hide", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(ws.Name, LazadaIndexSheetName, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(ws.Name, LazadaStatusSheetName, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(ws.Name, LazadaGlobalHideSheetName, StringComparison.OrdinalIgnoreCase)).Name;
    }

    private static string ResolveLazadaSheetName(
        IReadOnlyCollection<string> worksheetNames,
        ProductItem product,
        IReadOnlyDictionary<string, CategoryMapping>? categoryMappings)
    {
        if (worksheetNames.Count == 0)
        {
            throw new InvalidOperationException("Lazada template does not contain any visible product worksheets.");
        }

        if (categoryMappings is not null &&
            categoryMappings.TryGetValue(product.Category, out var mapping) &&
            !string.IsNullOrWhiteSpace(mapping.LazadaSheetName))
        {
            var exactMatch = worksheetNames.FirstOrDefault(name =>
                string.Equals(name, mapping.LazadaSheetName, StringComparison.OrdinalIgnoreCase));
            if (exactMatch is not null)
            {
                return exactMatch;
            }
        }

        var matched = worksheetNames.FirstOrDefault(name =>
            product.Category.Contains(name, StringComparison.OrdinalIgnoreCase) ||
            name.Contains(product.Category, StringComparison.OrdinalIgnoreCase));

        return matched ?? worksheetNames.First();
    }

    private static string NormalizeHeader(string header)
    {
        return header.Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .ToLowerInvariant();
    }

    private string MapValue(ProductItem product, string header, IReadOnlyDictionary<string, CategoryMapping>? categoryMappings)
    {
        var shopeeCategoryCode = ResolveShopeeCategoryCode(product, categoryMappings);
        var shopeeImages = BuildShopeeExportImageSequence(product);
        var hasValidShopeeDimensions = product.Length > 0 && product.Width > 0 && product.Height > 0;
        var shopeeImage1 = shopeeImages[0];
        var shopeeImage2 = shopeeImages[1];
        var shopeeImage3 = shopeeImages[2];
        var shopeeImage4 = shopeeImages[3];
        var lazadaImage1 = ResolveLazadaImageValue(product, 0, _settings.ProductCatalog.LazadaImageMode);
        var lazadaImage2 = ResolveLazadaImageValue(product, 1, _settings.ProductCatalog.LazadaImageMode);
        var lazadaImage3 = ResolveLazadaImageValue(product, 2, _settings.ProductCatalog.LazadaImageMode);
        var lazadaImage4 = ResolveLazadaImageValue(product, 3, _settings.ProductCatalog.LazadaImageMode);
        var variationImageUrl = NormalizePublicImageUrl(product.VariationImageUrl);
        var shopeeVariationImage = ResolveShopeeVariationImageUrl(variationImageUrl, shopeeImage1);
        var lazadaVariationImage = string.IsNullOrWhiteSpace(variationImageUrl) ? lazadaImage1 : variationImageUrl;
        var primaryVariationEntry = ResolvePrimaryVariationEntry(product);
        var shopeeVariationOption = string.IsNullOrWhiteSpace(primaryVariationEntry.Option) ? product.VariationOption : primaryVariationEntry.Option;
        var shopeeVariationPrice = primaryVariationEntry.Price ?? product.VariationPrice ?? product.Price;
        var shopeeVariationStock = primaryVariationEntry.Stock ?? product.VariationStock ?? product.Stock;
        var shopeeVariationImageValue = ResolveShopeeVariationImageUrl(
            string.IsNullOrWhiteSpace(primaryVariationEntry.ImageUrl) ? variationImageUrl : primaryVariationEntry.ImageUrl,
            shopeeImage1);
        var lazadaVariationValue = ResolveLazadaVariationValue(product);
        var raw = header.Trim().ToLowerInvariant();
        var normalized = NormalizeHeader(header);

        if (raw == "radiodangerousgoods" ||
            normalized.Contains("สินค้าอันตราย", StringComparison.Ordinal) ||
            normalized.Contains("อันตราย", StringComparison.Ordinal))
        {
            return NormalizeLazadaDangerousGoods(product.DangerousGoods);
        }

        return raw switch
        {
            "ps_category|0|0" => shopeeCategoryCode,
            "ps_product_name|1|0" => product.ProductName,
            "ps_product_description|1|0" => product.Description,
            "ps_sku_parent_short|0|0" => product.ProductCode,
            "ps_maximum_purchase_quantity|0|0" => string.Empty,
            "ps_maximum_purchase_quantity_start_date|0|0" => string.Empty,
            "ps_maximum_purchase_quantity_time_period|0|0" => string.Empty,
            "ps_maximum_purchase_quantity_end_date|0|0" => string.Empty,
            "ps_minimum_purchase_quantity|0|0" => string.Empty,
            "et_title_variation_integration_no|0|0" => product.ProductCode,
            "et_title_variation_1|0|0" => product.VariationName,
            "et_title_option_for_variation_1|0|0" => shopeeVariationOption,
            "et_title_image_per_variation|0|3" => shopeeVariationImageValue,
            "ps_price|1|1" => shopeeVariationPrice.ToString("0.##"),
            "ps_stock|0|1" => shopeeVariationStock.ToString(),
            "ps_sku_short|0|0" => product.SKU,
            "ps_item_cover_image|0|3" => shopeeImage1,
            "ps_item_image_1|0|3" => shopeeImage2,
            "ps_item_image_2|0|3" => shopeeImage3,
            "ps_item_image_3|0|3" => shopeeImage4,
            "ps_item_image_4|0|3" => string.Empty,
            "ps_item_image_5|0|3" => string.Empty,
            "ps_item_image_6|0|3" => string.Empty,
            "ps_item_image_7|0|3" => string.Empty,
            "ps_item_image_8|0|3" => string.Empty,
            "ps_weight|0|1" => product.Weight.ToString("0.###"),
            "ps_length|0|1" => hasValidShopeeDimensions ? product.Length.ToString("0.###") : string.Empty,
            "ps_width|0|1" => hasValidShopeeDimensions ? product.Width.ToString("0.###") : string.Empty,
            "ps_height|0|1" => hasValidShopeeDimensions ? product.Height.ToString("0.###") : string.Empty,
            "channel_id.7000|0|0" => NormalizeShopeeChannelEnabled(product.DeliveryStandard),
            "ps_product_pre_order_dts|0|1" => string.Empty,
            "et_title_reason|0|0" => string.Empty,
            "title.th_th" => product.ProductName,
            "sku.shop_sku" => product.SKU,
            _ => normalized switch
            {
                "groupno" => product.ProductCode,
                "productnoforbatch" => product.ProductCode,
                "catid" => string.Empty,
                "à¸Šà¸·à¹ˆà¸­à¸ªà¸´à¸™à¸„à¹‰à¸²" => product.ProductName,
                "à¸Šà¸·à¹ˆà¸­à¸ªà¸´à¸™à¸„à¹‰à¸²à¹ƒà¸™en" => product.ProductName,
                "à¸£à¸¹à¸›à¸ à¸²à¸žà¸ªà¸´à¸™à¸„à¹‰à¸²1" => product.Image1,
                "à¸£à¸¹à¸›à¸ à¸²à¸žà¸ªà¸´à¸™à¸„à¹‰à¸²2" => product.Image2,
                "à¸£à¸¹à¸›à¸ à¸²à¸žà¸ªà¸´à¸™à¸„à¹‰à¸²3" => product.Image3,
                "à¸£à¸¹à¸›à¸ à¸²à¸žà¸ªà¸´à¸™à¸„à¹‰à¸²4" => product.Image4,
                "à¸£à¸¹à¸›à¸ à¸²à¸žà¸ªà¸´à¸™à¸„à¹‰à¸²5" => string.Empty,
                "à¸£à¸¹à¸›à¸ à¸²à¸žà¸ªà¸´à¸™à¸„à¹‰à¸²6" => string.Empty,
                "à¸£à¸¹à¸›à¸ à¸²à¸žà¸ªà¸´à¸™à¸„à¹‰à¸²7" => string.Empty,
                "à¸£à¸¹à¸›à¸ à¸²à¸žà¸ªà¸´à¸™à¸„à¹‰à¸²8" => string.Empty,
                "mainimage.0" => lazadaImage1,
                "mainimage.1" => lazadaImage2,
                "mainimage.2" => lazadaImage3,
                "mainimage.3" => lazadaImage4,
                "mainimage.4" => string.Empty,
                "mainimage.5" => string.Empty,
                "mainimage.6" => string.Empty,
                "mainimage.7" => string.Empty,
                "marketimages.1:1" => lazadaImage1,
                "marketimages.1:2" => lazadaImage2,
                "marketimages.1:3" => lazadaImage3,
                "marketimages.1:4" => lazadaImage4,
                "à¸„à¸³à¸­à¸˜à¸´à¸šà¸²à¸¢à¸«à¸¥à¸±à¸" => product.Description,
                "description" => product.Description,
                "à¸ªà¸´à¸™à¸„à¹‰à¸²à¸ à¸²à¸¢à¹ƒà¸™à¸à¸¥à¹ˆà¸­à¸‡" => product.Description,
                "packagecontent" => product.Description,
                "à¸£à¹‰à¸²à¸™sku" => product.SKU,
                "à¸ à¸²à¸ž1" => product.Image1,
                "à¸ à¸²à¸ž2" => product.Image2,
                "à¸ à¸²à¸ž3" => product.Image3,
                "à¸ à¸²à¸ž4" => product.Image4,
                "à¸ à¸²à¸ž5" => string.Empty,
                "à¸ à¸²à¸ž6" => string.Empty,
                "à¸ à¸²à¸ž7" => string.Empty,
                "à¸ à¸²à¸ž8" => string.Empty,
                "sku.props" => lazadaVariationValue,
                "sku.color_thumbnail" => lazadaVariationImage,
                "catproperty.p20000" => NormalizeLazadaBrand(product.Brand),
                "ประเภทวัสดุสำหรับเด็กเล็ก" => product.BabyMaterial,
                "catproperty.p40387" => product.CountryOfOrigin,
                "à¸¢à¸µà¹ˆà¸«à¹‰à¸­" => product.Brand,
                "brand" => NormalizeLazadaBrand(product.Brand),
                "à¸à¸²à¸£à¸£à¸±à¸šà¸›à¸£à¸°à¸à¸±à¸™" => product.WarrantyType,
                "warrantytype" => NormalizeLazadaWarrantyType(product.WarrantyType),
                "à¸£à¸°à¸¢à¸°à¹€à¸§à¸¥à¸²à¸à¸²à¸£à¸£à¸±à¸šà¸›à¸£à¸°à¸à¸±à¸™" => product.WarrantyPeriod,
                "warrantyperiod" => product.WarrantyPeriod,
                "à¹‚à¸—à¸™à¸ªà¸µ" => product.ColorFamily,
                "colorfamily" => product.ColorFamily,
                "saleprop.p30097" => lazadaVariationValue,
                "à¸ªà¸´à¸™à¸„à¸²à¸­à¸±à¸™à¸•à¸£à¸²à¸¢" => product.DangerousGoods,
                "à¸ªà¸´à¸™à¸„à¹‰à¸²à¸­à¸±à¸™à¸•à¸£à¸²à¸¢" => product.DangerousGoods,
                "radiodangerousgoods" => NormalizeLazadaDangerousGoods(product.DangerousGoods),
                "à¸ˆà¸±à¸”à¸ªà¹ˆà¸‡à¸˜à¸£à¸£à¸¡à¸”à¸²" => product.DeliveryStandard,
                "deliverystandard" => NormalizeLazadaYesNoThai(product.DeliveryStandard),
                "sku.skupreorder.enable" => "No",
                "sku.skupreorder.shipdays" => string.Empty,
                "sku.packageweight" => product.Weight.ToString("0.###"),
                "sku.price" => product.VariationPrice?.ToString("0.##") ?? product.Price.ToString("0.##"),
                "sku.sellersku" => product.SKU,
                "sku.packagelength" => product.Length.ToString("0.###"),
                "sku.packagewidth" => product.Width.ToString("0.###"),
                "sku.packageheight" => product.Height.ToString("0.###"),
                "sku.multiwarehouseqty.dropshipping" => product.VariationStock?.ToString() ?? string.Empty,
                "price" => product.Price.ToString("0.##"),
                "quantity" => string.Empty,
                "packageweight(kg)" => product.Weight.ToString("0.###"),
                "length" => product.Length.ToString("0.###"),
                "width" => product.Width.ToString("0.###"),
                "height" => product.Height.ToString("0.###"),
                _ => string.Empty
            }
        };
    }

    private string MapTikTokValue(ProductItem product, string header, IReadOnlyDictionary<string, CategoryMapping>? categoryMappings)
    {
        var images = Enumerable.Range(0, 4)
            .Select(index => ResolveTikTokImageValue(product, index))
            .ToArray();

        var variationImage = string.IsNullOrWhiteSpace(product.VariationImageUrl)
            ? images[0]
            : NormalizePublicImageUrl(product.VariationImageUrl);

        return header switch
        {
            "category" => ResolveTikTokCategory(product, categoryMappings),
            "brand" => string.IsNullOrWhiteSpace(product.Brand) ? "No brand" : product.Brand,
            "product_name" => product.ProductName,
            "product_description" => product.Description,
            "main_image" => images[0],
            "image_2" => images[1],
            "image_3" => images[2],
            "image_4" => images[3],
            "image_5" => string.Empty,
            "image_6" => string.Empty,
            "image_7" => string.Empty,
            "image_8" => string.Empty,
            "image_9" => string.Empty,
            "property_name_1" => product.VariationName,
            "property_value_1" => product.VariationOption,
            "property_1_image" => string.IsNullOrWhiteSpace(product.VariationOption) ? string.Empty : variationImage,
            "property_name_2" => string.Empty,
            "property_value_2" => string.Empty,
            "parcel_weight" => ConvertWeightKilogramsToGrams(product.Weight),
            "parcel_length" => product.Length.ToString("0.###", CultureInfo.InvariantCulture),
            "parcel_width" => product.Width.ToString("0.###", CultureInfo.InvariantCulture),
            "parcel_height" => product.Height.ToString("0.###", CultureInfo.InvariantCulture),
            "delivery" => NormalizeTikTokDeliveryOption(product.DeliveryStandard),
            "price" => (product.VariationPrice ?? product.Price).ToString("0.##", CultureInfo.InvariantCulture),
            "pre_order_time" => string.Empty,
            "quantity" => (product.VariationStock ?? product.Stock).ToString(CultureInfo.InvariantCulture),
            "seller_sku" => product.SKU,
            "size_chart" => string.Empty,
            _ => string.Empty
        };
    }

    private static IXLWorksheet FindTikTokWorksheet(XLWorkbook workbook)
    {
        return workbook.Worksheets.FirstOrDefault(static sheet => sheet.Name.Equals(TikTokSheetName, StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.FirstOrDefault(static sheet => sheet.Cell(TikTokHeaderRow, 1).GetString().Trim().Equals("category", StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.Add(TikTokSheetName);
    }

    private static void ClearTikTokWorksheetData(IXLWorksheet worksheet, int lastColumn, int productCount)
    {
        var lastRow = Math.Max(
            worksheet.LastRowUsed()?.RowNumber() ?? TikTokDataStartRow,
            TikTokDataStartRow + Math.Max(productCount, 1) + 16);

        for (var row = TikTokDataStartRow; row <= lastRow; row++)
        {
            for (var column = 1; column <= lastColumn; column++)
            {
                worksheet.Cell(row, column).Clear(XLClearOptions.Contents);
            }
        }
    }

    private static string ConvertWeightKilogramsToGrams(decimal weightInKilograms)
    {
        var grams = weightInKilograms * 1000m;
        return grams.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string ResolveTikTokCategory(ProductItem product, IReadOnlyDictionary<string, CategoryMapping>? categoryMappings)
    {
        var category = (product.Category ?? string.Empty).Trim();
        var productName = (product.ProductName ?? string.Empty).Trim();
        var combined = $"{category} {productName}";

        if (categoryMappings is not null &&
            !string.IsNullOrWhiteSpace(category) &&
            categoryMappings.TryGetValue(category, out var mapping) &&
            !string.IsNullOrWhiteSpace(mapping.TikTokCategoryName))
        {
            return mapping.TikTokCategoryName.Trim();
        }

        if (string.IsNullOrWhiteSpace(combined))
        {
            return "Drinkware/Water Bottles";
        }

        if (category.Contains('/', StringComparison.Ordinal))
        {
            return category;
        }

        if (ContainsAny(combined, "กระบอกน้ำ", "ขวดน้ำ", "water bottle", "bottle"))
        {
            return "Drinkware/Water Bottles";
        }

        if (ContainsAny(combined, "แก้ว", "mug"))
        {
            return "Drinkware/Mugs";
        }

        if (ContainsAny(combined, "glass", "glasses"))
        {
            return "Drinkware/Glasses";
        }

        if (ContainsAny(combined, "กล่องข้าว", "lunch box"))
        {
            return "Cutlery & Tableware/Lunch Boxes";
        }

        return "Drinkware/Water Bottles";
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeTikTokDeliveryOption(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Default";
        }

        var normalized = value.Trim();
        return normalized.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
               normalized.Equals("Default", StringComparison.OrdinalIgnoreCase)
            ? "Default"
            : normalized;
    }

    private static string ResolveTikTokImageValue(ProductItem product, int index)
    {
        var sharedUrl = NormalizePublicImageUrl(product.GetSharedImageUrl(index));
        if (!string.IsNullOrWhiteSpace(sharedUrl))
        {
            return sharedUrl;
        }

        var localValue = index switch
        {
            0 => product.Image1,
            1 => product.Image2,
            2 => product.Image3,
            3 => product.Image4,
            _ => string.Empty
        };

        return NormalizeLocalImagePath(localValue);
    }

    private static string ResolveShopeeCategoryCode(ProductItem product, IReadOnlyDictionary<string, CategoryMapping>? categoryMappings)
    {
        if (categoryMappings is not null &&
            categoryMappings.TryGetValue(product.Category, out var mapping) &&
            !string.IsNullOrWhiteSpace(mapping.ShopeeCategoryCode))
        {
            return ShopeeCategoryCodeParser.Normalize(mapping.ShopeeCategoryCode);
        }

        return ShopeeCategoryCodeParser.Normalize(product.ShopeeCategoryCode);
    }

    private static string ResolveLazadaImageValue(ProductItem product, int index, LazadaImageMode imageMode)
    {
        var localPaths = new[]
        {
            product.Image1,
            product.Image2,
            product.Image3,
            product.Image4
        }.Select(static value => NormalizeLocalImagePath(value)).ToArray();

        var sharedUrls = new[]
        {
            product.GetSharedImageUrl(0),
            product.GetSharedImageUrl(1),
            product.GetSharedImageUrl(2),
            product.GetSharedImageUrl(3)
        }.Select(static value => NormalizePublicImageUrl(value)).ToArray();

        if (imageMode == LazadaImageMode.LocalFilePath)
        {
            return ResolveUniqueImageValueByPriority(index, localPaths, sharedUrls);
        }

        return ResolveUniqueImageValueByPriority(index, sharedUrls);
    }

    private static string NormalizePublicImageUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();

        if (trimmed.Equals("No Warranty", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("Warranty not available", StringComparison.OrdinalIgnoreCase))
        {
            return "ไม่มีการรับประกัน";
        }

        if (trimmed.Equals("Local Supplier Warranty", StringComparison.OrdinalIgnoreCase))
        {
            return "การรับประกันจากซัพพลายเออร์ในพื้นที่";
        }

        if (trimmed.Equals("International Warranty", StringComparison.OrdinalIgnoreCase))
        {
            return "การรับประกันจากผู้ผลิตระดับสากล";
        }

        if (trimmed.Equals("Seller Warranty", StringComparison.OrdinalIgnoreCase))
        {
            return "การรับประกันโดยผู้ขาย";
        }

        if (trimmed.Equals("Service Warranty", StringComparison.OrdinalIgnoreCase))
        {
            return "การรับประกันโดยผู้ให้บริการ";
        }
        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? uri.AbsoluteUri
            : string.Empty;
    }

    private static string ResolveShopeeVariationImageUrl(string variationImageUrl, string fallbackImageUrl)
    {
        if (string.IsNullOrWhiteSpace(variationImageUrl))
        {
            return fallbackImageUrl;
        }

        return IsAsciiOnly(variationImageUrl) ? variationImageUrl : fallbackImageUrl;
    }

    private static string[] BuildShopeeExportImageSequence(ProductItem product)
    {
        var sourceImages = new[]
        {
            NormalizePublicImageUrl(product.GetSharedImageUrl(0)),
            NormalizePublicImageUrl(product.GetSharedImageUrl(1)),
            NormalizePublicImageUrl(product.GetSharedImageUrl(2)),
            NormalizePublicImageUrl(product.GetSharedImageUrl(3))
        };

        var fallback = sourceImages.FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
        var exportImages = new string[4];
        for (var index = 0; index < exportImages.Length; index++)
        {
            var current = sourceImages[index];
            if (!string.IsNullOrWhiteSpace(current))
            {
                exportImages[index] = current;
                fallback = current;
                continue;
            }

            exportImages[index] = fallback;
        }

        return exportImages;
    }

    private static bool IsAsciiOnly(string value)
    {
        foreach (var character in value)
        {
            if (character > sbyte.MaxValue)
            {
                return false;
            }
        }

        return true;
    }

    private static string NormalizeLocalImagePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return trimmed;
    }

    private static string NormalizeLazadaWarrantyType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return trimmed.Equals("No", StringComparison.OrdinalIgnoreCase)
            ? "ไม่มีการรับประกัน"
            : trimmed;
    }

    private static string NormalizeLazadaBrand(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return trimmed.Equals("Nobrand", StringComparison.OrdinalIgnoreCase)
            ? "No Brand"
            : trimmed;
    }

    private static string NormalizeShopeeChannelEnabled(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "เปิด";
        }

        var trimmed = value.Trim();
        return trimmed.Equals("No", StringComparison.OrdinalIgnoreCase) ||
               trimmed.Equals("False", StringComparison.OrdinalIgnoreCase) ||
               trimmed.Equals("Disabled", StringComparison.OrdinalIgnoreCase) ||
               trimmed.Equals("Off", StringComparison.OrdinalIgnoreCase)
            ? "ปิด"
            : "เปิด";
    }

    private static string NormalizeLazadaDangerousGoods(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        if (trimmed.Equals("No", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        if (trimmed.Equals("Battery", StringComparison.OrdinalIgnoreCase))
        {
            return "แบตเตอรี่";
        }

        if (trimmed.Equals("Yes", StringComparison.OrdinalIgnoreCase))
        {
            return "จัดส่งพร้อมกับสินค้าจริง ไม่จำเป็นต้องมีการส่งรหัส (เช่น ประกันภัย)";
        }

        return trimmed;
    }

    private static string ResolveLazadaVariationValue(ProductItem product)
    {
        if (!string.IsNullOrWhiteSpace(product.VariationOption))
        {
            var parsedEntries = ParseVariationEntries(product.VariationOption);
            var parsedEntry = parsedEntries.FirstOrDefault(static entry => !string.IsNullOrWhiteSpace(entry.Option));
            if (!string.IsNullOrWhiteSpace(parsedEntry?.Option))
            {
                return parsedEntry.Option.Trim();
            }

            return product.VariationOption
                .Split('|', StringSplitOptions.TrimEntries)
                .FirstOrDefault(static part => !string.IsNullOrWhiteSpace(part))?
                .Trim() ?? string.Empty;
        }

        return product.ColorFamily?.Trim() ?? string.Empty;
    }

    private static VariationExportEntry ResolvePrimaryVariationEntry(ProductItem product)
    {
        if (string.IsNullOrWhiteSpace(product.VariationOption))
        {
            return new VariationExportEntry(string.Empty, null, null, string.Empty);
        }

        return ParseVariationEntries(product.VariationOption)
            .FirstOrDefault(static entry => !string.IsNullOrWhiteSpace(entry.Option))
            ?? new VariationExportEntry(product.VariationOption.Trim(), null, null, string.Empty);
    }

    private static string NormalizeLazadaYesNoThai(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        if (trimmed.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("True", StringComparison.OrdinalIgnoreCase))
        {
            return "ใช่";
        }

        if (trimmed.Equals("No", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("False", StringComparison.OrdinalIgnoreCase))
        {
            return "ไม่";
        }

        return trimmed;
    }

    private static string ResolveUniqueImageValueByPriority(int index, params string[][] sources)
    {
        if (index < 0)
        {
            return string.Empty;
        }

        var uniqueValues = new List<string>();
        foreach (var source in sources)
        {
            foreach (var candidate in source)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                var normalized = candidate.Trim();
                if (uniqueValues.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                uniqueValues.Add(normalized);
            }
        }

        if (index < uniqueValues.Count)
        {
            return uniqueValues[index];
        }

        return string.Empty;
    }

    private static string ResolveImageValueByPriority(int index, params string[][] sources)
    {
        foreach (var source in sources)
        {
            if (index >= 0 &&
                index < source.Length &&
                !string.IsNullOrWhiteSpace(source[index]))
            {
                return source[index].Trim();
            }
        }

        return string.Empty;
    }
}
