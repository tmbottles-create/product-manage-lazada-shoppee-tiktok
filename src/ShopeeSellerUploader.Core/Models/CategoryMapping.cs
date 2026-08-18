namespace ShopeeSellerUploader.Core.Models;

public sealed class CategoryMapping
{
    public long Id { get; set; }
    public string ProductCategory { get; set; } = string.Empty;
    public string LazadaSheetName { get; set; } = string.Empty;
    public string ShopeeCategoryCode { get; set; } = string.Empty;
    public string TikTokCategoryName { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
}
