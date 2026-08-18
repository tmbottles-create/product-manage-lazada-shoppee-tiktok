namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface IMarketplaceCategoryMasterRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetNamesAsync(string marketplace, CancellationToken cancellationToken = default);
    Task ReplaceAllAsync(string marketplace, IEnumerable<string> names, CancellationToken cancellationToken = default);
}
