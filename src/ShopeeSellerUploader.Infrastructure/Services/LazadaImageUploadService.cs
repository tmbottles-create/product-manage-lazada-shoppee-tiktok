using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Serilog;
using ShopeeSellerUploader.Contracts.Configuration;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Infrastructure.Services;

public sealed class LazadaImageUploadService : ILazadaImageUploadService
{
    private const int LazadaMinimumImageWidth = 330;
    private const int LazadaMinimumImageHeight = 330;

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif",
        ".bmp"
    };

    private readonly ImageKitOptions _options;
    private readonly IProductRepository _productRepository;
    private readonly IProductImageUploadStateRepository _uploadStateRepository;
    private readonly ILogger _logger;

    public LazadaImageUploadService(
        ImageKitOptions options,
        IProductRepository productRepository,
        IProductImageUploadStateRepository uploadStateRepository,
        ILogger logger)
    {
        _options = options;
        _productRepository = productRepository;
        _uploadStateRepository = uploadStateRepository;
        _logger = logger;
    }

    public async Task<LazadaImageUploadBatchResult> UploadAsync(
        IReadOnlyList<ProductItem> products,
        CancellationToken cancellationToken = default)
    {
        if (products.Count == 0)
        {
            return new LazadaImageUploadBatchResult();
        }

        ValidateConfiguration();

        var result = new LazadaImageUploadBatchResult();
        using var httpClient = CreateHttpClient();

        foreach (var product in products)
        {
            var productResult = new LazadaImageUploadProductResult
            {
                ProductId = product.Id,
                ProductCode = product.ProductCode
            };

            foreach (var image in GetProductImages(product))
            {
                var imageResult = await UploadSingleImageAsync(product, image, httpClient, cancellationToken);
                productResult.Images.Add(imageResult);
            }

            result.Products.Add(productResult);
        }

        return result;
    }

    public async Task<LazadaImageUploadImageResult> UploadSingleAsync(
        ProductItem product,
        int imageSequence,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        var image = GetProductImages(product)
            .FirstOrDefault(slot => slot.Sequence == imageSequence);

        if (image is null)
        {
            throw new ArgumentOutOfRangeException(nameof(imageSequence), imageSequence, "Unsupported image sequence.");
        }

        using var httpClient = CreateHttpClient();
        return await UploadSingleImageAsync(product, image, httpClient, cancellationToken);
    }

    public async Task<string> UploadExternalImageAsync(
        string productCode,
        string imagePath,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        var validationError = ValidateLocalImage(imagePath);
        if (!string.IsNullOrWhiteSpace(validationError))
        {
            throw new InvalidOperationException(validationError);
        }

        using var httpClient = CreateHttpClient();
        var slot = new ProductImageSlot(0, imagePath, string.Empty);
        var product = new ProductItem
        {
            ProductCode = productCode
        };

        var uploadedUrl = await UploadToImageKitAsync(product, slot, httpClient, cancellationToken);
        LogUpload(productCode, 0, imagePath, LazadaUploadStatus.Success, uploadedUrl, string.Empty);
        return uploadedUrl;
    }

    private async Task<LazadaImageUploadImageResult> UploadSingleImageAsync(
        ProductItem product,
        ProductImageSlot image,
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateLocalImage(image.LocalPath);
        if (!string.IsNullOrWhiteSpace(validationError))
        {
            var failedState = CreateState(product.Id, image.Sequence, image.LocalPath, string.Empty, LazadaUploadStatus.Failed, validationError, null);
            await _uploadStateRepository.SaveAsync(failedState, cancellationToken);
            LogUpload(product.ProductCode, image.Sequence, image.LocalPath, LazadaUploadStatus.Failed, string.Empty, validationError);
            return new LazadaImageUploadImageResult
            {
                ImageSequence = image.Sequence,
                LocalImagePath = image.LocalPath,
                Status = LazadaUploadStatus.Failed,
                ErrorMessage = validationError
            };
        }

        try
        {
            var uploadedUrl = await UploadToImageKitAsync(product, image, httpClient, cancellationToken);
            ApplyUploadedUrl(product, image.Sequence, uploadedUrl);
            await _productRepository.SaveAsync(product, cancellationToken);

            var successState = CreateState(product.Id, image.Sequence, image.LocalPath, uploadedUrl, LazadaUploadStatus.Success, string.Empty, DateTimeOffset.Now);
            await _uploadStateRepository.SaveAsync(successState, cancellationToken);
            LogUpload(product.ProductCode, image.Sequence, image.LocalPath, LazadaUploadStatus.Success, uploadedUrl, string.Empty);

            return new LazadaImageUploadImageResult
            {
                ImageSequence = image.Sequence,
                LocalImagePath = image.LocalPath,
                LazadaImageUrl = uploadedUrl,
                Status = LazadaUploadStatus.Success
            };
        }
        catch (Exception ex)
        {
            var error = ex.Message;
            var failedState = CreateState(product.Id, image.Sequence, image.LocalPath, string.Empty, LazadaUploadStatus.Failed, error, null);
            await _uploadStateRepository.SaveAsync(failedState, cancellationToken);
            LogUpload(product.ProductCode, image.Sequence, image.LocalPath, LazadaUploadStatus.Failed, string.Empty, error);
            return new LazadaImageUploadImageResult
            {
                ImageSequence = image.Sequence,
                LocalImagePath = image.LocalPath,
                Status = LazadaUploadStatus.Failed,
                ErrorMessage = error
            };
        }
    }

    private async Task<string> UploadToImageKitAsync(
        ProductItem product,
        ProductImageSlot image,
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(image.LocalPath);
        var uploadPayload = await PrepareImageForUploadAsync(fullPath, cancellationToken);
        var fileName = BuildUploadFileName(product.ProductCode, image.Sequence, uploadPayload.Extension);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(uploadPayload.Bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(uploadPayload.ContentType);
        content.Add(fileContent, "file", fileName);
        content.Add(new StringContent(fileName), "fileName");

        var uploadFolderPath = BuildUploadFolderPath(product.ProductCode);
        if (!string.IsNullOrWhiteSpace(uploadFolderPath))
        {
            content.Add(new StringContent(uploadFolderPath), "folder");
        }

        content.Add(new StringContent(_options.UseUniqueFileName ? "true" : "false"), "useUniqueFileName");

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.UploadApiUrl.Trim());
        request.Headers.Authorization = CreateBasicAuthHeader(GetPrivateKey());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = content;

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        if (!root.TryGetProperty("url", out var urlElement))
        {
            throw new InvalidOperationException("ImageKit upload succeeded but no URL was returned.");
        }

        var uploadedUrl = urlElement.GetString()?.Trim() ?? string.Empty;
        if (!IsImageKitHostedUrl(uploadedUrl))
        {
            throw new InvalidOperationException("ImageKit upload returned an unexpected image URL.");
        }

        return uploadedUrl;
    }

    private async Task<UploadPayload> PrepareImageForUploadAsync(string fullPath, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fullPath).ToLowerInvariant();
        await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sourceImage = Image.FromStream(fileStream, useEmbeddedColorManagement: false, validateImageData: false);

        if (sourceImage.Width >= LazadaMinimumImageWidth && sourceImage.Height >= LazadaMinimumImageHeight)
        {
            fileStream.Position = 0;
            var originalBytes = new byte[fileStream.Length];
            _ = await fileStream.ReadAsync(originalBytes, cancellationToken);
            return new UploadPayload(originalBytes, extension, GetContentType(extension));
        }

        var scale = Math.Max(
            1D,
            Math.Max(
                (double)LazadaMinimumImageWidth / sourceImage.Width,
                (double)LazadaMinimumImageHeight / sourceImage.Height));
        var resizedWidth = Math.Max(LazadaMinimumImageWidth, (int)Math.Ceiling(sourceImage.Width * scale));
        var resizedHeight = Math.Max(LazadaMinimumImageHeight, (int)Math.Ceiling(sourceImage.Height * scale));

        using var bitmap = new Bitmap(resizedWidth, resizedHeight);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.White);
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.DrawImage(sourceImage, 0, 0, resizedWidth, resizedHeight);
        }

        await using var outputStream = new MemoryStream();
        bitmap.Save(outputStream, ImageFormat.Png);
        return new UploadPayload(outputStream.ToArray(), ".png", "image/png");
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.UploadApiUrl))
        {
            throw new InvalidOperationException("ImageKit UploadApiUrl is missing.");
        }

        if (string.IsNullOrWhiteSpace(_options.UrlEndpoint))
        {
            throw new InvalidOperationException("ImageKit UrlEndpoint is missing.");
        }

        if (string.IsNullOrWhiteSpace(GetPrivateKey()))
        {
            throw new InvalidOperationException(
                $"ImageKit private key is missing. Set AppSettings:ImageKit:PrivateKey or environment variable '{_options.PrivateKeyEnvironmentVariable}'.");
        }
    }

    private string GetPrivateKey()
    {
        return string.IsNullOrWhiteSpace(_options.PrivateKey)
            ? Environment.GetEnvironmentVariable(_options.PrivateKeyEnvironmentVariable)?.Trim() ?? string.Empty
            : _options.PrivateKey.Trim();
    }

    private bool IsImageKitHostedUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            string.IsNullOrWhiteSpace(_options.UrlEndpoint))
        {
            return false;
        }

        if (!Uri.TryCreate(_options.UrlEndpoint.Trim(), UriKind.Absolute, out var endpointUri) ||
            !Uri.TryCreate(value.Trim(), UriKind.Absolute, out var imageUri))
        {
            return false;
        }

        return string.Equals(endpointUri.Host, imageUri.Host, StringComparison.OrdinalIgnoreCase);
    }

    private HttpClient CreateHttpClient()
    {
        return new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(Math.Max(15, _options.TimeoutSeconds))
        };
    }

    private static AuthenticationHeaderValue CreateBasicAuthHeader(string privateKey)
    {
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{privateKey}:"));
        return new AuthenticationHeaderValue("Basic", token);
    }

    private static IEnumerable<ProductImageSlot> GetProductImages(ProductItem product)
    {
        yield return new ProductImageSlot(1, product.Image1, product.LazadaImage1Url);
        yield return new ProductImageSlot(2, product.Image2, product.LazadaImage2Url);
        yield return new ProductImageSlot(3, product.Image3, product.LazadaImage3Url);
        yield return new ProductImageSlot(4, product.Image4, product.LazadaImage4Url);
    }

    private static void ApplyUploadedUrl(ProductItem product, int imageSequence, string uploadedUrl)
    {
        switch (imageSequence)
        {
            case 1:
                product.SetSharedImageUrl(0, uploadedUrl);
                break;
            case 2:
                product.SetSharedImageUrl(1, uploadedUrl);
                break;
            case 3:
                product.SetSharedImageUrl(2, uploadedUrl);
                break;
            case 4:
                product.SetSharedImageUrl(3, uploadedUrl);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(imageSequence), imageSequence, "Unsupported image sequence.");
        }
    }

    private string? ValidateLocalImage(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return "Local image path is empty.";
        }

        var fullPath = Path.GetFullPath(imagePath);
        if (!File.Exists(fullPath))
        {
            return $"Image file not found: {fullPath}";
        }

        var extension = Path.GetExtension(fullPath);
        if (!SupportedExtensions.Contains(extension))
        {
            return $"Unsupported image format '{extension}'.";
        }

        var fileInfo = new FileInfo(fullPath);
        if (fileInfo.Length > _options.MaxUploadSizeMb * 1024L * 1024L)
        {
            return $"Image file is larger than {_options.MaxUploadSizeMb} MB, which exceeds the configured ImageKit upload limit.";
        }

        return null;
    }

    private static string NormalizeFolderPath(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        return trimmed.StartsWith('/') ? trimmed : $"/{trimmed}";
    }

    private string BuildUploadFolderPath(string? productCode)
    {
        var baseFolder = NormalizeFolderPath(_options.UploadFolderPath);
        var productFolder = string.IsNullOrWhiteSpace(productCode)
            ? "product"
            : SanitizeFolderName(productCode);

        if (string.IsNullOrWhiteSpace(baseFolder) || baseFolder == "/")
        {
            return $"/{productFolder}";
        }

        return $"{baseFolder.TrimEnd('/')}/{productFolder}";
    }

    private static string SanitizeFolderName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);
        foreach (var character in value.Trim())
        {
            if (invalid.Contains(character) || character is '/' or '\\')
            {
                builder.Append('-');
                continue;
            }

            if (char.IsLetterOrDigit(character) || character is '-' or '_')
            {
                builder.Append(character);
                continue;
            }

            if (char.IsWhiteSpace(character))
            {
                if (builder.Length == 0 || builder[^1] == '-')
                {
                    continue;
                }

                builder.Append('-');
                continue;
            }

            if (builder.Length == 0 || builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var sanitized = builder.ToString().Trim('-', '_', ' ');
        return string.IsNullOrWhiteSpace(sanitized) ? "product" : sanitized;
    }

    private static ProductImageUploadState CreateState(
        long productId,
        int imageSequence,
        string localImagePath,
        string lazadaImageUrl,
        LazadaUploadStatus status,
        string error,
        DateTimeOffset? uploadedAt)
    {
        return new ProductImageUploadState
        {
            ProductId = productId,
            ImageSequence = imageSequence,
            LocalImagePath = localImagePath,
            LazadaImageUrl = lazadaImageUrl,
            Status = status,
            UploadError = error,
            UploadedAt = uploadedAt
        };
    }

    private void LogUpload(
        string productCode,
        int imageSequence,
        string localPath,
        LazadaUploadStatus status,
        string imageUrl,
        string error)
    {
        if (status == LazadaUploadStatus.Failed)
        {
            _logger.Warning(
                "[ImageKit Upload] Product={ProductCode} ImageSeq={ImageSequence} LocalPath={LocalPath} Status={Status} Error={Error}",
                productCode,
                imageSequence,
                localPath,
                status,
                error);
            return;
        }

        _logger.Information(
            "[ImageKit Upload] Product={ProductCode} ImageSeq={ImageSequence} LocalPath={LocalPath} Status={Status} Url={Url}",
            productCode,
            imageSequence,
            localPath,
            status,
            imageUrl);
    }

    private static string BuildUploadFileName(string productCode, int imageSequence, string extension)
    {
        var safeCode = string.IsNullOrWhiteSpace(productCode) ? "product" : SanitizeFileName(productCode);
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".jpg" : extension.ToLowerInvariant();
        var stableSuffix = ComputeStableSuffix(productCode);
        return $"{safeCode}-{stableSuffix}-image-{imageSequence}{safeExtension}";
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (invalid.Contains(character))
            {
                builder.Append('-');
                continue;
            }

            if ((character >= 'A' && character <= 'Z') ||
                (character >= 'a' && character <= 'z') ||
                (character >= '0' && character <= '9'))
            {
                builder.Append(character);
                continue;
            }

            if (character is '-' or '_')
            {
                builder.Append(character);
                continue;
            }

            if (char.IsWhiteSpace(character) && (builder.Length == 0 || builder[^1] != '-'))
            {
                builder.Append('-');
                continue;
            }

            if (builder.Length == 0 || builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var sanitized = builder.ToString().Trim('-', '_', ' ');
        return string.IsNullOrWhiteSpace(sanitized) ? "image" : sanitized;
    }

    private static string ComputeStableSuffix(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "00000000";
        }

        var bytes = Encoding.UTF8.GetBytes(value.Trim());
        return Convert.ToHexString(SHA256.HashData(bytes))[..8].ToLowerInvariant();
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };
    }

    private sealed record ProductImageSlot(int Sequence, string LocalPath, string CurrentLazadaUrl);
    private sealed record UploadPayload(byte[] Bytes, string Extension, string ContentType);
}
