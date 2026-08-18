namespace ShopeeSellerUploader.Core.Models;

public enum ProductProcessStatus
{
    Pending,
    Validated,
    Running,
    Success,
    Failed,
    Skipped,
    RequiresManualAction,
    Cancelled
}
