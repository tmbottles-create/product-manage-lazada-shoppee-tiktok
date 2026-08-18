namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface ISessionStore
{
    bool Exists();
    Task SaveAsync(string plainJson, CancellationToken cancellationToken = default);
    Task<string?> LoadAsync(CancellationToken cancellationToken = default);
}
