namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface IExcelTemplateService
{
    Task<string> EnsureTemplateAsync(CancellationToken cancellationToken = default);
}
