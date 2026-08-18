namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface IApiKeyStore
{
    bool Exists();
    Task SaveAsync(string apiKey, CancellationToken cancellationToken = default);
    Task<string?> LoadAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(CancellationToken cancellationToken = default);
}
