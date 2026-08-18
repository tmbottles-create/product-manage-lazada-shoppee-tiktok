namespace ShopeeSellerUploader.Contracts.Interfaces;

public interface IOneDriveDeviceCodePrompt
{
    Task<bool> ShowAsync(
        string message,
        string verificationUri,
        string userCode,
        CancellationToken cancellationToken = default);
}
