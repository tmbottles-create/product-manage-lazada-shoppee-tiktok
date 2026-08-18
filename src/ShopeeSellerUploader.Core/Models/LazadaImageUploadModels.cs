namespace ShopeeSellerUploader.Core.Models;

public enum LazadaUploadStatus
{
    Waiting = 0,
    Success = 1,
    Failed = 2,
    AlreadyUploaded = 3
}

public sealed class ProductImageUploadState
{
    public long ProductId { get; set; }
    public int ImageSequence { get; set; }
    public string LocalImagePath { get; set; } = string.Empty;
    public string LazadaImageUrl { get; set; } = string.Empty;
    public LazadaUploadStatus Status { get; set; } = LazadaUploadStatus.Waiting;
    public string UploadError { get; set; } = string.Empty;
    public DateTimeOffset? UploadedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
}

public sealed class LazadaImageUploadBatchResult
{
    public List<LazadaImageUploadProductResult> Products { get; } = [];
    public int TotalImages => Products.Sum(static product => product.Images.Count);
    public int SuccessCount => Products.Sum(static product => product.Images.Count(image => image.Status == LazadaUploadStatus.Success));
    public int FailedCount => Products.Sum(static product => product.Images.Count(image => image.Status == LazadaUploadStatus.Failed));
    public int SkippedCount => Products.Sum(static product => product.Images.Count(image => image.Status == LazadaUploadStatus.AlreadyUploaded));
}

public sealed class LazadaImageUploadProductResult
{
    public long ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public List<LazadaImageUploadImageResult> Images { get; } = [];
}

public sealed class LazadaImageUploadImageResult
{
    public int ImageSequence { get; init; }
    public string LocalImagePath { get; init; } = string.Empty;
    public string LazadaImageUrl { get; init; } = string.Empty;
    public LazadaUploadStatus Status { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
}

public sealed class OneDriveTokenSnapshot
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTimeOffset? AccessTokenExpiresAt { get; init; }
    public DateTimeOffset? RefreshTokenExpiresAt { get; init; }
}
