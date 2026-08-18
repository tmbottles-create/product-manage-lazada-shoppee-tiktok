using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface ILazadaImageUploadService
{
    Task<LazadaImageUploadBatchResult> UploadAsync(
        IReadOnlyList<ProductItem> products,
        CancellationToken cancellationToken = default);

    Task<LazadaImageUploadImageResult> UploadSingleAsync(
        ProductItem product,
        int imageSequence,
        CancellationToken cancellationToken = default);

    Task<string> UploadExternalImageAsync(
        string productCode,
        string imagePath,
        CancellationToken cancellationToken = default);
}
