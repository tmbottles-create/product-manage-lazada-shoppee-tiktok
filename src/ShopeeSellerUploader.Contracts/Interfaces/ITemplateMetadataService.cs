namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface ITemplateMetadataService
{
    Task<IReadOnlyList<string>> GetLazadaSheetNamesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetTikTokCategoryNamesAsync(CancellationToken cancellationToken = default);
}
