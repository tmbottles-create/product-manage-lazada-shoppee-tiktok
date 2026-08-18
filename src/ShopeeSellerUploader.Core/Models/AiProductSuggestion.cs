namespace ShopeeSellerUploader.Core.Models;

public sealed class AiProductSuggestion
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string VariationName { get; set; } = string.Empty;
    public string VariationOption { get; set; } = string.Empty;
    public decimal? VariationPrice { get; set; }
    public int? VariationStock { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string BabyMaterial { get; set; } = string.Empty;
    public string WarrantyType { get; set; } = string.Empty;
    public string WarrantyPeriod { get; set; } = string.Empty;
    public string ColorFamily { get; set; } = string.Empty;
    public string DangerousGoods { get; set; } = string.Empty;
    public string DeliveryStandard { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
