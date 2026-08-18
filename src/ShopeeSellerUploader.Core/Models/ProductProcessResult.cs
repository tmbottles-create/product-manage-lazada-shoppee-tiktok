namespace ShopeeSellerUploader.Core.Models;

public sealed class ProductProcessResult
{
    public int RowNumber { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public ProductProcessStatus Status { get; set; } = ProductProcessStatus.Pending;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    public int Attempts { get; set; }
    public bool WasResumed { get; set; }

    public static ProductProcessResult FromProduct(ProductRecord product) =>
        new()
        {
            RowNumber = product.RowNumber,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            Status = ProductProcessStatus.Pending
        };
}
