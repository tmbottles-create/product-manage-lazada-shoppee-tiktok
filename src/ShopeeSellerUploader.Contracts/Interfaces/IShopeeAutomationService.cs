using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface IShopeeAutomationService : IAsyncDisposable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<bool> OpenLoginBrowserAsync(Func<string, Task<bool>> confirmReadyAsync, CancellationToken cancellationToken = default);
    Task<ProductProcessResult> ProcessProductAsync(ProductRecord product, Func<string, Task<bool>> manualActionAsync, CancellationToken cancellationToken = default);
    Task<bool> UploadMassImportFileAsync(string filePath, Func<string, Task<bool>> confirmReadyAsync, CancellationToken cancellationToken = default);
}
