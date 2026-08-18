using System.Security.Cryptography;
using System.Runtime.Versioning;
using System.Text;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Infrastructure.Configuration;

namespace ShopeeSellerUploader.Infrastructure.Services;

[SupportedOSPlatform("windows")]
public sealed class DpapiSessionStore : ISessionStore
{
    private readonly PathProvider _pathProvider;

    public DpapiSessionStore(PathProvider pathProvider)
    {
        _pathProvider = pathProvider;
    }

    public bool Exists() => File.Exists(_pathProvider.SessionFilePath);

    public async Task SaveAsync(string plainJson, CancellationToken cancellationToken = default)
    {
        var bytes = Encoding.UTF8.GetBytes(plainJson);
        var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(_pathProvider.SessionFilePath, encrypted, cancellationToken);
    }

    public async Task<string?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!Exists())
        {
            return null;
        }

        var encrypted = await File.ReadAllBytesAsync(_pathProvider.SessionFilePath, cancellationToken);
        var plain = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(plain);
    }
}
