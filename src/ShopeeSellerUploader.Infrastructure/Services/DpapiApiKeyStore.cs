using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Infrastructure.Configuration;

namespace ShopeeSellerUploader.Infrastructure.Services;

[SupportedOSPlatform("windows")]
public sealed class DpapiApiKeyStore : IApiKeyStore
{
    private readonly PathProvider _pathProvider;

    public DpapiApiKeyStore(PathProvider pathProvider)
    {
        _pathProvider = pathProvider;
    }

    public bool Exists() => File.Exists(_pathProvider.OpenAiApiKeyFilePath);

    public async Task SaveAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        var plainBytes = Encoding.UTF8.GetBytes(apiKey);
        var encrypted = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(_pathProvider.OpenAiApiKeyFilePath, encrypted, cancellationToken);
    }

    public async Task<string?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!Exists())
        {
            return null;
        }

        var encrypted = await File.ReadAllBytesAsync(_pathProvider.OpenAiApiKeyFilePath, cancellationToken);
        var plain = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(plain);
    }

    public Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        if (Exists())
        {
            File.Delete(_pathProvider.OpenAiApiKeyFilePath);
        }

        return Task.CompletedTask;
    }
}
