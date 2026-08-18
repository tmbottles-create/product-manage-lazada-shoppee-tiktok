namespace ShopeeSellerUploader.Core.Models;

public sealed class CheckpointState
{
    public string SourceFilePath { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    public List<ProductProcessResult> Results { get; set; } = [];
}
