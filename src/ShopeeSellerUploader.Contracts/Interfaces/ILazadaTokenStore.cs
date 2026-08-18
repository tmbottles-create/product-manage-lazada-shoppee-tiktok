using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface IOneDriveTokenStore
{
    bool Exists();
    Task SaveAsync(OneDriveTokenSnapshot snapshot, CancellationToken cancellationToken = default);
    Task<OneDriveTokenSnapshot?> LoadAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(CancellationToken cancellationToken = default);
}
