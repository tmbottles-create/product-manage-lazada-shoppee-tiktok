using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ShopeeSellerUploader.Contracts.Configuration;
using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Infrastructure.Services;

public sealed class LazadaAuthorizationService
{
    public string BuildAuthorizationUrl(LazadaOptions options, string? state = null)
    {
        if (string.IsNullOrWhiteSpace(options.AuthorizeUrl))
        {
            throw new InvalidOperationException("Lazada authorize URL is missing.");
        }

        if (string.IsNullOrWhiteSpace(options.AppKey))
        {
            throw new InvalidOperationException("Lazada AppKey is required before building the authorization URL.");
        }

        if (string.IsNullOrWhiteSpace(options.CallbackUrl))
        {
            throw new InvalidOperationException("Lazada Callback URL is required before building the authorization URL.");
        }

        var queryParts = new List<string>
        {
            $"response_type={Uri.EscapeDataString("code")}",
            $"force_auth={Uri.EscapeDataString("true")}",
            $"redirect_uri={Uri.EscapeDataString(options.CallbackUrl.Trim())}",
            $"client_id={Uri.EscapeDataString(options.AppKey.Trim())}"
        };

        if (!string.IsNullOrWhiteSpace(state))
        {
            queryParts.Add($"state={Uri.EscapeDataString(state.Trim())}");
        }

        return $"{options.AuthorizeUrl.Trim()}?{string.Join("&", queryParts)}";
    }

    public async Task<OneDriveTokenSnapshot> CreateTokenAsync(
        LazadaOptions options,
        string authorizationCode,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration(options);

        if (string.IsNullOrWhiteSpace(authorizationCode))
        {
            throw new InvalidOperationException("Authorization code is required.");
        }

        var parameters = CreateCommonParameters(options);
        parameters["code"] = authorizationCode.Trim();

        var requestUri = BuildSignedUri(options, "/auth/token/create", parameters);
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(Math.Max(15, options.TimeoutSeconds))
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        EnsureApiSuccess(root);

        var accessToken = GetRequiredString(root, "access_token");
        var refreshToken = GetOptionalString(root, "refresh_token");
        var expiresIn = GetOptionalInt32(root, "expires_in") ?? 3600;
        var refreshExpiresIn = GetOptionalInt32(root, "refresh_expires_in");

        return new OneDriveTokenSnapshot
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, expiresIn)),
            RefreshTokenExpiresAt = refreshExpiresIn.HasValue && refreshExpiresIn.Value > 0
                ? DateTimeOffset.UtcNow.AddSeconds(refreshExpiresIn.Value)
                : null
        };
    }

    private static void ValidateConfiguration(LazadaOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.AuthBaseUrl))
        {
            throw new InvalidOperationException("Lazada auth base URL is missing.");
        }

        if (string.IsNullOrWhiteSpace(options.AppKey))
        {
            throw new InvalidOperationException("Lazada AppKey is missing.");
        }

        if (string.IsNullOrWhiteSpace(options.AppSecret))
        {
            throw new InvalidOperationException("Lazada AppSecret is missing.");
        }
    }

    private static Dictionary<string, string> CreateCommonParameters(LazadaOptions options)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["app_key"] = options.AppKey.Trim(),
            ["sign_method"] = "sha256",
            ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)
        };
    }

    private static string BuildSignedUri(LazadaOptions options, string apiPath, IReadOnlyDictionary<string, string> parameters)
    {
        var signingParameters = parameters
            .Where(static pair => !string.IsNullOrWhiteSpace(pair.Value))
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .ToArray();

        var builder = new StringBuilder(apiPath);
        foreach (var pair in signingParameters)
        {
            builder.Append(pair.Key);
            builder.Append(pair.Value);
        }

        var signature = Sign(options.AppSecret.Trim(), builder.ToString());
        var query = string.Join("&", signingParameters
            .Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}")
            .Append($"sign={Uri.EscapeDataString(signature)}"));

        return $"{options.AuthBaseUrl.TrimEnd('/')}{apiPath}?{query}";
    }

    private static string Sign(string appSecret, string source)
    {
        var secretBytes = Encoding.UTF8.GetBytes(appSecret);
        var sourceBytes = Encoding.UTF8.GetBytes(source);
        using var hmac = new HMACSHA256(secretBytes);
        return Convert.ToHexString(hmac.ComputeHash(sourceBytes));
    }

    private static void EnsureApiSuccess(JsonElement root)
    {
        var code = GetOptionalString(root, "code");
        if (string.IsNullOrWhiteSpace(code) || code == "0")
        {
            return;
        }

        var message = GetOptionalString(root, "message");
        throw new InvalidOperationException(
            string.IsNullOrWhiteSpace(message)
                ? $"Lazada authorization failed with code {code}."
                : $"Lazada authorization failed with code {code}: {message}");
    }

    private static string GetRequiredString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var element))
        {
            throw new InvalidOperationException($"Lazada response is missing '{propertyName}'.");
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString()?.Trim() ?? string.Empty,
            JsonValueKind.Number => element.GetRawText(),
            _ => throw new InvalidOperationException($"Lazada response field '{propertyName}' has an unexpected format.")
        };
    }

    private static string GetOptionalString(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var element)
            ? element.ValueKind switch
            {
                JsonValueKind.String => element.GetString()?.Trim() ?? string.Empty,
                JsonValueKind.Number => element.GetRawText(),
                _ => string.Empty
            }
            : string.Empty;
    }

    private static int? GetOptionalInt32(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var element))
        {
            return null;
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var numericValue))
        {
            return numericValue;
        }

        if (element.ValueKind == JsonValueKind.String &&
            int.TryParse(element.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue))
        {
            return parsedValue;
        }

        return null;
    }
}
