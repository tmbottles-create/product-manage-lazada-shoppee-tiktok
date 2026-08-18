using Serilog;
using ShopeeSellerUploader.Core.Models;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ShopeeSellerUploader.App.Services;

public sealed class MarketplaceWebPriceService
{
    private static readonly HttpClient HttpClient = CreateHttpClient();
    private static readonly Regex ScriptTagRegex = new("<script[^>]*>(.*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex JsonScriptRegex = new("<script[^>]*type=\"(?:application/json|application/ld\\+json)\"[^>]*>(.*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex TikTokDataRegex = new("<script id=\"__UNIVERSAL_DATA_FOR_REHYDRATION__\"[^>]*>(.*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly string[] TitleKeys = ["title", "productTitle", "product_title", "productName", "product_name", "name"];
    private static readonly string[] LinkKeys = ["productUrl", "productURL", "url", "shareUrl", "share_url", "link", "href"];
    private static readonly string[] PriceKeys = ["priceShow", "price", "salePrice", "sale_price", "displayPrice", "formattedPrice", "formatted_price", "currentPrice", "current_price"];
    private readonly ILogger _logger;

    public MarketplaceWebPriceService(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyList<MarketplaceWebPriceResult>> SearchAsync(ProductItem product, CancellationToken cancellationToken = default)
    {
        var query = BuildMarketplaceSearchQuery(product);
        var results = new List<MarketplaceWebPriceResult>();

        results.AddRange(await SearchShopeeAsync(product, query, cancellationToken));
        results.AddRange(await SearchLazadaAsync(product, query, cancellationToken));
        results.AddRange(await SearchTikTokAsync(product, query, cancellationToken));

        return results
            .OrderBy(static result => result.ProductCode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static result => GetMarketplaceRank(result.Marketplace))
            .ThenBy(static result => result.PriceValue ?? decimal.MaxValue)
            .ThenBy(static result => result.Title, StringComparer.OrdinalIgnoreCase)
            .Take(60)
            .ToArray();
    }

    private async Task<IReadOnlyList<MarketplaceWebPriceResult>> SearchShopeeAsync(ProductItem product, string query, CancellationToken cancellationToken)
    {
        var searchUrl = BuildShopeeSearchUrl(query);
        var results = new List<MarketplaceWebPriceResult>();

        try
        {
            var apiUrl = $"https://shopee.co.th/api/v4/search/search_items?by=relevancy&keyword={Uri.EscapeDataString(query)}&limit=8&newest=0&order=desc&page_type=search&scenario=PAGE_OTHERS&version=2";
            var apiBody = await DownloadStringAsync(apiUrl, searchUrl, cancellationToken);
            results.AddRange(ExtractCandidatesFromJsonPayload("Shopee", product, apiBody, searchUrl));
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Shopee web price API lookup failed for {ProductCode}", product.ProductCode);
        }

        if (results.Count == 0)
        {
            try
            {
                var html = await DownloadStringAsync(searchUrl, searchUrl, cancellationToken);
                results.AddRange(ExtractCandidatesFromHtml("Shopee", product, html, searchUrl));
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Shopee web price HTML lookup failed for {ProductCode}", product.ProductCode);
            }
        }

        return EnsureFallback("Shopee", product, query, searchUrl, results);
    }

    private async Task<IReadOnlyList<MarketplaceWebPriceResult>> SearchLazadaAsync(ProductItem product, string query, CancellationToken cancellationToken)
    {
        var searchUrl = BuildLazadaSearchUrl(query);
        var results = new List<MarketplaceWebPriceResult>();

        try
        {
            var html = await DownloadStringAsync(searchUrl, searchUrl, cancellationToken);
            results.AddRange(ExtractCandidatesFromHtml("Lazada", product, html, searchUrl));
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Lazada web price lookup failed for {ProductCode}", product.ProductCode);
        }

        return EnsureFallback("Lazada", product, query, searchUrl, results);
    }

    private async Task<IReadOnlyList<MarketplaceWebPriceResult>> SearchTikTokAsync(ProductItem product, string query, CancellationToken cancellationToken)
    {
        var searchUrl = BuildTikTokSearchUrl(query);
        var results = new List<MarketplaceWebPriceResult>();

        try
        {
            var html = await DownloadStringAsync(searchUrl, searchUrl, cancellationToken);
            results.AddRange(ExtractCandidatesFromHtml("TikTok", product, html, searchUrl));
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "TikTok web price lookup failed for {ProductCode}", product.ProductCode);
        }

        return EnsureFallback("TikTok", product, query, searchUrl, results);
    }

    private static IReadOnlyList<MarketplaceWebPriceResult> EnsureFallback(
        string marketplace,
        ProductItem product,
        string query,
        string searchUrl,
        List<MarketplaceWebPriceResult> results)
    {
        var distinct = results
            .Where(static result => !string.IsNullOrWhiteSpace(result.Title) || !string.IsNullOrWhiteSpace(result.PriceText))
            .DistinctBy(static result => $"{result.Marketplace}|{result.Link}|{result.Title}")
            .Take(8)
            .ToList();

        if (distinct.Count > 0)
        {
            return distinct;
        }

        return
        [
            new MarketplaceWebPriceResult(
                marketplace,
                product.ProductCode,
                query,
                "Open marketplace search page",
                string.Empty,
                null,
                searchUrl,
                "Search link")
        ];
    }

    private static IEnumerable<MarketplaceWebPriceResult> ExtractCandidatesFromHtml(string marketplace, ProductItem product, string html, string searchUrl)
    {
        foreach (var payload in ExtractJsonPayloads(html))
        {
            foreach (var result in ExtractCandidatesFromJsonPayload(marketplace, product, payload, searchUrl))
            {
                yield return result;
            }
        }
    }

    private static IEnumerable<MarketplaceWebPriceResult> ExtractCandidatesFromJsonPayload(string marketplace, ProductItem product, string payload, string searchUrl)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            yield break;
        }

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(payload);
        }
        catch
        {
            yield break;
        }

        if (root is null)
        {
            yield break;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var result in ExtractCandidatesFromNode(marketplace, product, root, searchUrl))
        {
            var key = $"{result.Marketplace}|{result.Link}|{result.Title}";
            if (seen.Add(key))
            {
                yield return result;
            }
        }
    }

    private static IEnumerable<MarketplaceWebPriceResult> ExtractCandidatesFromNode(
        string marketplace,
        ProductItem product,
        JsonNode root,
        string searchUrl)
    {
        var results = new List<MarketplaceWebPriceResult>();
        Visit(root);
        return results;

        void Visit(JsonNode? node)
        {
            if (node is null || results.Count >= 12)
            {
                return;
            }

            if (node is JsonObject obj)
            {
                var candidate = TryCreateCandidate(marketplace, product, obj, searchUrl);
                if (candidate is not null)
                {
                    results.Add(candidate);
                }

                foreach (var property in obj)
                {
                    Visit(property.Value);
                }

                return;
            }

            if (node is JsonArray array)
            {
                foreach (var child in array)
                {
                    Visit(child);
                }
            }
        }
    }

    private static MarketplaceWebPriceResult? TryCreateCandidate(string marketplace, ProductItem product, JsonObject obj, string searchUrl)
    {
        var title = GetFirstString(obj, TitleKeys);
        var link = NormalizeLink(GetFirstString(obj, LinkKeys), searchUrl);
        var priceText = GetPriceText(obj);
        var priceValue = TryParsePriceValue(priceText);

        if (string.IsNullOrWhiteSpace(title) || title.Length < 8)
        {
            return null;
        }

        if (LooksLikeNonProductTitle(title))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(priceText) && !priceValue.HasValue && string.IsNullOrWhiteSpace(link))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(link))
        {
            link = searchUrl;
        }

        return new MarketplaceWebPriceResult(
            marketplace,
            product.ProductCode,
            BuildMarketplaceSearchQuery(product),
            title.Trim(),
            priceText,
            priceValue,
            link,
            "Matched from web");
    }

    private static IEnumerable<string> ExtractJsonPayloads(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            yield break;
        }

        foreach (Match match in JsonScriptRegex.Matches(html))
        {
            var payload = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
            if (payload.StartsWith('{') || payload.StartsWith('['))
            {
                yield return payload;
            }
        }

        var tiktokMatch = TikTokDataRegex.Match(html);
        if (tiktokMatch.Success)
        {
            yield return WebUtility.HtmlDecode(tiktokMatch.Groups[1].Value).Trim();
        }

        foreach (var marker in new[] { "window.runParams", "window.__INIT_DATA__", "window.__NEXT_DATA__", "window.__INITIAL_STATE__", "__PRELOADED_STATE__" })
        {
            var payload = FindJsonPayloadAfterMarker(html, marker);
            if (!string.IsNullOrWhiteSpace(payload))
            {
                yield return payload;
            }
        }

        foreach (Match match in ScriptTagRegex.Matches(html))
        {
            var script = match.Groups[1].Value;
            if (!script.Contains("price", StringComparison.OrdinalIgnoreCase) ||
                !script.Contains("title", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var payload = FindFirstJsonBlock(script);
            if (!string.IsNullOrWhiteSpace(payload))
            {
                yield return payload;
            }
        }
    }

    private static string? FindJsonPayloadAfterMarker(string text, string marker)
    {
        var markerIndex = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return null;
        }

        var openBraceIndex = text.IndexOf('{', markerIndex);
        var openBracketIndex = text.IndexOf('[', markerIndex);
        var startIndex = ChooseFirstNonNegative(openBraceIndex, openBracketIndex);
        return startIndex < 0 ? null : ReadBalancedJson(text, startIndex);
    }

    private static string? FindFirstJsonBlock(string text)
    {
        var openBraceIndex = text.IndexOf('{');
        var openBracketIndex = text.IndexOf('[');
        var startIndex = ChooseFirstNonNegative(openBraceIndex, openBracketIndex);
        return startIndex < 0 ? null : ReadBalancedJson(text, startIndex);
    }

    private static int ChooseFirstNonNegative(int left, int right)
    {
        if (left < 0)
        {
            return right;
        }

        if (right < 0)
        {
            return left;
        }

        return Math.Min(left, right);
    }

    private static string? ReadBalancedJson(string text, int startIndex)
    {
        var opener = text[startIndex];
        var closer = opener == '{' ? '}' : ']';
        var depth = 0;
        var inString = false;
        var escapeNext = false;

        for (var index = startIndex; index < text.Length; index++)
        {
            var current = text[index];

            if (escapeNext)
            {
                escapeNext = false;
                continue;
            }

            if (current == '\\')
            {
                escapeNext = inString;
                continue;
            }

            if (current == '"')
            {
                inString = !inString;
                continue;
            }

            if (inString)
            {
                continue;
            }

            if (current == opener)
            {
                depth++;
            }
            else if (current == closer)
            {
                depth--;
                if (depth == 0)
                {
                    return text[startIndex..(index + 1)];
                }
            }
        }

        return null;
    }

    private static string BuildMarketplaceSearchQuery(ProductItem product)
    {
        if (!string.IsNullOrWhiteSpace(product.ProductName))
        {
            return product.ProductName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(product.ProductCode))
        {
            return product.ProductCode.Trim();
        }

        return string.IsNullOrWhiteSpace(product.SKU) ? "product" : product.SKU.Trim();
    }

    private static string BuildShopeeSearchUrl(string query) =>
        $"https://shopee.co.th/search?keyword={Uri.EscapeDataString(query)}";

    private static string BuildLazadaSearchUrl(string query) =>
        $"https://www.lazada.co.th/catalog/?q={Uri.EscapeDataString(query)}";

    private static string BuildTikTokSearchUrl(string query) =>
        $"https://www.tiktok.com/search?q={Uri.EscapeDataString(query)}";

    private static string GetFirstString(JsonObject obj, IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            if (!obj.TryGetPropertyValue(key, out var value) || value is null)
            {
                continue;
            }

            var text = value switch
            {
                JsonValue jsonValue => jsonValue.ToString(),
                JsonObject nestedObject => GetPriceText(nestedObject),
                _ => value.ToJsonString()
            };

            if (!string.IsNullOrWhiteSpace(text))
            {
                return WebUtility.HtmlDecode(text).Trim();
            }
        }

        return string.Empty;
    }

    private static string GetPriceText(JsonObject obj)
    {
        foreach (var key in PriceKeys)
        {
            if (!obj.TryGetPropertyValue(key, out var value) || value is null)
            {
                continue;
            }

            if (value is JsonObject nestedObject)
            {
                var amount = GetFirstString(nestedObject, ["formatted_amount", "amount", "value"]);
                var currency = GetFirstString(nestedObject, ["currency", "currency_symbol"]);
                var combined = string.IsNullOrWhiteSpace(currency) ? amount : $"{currency} {amount}";
                if (!string.IsNullOrWhiteSpace(combined))
                {
                    return combined.Trim();
                }
            }

            var text = value.ToString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return WebUtility.HtmlDecode(text).Trim();
            }
        }

        return string.Empty;
    }

    private static decimal? TryParsePriceValue(string priceText)
    {
        if (string.IsNullOrWhiteSpace(priceText))
        {
            return null;
        }

        var digits = new string(priceText.Where(static character => char.IsDigit(character) || character is '.' or ',').ToArray());
        if (string.IsNullOrWhiteSpace(digits))
        {
            return null;
        }

        var normalized = digits;
        if (normalized.Count(static character => character == ',') > 0 && normalized.Count(static character => character == '.') == 0)
        {
            normalized = normalized.Replace(",", ".");
        }
        else
        {
            normalized = normalized.Replace(",", string.Empty);
        }

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static string NormalizeLink(string value, string searchUrl)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        value = WebUtility.HtmlDecode(value).Trim();

        if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        if (value.StartsWith("//", StringComparison.Ordinal))
        {
            return $"https:{value}";
        }

        if (value.StartsWith("/", StringComparison.Ordinal) &&
            Uri.TryCreate(searchUrl, UriKind.Absolute, out var baseUri))
        {
            return new Uri(baseUri, value).ToString();
        }

        return value;
    }

    private static bool LooksLikeNonProductTitle(string title)
    {
        var normalized = title.Trim();
        if (normalized.Length > 180)
        {
            return true;
        }

        return normalized.Equals("search", StringComparison.OrdinalIgnoreCase) ||
               normalized.Equals("product", StringComparison.OrdinalIgnoreCase) ||
               normalized.Equals("products", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("tiktok", StringComparison.OrdinalIgnoreCase) && normalized.Length < 15;
    }

    private static int GetMarketplaceRank(string marketplace) => marketplace switch
    {
        "Shopee" => 0,
        "Lazada" => 1,
        "TikTok" => 2,
        _ => 3
    };

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("th-TH"));
        client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US", 0.9));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 0.9));
        return client;
    }

    private static async Task<string> DownloadStringAsync(string url, string referer, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Referrer = Uri.TryCreate(referer, UriKind.Absolute, out var refererUri) ? refererUri : null;
        request.Headers.TryAddWithoutValidation("x-api-source", "pc");

        using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}

public sealed record MarketplaceWebPriceResult(
    string Marketplace,
    string ProductCode,
    string Query,
    string Title,
    string PriceText,
    decimal? PriceValue,
    string Link,
    string Status);
