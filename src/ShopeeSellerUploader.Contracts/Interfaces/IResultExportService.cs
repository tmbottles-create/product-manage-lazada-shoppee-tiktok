using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface IResultExportService
{
    Task ExportAsync(string filePath, IEnumerable<ProductProcessResult> results, CancellationToken cancellationToken = default);
}
