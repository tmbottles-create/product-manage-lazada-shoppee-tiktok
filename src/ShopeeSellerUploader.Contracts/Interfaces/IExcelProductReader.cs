using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface IExcelProductReader
{
    Task<IReadOnlyList<ProductRecord>> ReadAsync(string filePath, CancellationToken cancellationToken = default);
}
