using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface IMarketplaceExportService
{
    Task<string> ExportAsync(
        MarketplaceType marketplace,
        IEnumerable<ProductItem> products,
        string outputFilePath,
        IReadOnlyDictionary<string, CategoryMapping>? categoryMappings = null,
        CancellationToken cancellationToken = default);
}
