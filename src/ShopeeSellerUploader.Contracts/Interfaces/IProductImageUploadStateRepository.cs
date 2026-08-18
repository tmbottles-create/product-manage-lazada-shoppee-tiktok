using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface IProductImageUploadStateRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductImageUploadState>> GetByProductIdAsync(long productId, CancellationToken cancellationToken = default);
    Task SaveAsync(ProductImageUploadState state, CancellationToken cancellationToken = default);
}
