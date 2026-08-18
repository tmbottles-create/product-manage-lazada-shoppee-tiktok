using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface ICategoryMappingRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryMapping>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveManyAsync(IEnumerable<CategoryMapping> mappings, CancellationToken cancellationToken = default);
}
