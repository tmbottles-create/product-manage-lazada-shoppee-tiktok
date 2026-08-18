namespace ShopeeSellerUploader.Core.Models;

public sealed class ProductRecord
{
    public int RowNumber { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Image1 { get; set; } = string.Empty;
    public string Image2 { get; set; } = string.Empty;
    public string Image3 { get; set; } = string.Empty;
    public string Image4 { get; set; } = string.Empty;
    public string VariationName { get; set; } = string.Empty;
    public string VariationOption { get; set; } = string.Empty;
    public decimal? VariationPrice { get; set; }
    public int? VariationStock { get; set; }

    public IReadOnlyList<string> GetImagePaths() =>
        new[] { Image1, Image2, Image3, Image4 }
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray();
}
