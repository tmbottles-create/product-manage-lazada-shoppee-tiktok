using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ShopeeSellerUploader.Contracts.Configuration;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Infrastructure.Services;

public sealed class OpenAiProductSuggestionService : IAiProductSuggestionService
{
    private readonly OpenAiOptions _options;
    private readonly IApiKeyStore _apiKeyStore;

    public OpenAiProductSuggestionService(OpenAiOptions options, IApiKeyStore apiKeyStore)
    {
        _options = options;
        _apiKeyStore = apiKeyStore;
    }

    public async Task<AiProductSuggestion> SuggestAsync(AiProductSuggestionRequest request, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException("OpenAI integration is disabled in appsettings.json.");
        }

        var imagePaths = request.ImagePaths
            .Where(File.Exists)
            .Take(Math.Max(1, _options.MaxImagesPerRequest))
            .ToList();

        if (imagePaths.Count == 0)
        {
            throw new InvalidOperationException("Select at least one existing image file before using AI Fill.");
        }

        var apiKey = Environment.GetEnvironmentVariable(_options.ApiKeyEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = await _apiKeyStore.LoadAsync(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                $"OpenAI API key not found. Set environment variable '{_options.ApiKeyEnvironmentVariable}' or save the key in the app first.");
        }

        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(Math.Max(30, _options.TimeoutSeconds))
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var inputContent = new List<object>
        {
            new
            {
                type = "input_text",
                text = BuildUserPrompt(request)
            }
        };

        foreach (var imagePath in imagePaths)
        {
            inputContent.Add(new
            {
                type = "input_image",
                image_url = BuildDataUrl(imagePath),
                detail = "high"
            });
        }

        var payload = new
        {
            model = _options.Model,
            input = new object[]
            {
                new
                {
                    role = "developer",
                    content = new object[]
                    {
                        new
                        {
                            type = "input_text",
                            text = BuildDeveloperPrompt()
                        }
                    }
                },
                new
                {
                    role = "user",
                    content = inputContent.ToArray()
                }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "product_suggestion",
                    description = "Structured product suggestion inferred from user-provided product images.",
                    strict = true,
                    schema = BuildSchema()
                }
            }
        };

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw BuildFriendlyException(response.StatusCode, responseBody);
        }

        var envelope = JsonSerializer.Deserialize<OpenAiResponseEnvelope>(responseBody, JsonOptions)
            ?? throw new InvalidOperationException("OpenAI returned an empty response.");

        var structuredText = ExtractStructuredText(envelope);
        if (string.IsNullOrWhiteSpace(structuredText))
        {
            throw new InvalidOperationException("OpenAI returned no structured product suggestion.");
        }

        var suggestion = JsonSerializer.Deserialize<AiProductSuggestion>(structuredText, JsonOptions)
            ?? throw new InvalidOperationException("Failed to parse OpenAI structured product suggestion.");

        NormalizeSuggestion(suggestion);
        return suggestion;
    }

    private static JsonSerializerOptions JsonOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static string BuildDeveloperPrompt()
    {
        return
            """
            You are helping create product master data for marketplace import.
            Analyze only what is visible or reasonably implied by the provided product images and user hints.
            Return Thai content when suitable for Thai marketplaces.
            If you are unsure about a field, return an empty string or null instead of inventing facts.
            Do not fabricate certifications, warranty terms, dangerous goods status, or exact measurements.
            Create concise marketplace-ready product names and descriptions.
            ProductCode and SKU should be short, uppercase, ASCII-friendly, and derived from the likely product identity when possible.
            DangerousGoods should be "No", "Yes", or "Battery" only.
            DeliveryStandard should be "Yes" or "No" only.
            """;
    }

    private static string BuildUserPrompt(AiProductSuggestionRequest request)
    {
        return
            $"""
            Please infer a product draft from these images.
            User hint: {request.UserHint}
            Existing category hint: {request.ExistingCategory}
            Existing brand hint: {request.ExistingBrand}

            Fill these fields when possible:
            ProductCode, ProductName, Description, Category, Price, Stock, Weight, Length, Width, Height,
            SKU, VariationName, VariationOption, VariationPrice, VariationStock,
            Brand, BabyMaterial, WarrantyType, WarrantyPeriod, ColorFamily, DangerousGoods, DeliveryStandard, Notes.

            Rules:
            - ProductName should be sellable and clear, not overly long.
            - Description should be plain text with short benefit-focused paragraphs.
            - Category should be a human-readable category label, not an ID.
            - Only return numeric values when reasonably inferable; otherwise return null.
            - Notes should briefly tell the user what still needs manual review.
            """;
    }

    private static object BuildSchema()
    {
        return new
        {
            type = "object",
            additionalProperties = false,
            properties = new Dictionary<string, object>
            {
                ["productCode"] = new { type = "string" },
                ["productName"] = new { type = "string" },
                ["description"] = new { type = "string" },
                ["category"] = new { type = "string" },
                ["price"] = new { type = new[] { "number", "null" } },
                ["stock"] = new { type = new[] { "integer", "null" } },
                ["weight"] = new { type = new[] { "number", "null" } },
                ["length"] = new { type = new[] { "number", "null" } },
                ["width"] = new { type = new[] { "number", "null" } },
                ["height"] = new { type = new[] { "number", "null" } },
                ["sku"] = new { type = "string" },
                ["variationName"] = new { type = "string" },
                ["variationOption"] = new { type = "string" },
                ["variationPrice"] = new { type = new[] { "number", "null" } },
                ["variationStock"] = new { type = new[] { "integer", "null" } },
                ["brand"] = new { type = "string" },
                ["babyMaterial"] = new { type = "string" },
                ["warrantyType"] = new { type = "string" },
                ["warrantyPeriod"] = new { type = "string" },
                ["colorFamily"] = new { type = "string" },
                ["dangerousGoods"] = new { type = "string" },
                ["deliveryStandard"] = new { type = "string" },
                ["notes"] = new { type = "string" }
            },
            required = new[]
            {
                "productCode", "productName", "description", "category", "price", "stock",
                "weight", "length", "width", "height", "sku", "variationName", "variationOption",
                "variationPrice", "variationStock", "brand", "babyMaterial", "warrantyType", "warrantyPeriod",
                "colorFamily", "dangerousGoods", "deliveryStandard", "notes"
            }
        };
    }

    private static string BuildDataUrl(string imagePath)
    {
        var extension = Path.GetExtension(imagePath).ToLowerInvariant();
        var mimeType = extension switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };

        var bytes = File.ReadAllBytes(imagePath);
        return $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}";
    }

    private static void NormalizeSuggestion(AiProductSuggestion suggestion)
    {
        suggestion.DangerousGoods = NormalizeDangerousGoods(suggestion.DangerousGoods);
        suggestion.DeliveryStandard = NormalizeDeliveryStandard(suggestion.DeliveryStandard);
        suggestion.ProductCode = suggestion.ProductCode.Trim();
        suggestion.ProductName = suggestion.ProductName.Trim();
        suggestion.Description = suggestion.Description.Trim();
        suggestion.Category = suggestion.Category.Trim();
        suggestion.SKU = suggestion.SKU.Trim();
        suggestion.Brand = suggestion.Brand.Trim();
        suggestion.BabyMaterial = suggestion.BabyMaterial.Trim();
        suggestion.WarrantyType = suggestion.WarrantyType.Trim();
        suggestion.WarrantyPeriod = suggestion.WarrantyPeriod.Trim();
        suggestion.ColorFamily = suggestion.ColorFamily.Trim();
        suggestion.VariationName = suggestion.VariationName.Trim();
        suggestion.VariationOption = suggestion.VariationOption.Trim();
        suggestion.Notes = suggestion.Notes.Trim();
    }

    private static string NormalizeDangerousGoods(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "yes" => "Yes",
            "battery" => "Battery",
            _ => "No"
        };
    }

    private static string NormalizeDeliveryStandard(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "no" => "No",
            _ => "Yes"
        };
    }

    private static string ExtractStructuredText(OpenAiResponseEnvelope envelope)
    {
        if (!string.IsNullOrWhiteSpace(envelope.OutputText))
        {
            return envelope.OutputText.Trim();
        }

        var contentText = envelope.Output?
            .SelectMany(static output => output.Content ?? [])
            .Select(static content => content.Text)
            .FirstOrDefault(static text => !string.IsNullOrWhiteSpace(text));

        if (!string.IsNullOrWhiteSpace(contentText))
        {
            return contentText.Trim();
        }

        return string.Empty;
    }

    private static Exception BuildFriendlyException(HttpStatusCode statusCode, string responseBody)
    {
        var apiMessage = TryExtractApiErrorMessage(responseBody);

        return statusCode switch
        {
            HttpStatusCode.TooManyRequests => new InvalidOperationException(
                "OpenAI quota exceeded. Please check Usage, Billing, or project credits in OpenAI Platform."),
            HttpStatusCode.Unauthorized => new InvalidOperationException(
                "OpenAI API key is invalid or expired. Please set a valid API key and try again."),
            HttpStatusCode.Forbidden => new InvalidOperationException(
                "This OpenAI project does not have permission to use the configured model or endpoint."),
            HttpStatusCode.BadRequest => new InvalidOperationException(
                $"OpenAI rejected the request. {apiMessage}"),
            _ => new InvalidOperationException(
                $"OpenAI request failed ({(int)statusCode}). {apiMessage}")
        };
    }

    private static string TryExtractApiErrorMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return "No additional error details were returned.";
        }

        try
        {
            var envelope = JsonSerializer.Deserialize<OpenAiErrorEnvelope>(responseBody, JsonOptions);
            if (!string.IsNullOrWhiteSpace(envelope?.Error?.Message))
            {
                return envelope.Error.Message.Trim();
            }
        }
        catch
        {
        }

        return responseBody.Length <= 240
            ? responseBody
            : $"{responseBody[..240]}...";
    }

    private sealed class OpenAiResponseEnvelope
    {
        [JsonPropertyName("output_text")]
        public string OutputText { get; set; } = string.Empty;

        [JsonPropertyName("output")]
        public List<OpenAiOutputItem>? Output { get; set; }
    }

    private sealed class OpenAiErrorEnvelope
    {
        [JsonPropertyName("error")]
        public OpenAiErrorDetail? Error { get; set; }
    }

    private sealed class OpenAiErrorDetail
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    private sealed class OpenAiOutputItem
    {
        [JsonPropertyName("content")]
        public List<OpenAiOutputContent>? Content { get; set; }
    }

    private sealed class OpenAiOutputContent
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
