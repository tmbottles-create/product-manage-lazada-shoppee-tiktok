using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Infrastructure.Configuration;

namespace ShopeeSellerUploader.Infrastructure.Services;

[SupportedOSPlatform("windows")]
public sealed class DpapiLazadaTokenStore : IOneDriveTokenStore
{
    private readonly PathProvider _pathProvider;

    public DpapiLazadaTokenStore(PathProvider pathProvider)
    {
        _pathProvider = pathProvider;
    }

    public bool Exists() => File.Exists(_pathProvider.LazadaTokenFilePath);

    public async Task SaveAsync(OneDriveTokenSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(snapshot);
        var plainBytes = Encoding.UTF8.GetBytes(json);
        var encrypted = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(_pathProvider.LazadaTokenFilePath, encrypted, cancellationToken);
    }

    public async Task<OneDriveTokenSnapshot?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!Exists())
        {
            return null;
        }

        var encrypted = await File.ReadAllBytesAsync(_pathProvider.LazadaTokenFilePath, cancellationToken);
        var plain = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        return JsonSerializer.Deserialize<OneDriveTokenSnapshot>(plain);
    }

    public Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        if (Exists())
        {
            File.Delete(_pathProvider.LazadaTokenFilePath);
        }

        return Task.CompletedTask;
    }
}
