namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface ITemplateMetadataService
{
    Task<IReadOnlyList<string>> GetLazadaSheetNamesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetShopeeCategoryCodesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetTikTokCategoryNamesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetLazadaSheetNamesFromFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetTikTokCategoryNamesFromFileAsync(string filePath, CancellationToken cancellationToken = default);
}
