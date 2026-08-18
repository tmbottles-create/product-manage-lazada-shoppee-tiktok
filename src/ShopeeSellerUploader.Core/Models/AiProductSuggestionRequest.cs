namespace ShopeeSellerUploader.Core.Models;

public sealed class AiProductSuggestionRequest
{
    public IReadOnlyList<string> ImagePaths { get; init; } = [];
    public string UserHint { get; init; } = string.Empty;
    public string ExistingCategory { get; init; } = string.Empty;
    public string ExistingBrand { get; init; } = string.Empty;
}
