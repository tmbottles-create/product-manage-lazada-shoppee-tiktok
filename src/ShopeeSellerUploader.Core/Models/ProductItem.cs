namespace ShopeeSellerUploader.Core.Models;

public sealed class ProductItem
{
    public long Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ShopeeCategoryCode { get; set; } = string.Empty;
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
    public string ShopeeImage1Url { get; set; } = string.Empty;
    public string ShopeeImage2Url { get; set; } = string.Empty;
    public string ShopeeImage3Url { get; set; } = string.Empty;
    public string ShopeeImage4Url { get; set; } = string.Empty;
    public string LazadaImage1Url { get; set; } = string.Empty;
    public string LazadaImage2Url { get; set; } = string.Empty;
    public string LazadaImage3Url { get; set; } = string.Empty;
    public string LazadaImage4Url { get; set; } = string.Empty;
    public string VariationName { get; set; } = string.Empty;
    public string VariationOption { get; set; } = string.Empty;
    public decimal? VariationPrice { get; set; }
    public int? VariationStock { get; set; }
    public string VariationImageUrl { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string BabyMaterial { get; set; } = string.Empty;
    public string CountryOfOrigin { get; set; } = string.Empty;
    public string WarrantyType { get; set; } = string.Empty;
    public string WarrantyPeriod { get; set; } = string.Empty;
    public string ColorFamily { get; set; } = string.Empty;
    public string DangerousGoods { get; set; } = "No";
    public string DeliveryStandard { get; set; } = "Yes";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    public IReadOnlyList<string> GetImagePaths() =>
        new[] { Image1, Image2, Image3, Image4 }
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

    public IReadOnlyList<string> GetShopeeImageUrls() =>
        Enumerable.Range(0, 4)
            .Select(GetSharedImageUrl)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

    public IReadOnlyList<string> GetLazadaImageUrls() =>
        Enumerable.Range(0, 4)
            .Select(GetSharedImageUrl)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

    public string GetSharedImageUrl(int index)
    {
        var lazadaValue = index switch
        {
            0 => LazadaImage1Url,
            1 => LazadaImage2Url,
            2 => LazadaImage3Url,
            3 => LazadaImage4Url,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Image index must be between 0 and 3.")
        };

        if (!string.IsNullOrWhiteSpace(lazadaValue))
        {
            return lazadaValue.Trim();
        }

        var shopeeValue = index switch
        {
            0 => ShopeeImage1Url,
            1 => ShopeeImage2Url,
            2 => ShopeeImage3Url,
            3 => ShopeeImage4Url,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Image index must be between 0 and 3.")
        };

        return shopeeValue.Trim();
    }

    public void SetSharedImageUrl(int index, string value)
    {
        var normalized = value.Trim();

        switch (index)
        {
            case 0:
                ShopeeImage1Url = normalized;
                LazadaImage1Url = normalized;
                break;
            case 1:
                ShopeeImage2Url = normalized;
                LazadaImage2Url = normalized;
                break;
            case 2:
                ShopeeImage3Url = normalized;
                LazadaImage3Url = normalized;
                break;
            case 3:
                ShopeeImage4Url = normalized;
                LazadaImage4Url = normalized;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(index), index, "Image index must be between 0 and 3.");
        }
    }

    public void ClearSharedImageUrl(int index) => SetSharedImageUrl(index, string.Empty);

    public void SynchronizeMarketplaceImageUrls()
    {
        for (var index = 0; index < 4; index++)
        {
            SetSharedImageUrl(index, GetSharedImageUrl(index));
        }
    }
}
