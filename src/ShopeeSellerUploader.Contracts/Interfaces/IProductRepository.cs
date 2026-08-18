using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface IProductRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductItem> SaveAsync(ProductItem product, CancellationToken cancellationToken = default);
    Task DeleteAsync(long productId, CancellationToken cancellationToken = default);
}
