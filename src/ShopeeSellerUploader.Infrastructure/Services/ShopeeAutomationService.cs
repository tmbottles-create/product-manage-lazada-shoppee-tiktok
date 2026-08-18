using Microsoft.Playwright;
using Serilog;
using ShopeeSellerUploader.Contracts.Configuration;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Infrastructure.Playwright;

namespace ShopeeSellerUploader.Infrastructure.Services;

public sealed class ShopeeAutomationService : IShopeeAutomationService
{
    private const string ShopeeMassUploadUrl = "https://seller.shopee.co.th/portal/product-mass/import/upload";
    private readonly AppSettings _settings;
    private readonly ISessionStore _sessionStore;
    private readonly ILogger _logger;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    public ShopeeAutomationService(AppSettings settings, ISessionStore sessionStore, ILogger logger)
    {
        _settings = settings;
        _sessionStore = sessionStore;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_browser is not null && _browser.IsConnected)
        {
            return;
        }

        await DisposeBrowserOnlyAsync();
        _playwright ??= await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await LaunchBrowserAsync();
    }

    public async Task<bool> OpenLoginBrowserAsync(Func<string, Task<bool>> confirmReadyAsync, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await ResetContextAsync(cancellationToken);

        await _page!.GotoAsync(_settings.Browser.SellerCenterUrl, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        var productPage = new ShopeeProductPage(_page, _settings.Selectors);

        if (await productPage.IsLoggedInAsync())
        {
            await SaveSessionAsync(cancellationToken);
            return true;
        }

        var acknowledged = await confirmReadyAsync("A browser window has been opened. Please sign in to Shopee Seller Center there, then click Continue here to save the session.");
        if (!acknowledged)
        {
            return false;
        }

        await EnsureContextAsync(cancellationToken);
        if (_page is null)
        {
            return false;
        }

        productPage = new ShopeeProductPage(_page, _settings.Selectors);
        if (await productPage.HasVerificationChallengeAsync())
        {
            _logger.Warning("Verification challenge detected during login.");
            return false;
        }

        if (!await productPage.IsLoggedInAsync())
        {
            _logger.Warning("User confirmed login, but Playwright still does not detect a logged-in state.");
            return false;
        }

        await SaveSessionAsync(cancellationToken);
        return true;
    }

    public async Task<ProductProcessResult> ProcessProductAsync(ProductRecord product, Func<string, Task<bool>> manualActionAsync, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await EnsureContextAsync(cancellationToken);

        var result = ProductProcessResult.FromProduct(product);
        var page = _page ?? throw new InvalidOperationException("Automation page is not available.");
        var pageModel = new ShopeeProductPage(page, _settings.Selectors);

        await page.GotoAsync(_settings.Browser.SellerCenterUrl, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        if (!await pageModel.IsLoggedInAsync())
        {
            result.Status = ProductProcessStatus.Failed;
            result.Message = "Session expired or login has not been completed.";
            return result;
        }

        if (await pageModel.HasVerificationChallengeAsync())
        {
            result.Status = ProductProcessStatus.RequiresManualAction;
            result.Message = "CAPTCHA, OTP, or identity verification was detected. Please handle it manually.";
            return result;
        }

        Exception? lastException = null;
        for (var attempt = 1; attempt <= _settings.Automation.MaxRetryCount; attempt++)
        {
            result.Attempts = attempt;
            try
            {
                await EnsureContextAsync(cancellationToken);
                page = _page ?? throw new InvalidOperationException("Automation page is not available.");
                pageModel = new ShopeeProductPage(page, _settings.Selectors);

                await pageModel.NavigateToAddProductAsync(cancellationToken);
                await pageModel.FillProductAsync(product, manualActionAsync, cancellationToken);

                if (_settings.Automation.SaveAsDraftOnly)
                {
                    await pageModel.SaveDraftAsync(cancellationToken);
                    result.Status = ProductProcessStatus.Success;
                    result.Message = "Saved as draft successfully.";
                    break;
                }

                if (_settings.Automation.RequireManualPublishConfirmation)
                {
                    var proceed = await manualActionAsync("The product form has been filled. Please review it in the browser and publish manually, then click Continue here.");
                    if (!proceed)
                    {
                        result.Status = ProductProcessStatus.Cancelled;
                        result.Message = "Cancelled before publish.";
                        break;
                    }
                }
                else
                {
                    await pageModel.PublishAsync(cancellationToken);
                }

                result.Status = await pageModel.IsPublishSuccessAsync()
                    ? ProductProcessStatus.Success
                    : ProductProcessStatus.RequiresManualAction;
                result.Message = result.Status == ProductProcessStatus.Success
                    ? "Published successfully."
                    : "Publish confirmation was not detected. Please verify the result in the browser.";
                break;
            }
            catch (Exception ex) when (attempt < _settings.Automation.MaxRetryCount)
            {
                lastException = ex;
                _logger.Warning(ex, "Attempt {Attempt} failed for product {ProductCode}", attempt, product.ProductCode);
                await ResetContextAsync(cancellationToken);
                await Task.Delay(_settings.Automation.DelayBetweenProductsMs, cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;
                break;
            }
        }

        if (result.Status is ProductProcessStatus.Pending or ProductProcessStatus.Running or ProductProcessStatus.Validated)
        {
            result.Status = ProductProcessStatus.Failed;
            result.Message = lastException?.Message ?? "Unknown automation error.";
        }

        return result;
    }

    public async Task<bool> UploadMassImportFileAsync(string filePath, Func<string, Task<bool>> confirmReadyAsync, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Export file path is required.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Export file was not found.", filePath);
        }

        await InitializeAsync(cancellationToken);
        await EnsureContextAsync(cancellationToken);

        var page = _page ?? throw new InvalidOperationException("Automation page is not available.");
        await page.GotoAsync(ShopeeMassUploadUrl, new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        if (await RequiresLoginAsync(page))
        {
            var loggedIn = await OpenLoginBrowserAsync(confirmReadyAsync, cancellationToken);
            if (!loggedIn)
            {
                return false;
            }

            await EnsureContextAsync(cancellationToken);
            page = _page ?? throw new InvalidOperationException("Automation page is not available.");
            await page.GotoAsync(ShopeeMassUploadUrl, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        }

        if (await HasVerificationChallengeAsync(page))
        {
            var continueAfterManualAction = await confirmReadyAsync("Shopee asked for verification in the browser. Please complete it there, then click Continue here.");
            if (!continueAfterManualAction)
            {
                return false;
            }

            await page.GotoAsync(ShopeeMassUploadUrl, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        }

        await UploadFileAsync(page, filePath, cancellationToken);
        _logger.Information("Shopee mass upload file selected: {FilePath}", filePath);
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeContextOnlyAsync();
        await DisposeBrowserOnlyAsync();
        _playwright?.Dispose();
    }

    private async Task EnsureContextAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);
        if (_page is not null && !_page.IsClosed && _context is not null)
        {
            return;
        }

        await ResetContextAsync(cancellationToken);
    }

    private async Task ResetContextAsync(CancellationToken cancellationToken)
    {
        await DisposeContextOnlyAsync();
        var options = new BrowserNewContextOptions
        {
            ViewportSize = new() { Width = 1440, Height = 960 }
        };

        string? tempStatePath = null;
        var sessionJson = await _sessionStore.LoadAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(sessionJson))
        {
            tempStatePath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempStatePath, sessionJson, cancellationToken);
            options.StorageStatePath = tempStatePath;
        }

        _context = await _browser!.NewContextAsync(options);
        _page = await _context.NewPageAsync();
        _page.SetDefaultTimeout(_settings.Browser.DefaultTimeoutMs);

        if (!string.IsNullOrWhiteSpace(tempStatePath) && File.Exists(tempStatePath))
        {
            File.Delete(tempStatePath);
        }
    }

    private async Task SaveSessionAsync(CancellationToken cancellationToken)
    {
        var storageState = await _context!.StorageStateAsync();
        await _sessionStore.SaveAsync(storageState, cancellationToken);
        _logger.Information("Login session saved successfully.");
    }

    private async Task<IBrowser> LaunchBrowserAsync()
    {
        if (!string.IsNullOrWhiteSpace(_settings.Browser.ExecutablePath))
        {
            _logger.Information("Launching browser using executable path {ExecutablePath}", _settings.Browser.ExecutablePath);
            return await _playwright!.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = _settings.Browser.Headless,
                ExecutablePath = _settings.Browser.ExecutablePath
            });
        }

        var attempts = new List<string>();
        foreach (var channel in GetChannelsToTry())
        {
            try
            {
                _logger.Information("Launching browser channel {Channel}", channel);
                return await _playwright!.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = _settings.Browser.Headless,
                    Channel = channel
                });
            }
            catch (Exception ex)
            {
                attempts.Add(channel);
                _logger.Warning(ex, "Failed to launch browser channel {Channel}", channel);
            }
        }

        throw new InvalidOperationException($"Unable to launch browser. Tried channels: {string.Join(", ", attempts)}");
    }

    private IEnumerable<string> GetChannelsToTry()
    {
        if (!string.IsNullOrWhiteSpace(_settings.Browser.Channel))
        {
            yield return _settings.Browser.Channel;
        }

        if (!string.IsNullOrWhiteSpace(_settings.Browser.FallbackChannel) &&
            !string.Equals(_settings.Browser.FallbackChannel, _settings.Browser.Channel, StringComparison.OrdinalIgnoreCase))
        {
            yield return _settings.Browser.FallbackChannel;
        }
    }

    private async Task DisposeContextOnlyAsync()
    {
        if (_context is not null)
        {
            try
            {
                await _context.CloseAsync();
            }
            catch
            {
            }
        }

        _context = null;
        _page = null;
    }

    private async Task DisposeBrowserOnlyAsync()
    {
        await DisposeContextOnlyAsync();
        if (_browser is not null)
        {
            try
            {
                await _browser.CloseAsync();
            }
            catch
            {
            }
        }

        _browser = null;
    }

    private async Task<bool> RequiresLoginAsync(IPage page)
    {
        var productPage = new ShopeeProductPage(page, _settings.Selectors);
        return !await productPage.IsLoggedInAsync();
    }

    private async Task<bool> HasVerificationChallengeAsync(IPage page)
    {
        var productPage = new ShopeeProductPage(page, _settings.Selectors);
        return await productPage.HasVerificationChallengeAsync();
    }

    private static async Task UploadFileAsync(IPage page, string filePath, CancellationToken cancellationToken)
    {
        const string fileInputSelector = "input[type='file']";
        const string browseButtonSelector = "button:has-text('เลือกไฟล์'), button:has-text('Browse'), button:has-text('Select File')";

        var fileInput = page.Locator(fileInputSelector).First;
        if (await fileInput.CountAsync() > 0)
        {
            await fileInput.SetInputFilesAsync(filePath);
            return;
        }

        var browseButton = page.Locator(browseButtonSelector).First;
        if (await browseButton.CountAsync() == 0)
        {
            throw new InvalidOperationException("Could not find Shopee mass upload file picker.");
        }

        var fileChooserTask = page.WaitForFileChooserAsync();
        await browseButton.ClickAsync();
        var fileChooser = await fileChooserTask;
        cancellationToken.ThrowIfCancellationRequested();
        await fileChooser.SetFilesAsync(filePath);
    }
}
