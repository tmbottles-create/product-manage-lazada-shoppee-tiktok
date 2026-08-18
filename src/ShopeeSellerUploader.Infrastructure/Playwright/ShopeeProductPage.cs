using Microsoft.Playwright;
using ShopeeSellerUploader.Contracts.Configuration;
using ShopeeSellerUploader.Core.Models;

namespace ShopeeSellerUploader.Infrastructure.Playwright;

public sealed class ShopeeProductPage
{
    private readonly IPage _page;
    private readonly SelectorOptions _selectors;

    public ShopeeProductPage(IPage page, SelectorOptions selectors)
    {
        _page = page;
        _selectors = selectors;
    }

    public async Task NavigateToAddProductAsync(CancellationToken cancellationToken)
    {
        await _page.Locator(_selectors.ProductForm.AddProductButton).First.ClickAsync(new() { Timeout = 30000 });
        await WaitForIdleAsync(cancellationToken);
    }

    public async Task FillProductAsync(ProductRecord product, Func<string, Task<bool>> manualActionAsync, CancellationToken cancellationToken)
    {
        await FillInputAsync(_selectors.ProductForm.ProductNameInput, product.ProductName);
        await FillDescriptionAsync(product.Description);
        await SelectCategoryAsync(product.Category, manualActionAsync, cancellationToken);
        await FillInputAsync(_selectors.ProductForm.PriceInput, product.Price.ToString("0.##"));
        await FillInputAsync(_selectors.ProductForm.StockInput, product.Stock.ToString());
        await FillInputAsync(_selectors.ProductForm.WeightInput, product.Weight.ToString("0.###"));
        await FillInputAsync(_selectors.ProductForm.LengthInput, product.Length.ToString("0.###"));
        await FillInputAsync(_selectors.ProductForm.WidthInput, product.Width.ToString("0.###"));
        await FillInputAsync(_selectors.ProductForm.HeightInput, product.Height.ToString("0.###"));
        await FillInputAsync(_selectors.ProductForm.SkuInput, product.SKU);
        await UploadImagesAsync(product.GetImagePaths());
        await FillVariationAsync(product);
        await WaitForIdleAsync(cancellationToken);
    }

    public async Task SaveDraftAsync(CancellationToken cancellationToken)
    {
        await _page.Locator(_selectors.ProductForm.SaveDraftButton).First.ClickAsync();
        await WaitForIdleAsync(cancellationToken);
    }

    public async Task PublishAsync(CancellationToken cancellationToken)
    {
        await _page.Locator(_selectors.ProductForm.PublishButton).First.ClickAsync();
        await WaitForIdleAsync(cancellationToken);
    }

    public async Task<bool> IsLoggedInAsync()
    {
        return await _page.Locator(_selectors.Login.UserAvatar).First.IsVisibleAsync() ||
               !await _page.Locator(_selectors.Login.LoginForm).First.IsVisibleAsync();
    }

    public async Task<bool> HasVerificationChallengeAsync()
    {
        var captchaVisible = await TryIsVisibleAsync(_selectors.Common.CaptchaIndicator);
        var otpVisible = await TryIsVisibleAsync(_selectors.Common.OtpIndicator);
        return captchaVisible || otpVisible;
    }

    public async Task<bool> IsPublishSuccessAsync()
    {
        return await TryIsVisibleAsync(_selectors.Common.PublishSuccessIndicator, 4000);
    }

    private async Task FillDescriptionAsync(string description)
    {
        var editor = _page.Locator(_selectors.ProductForm.DescriptionEditor).First;
        await editor.ClickAsync();
        await editor.FillAsync(string.Empty);
        await editor.FillAsync(description);
    }

    private async Task SelectCategoryAsync(string category, Func<string, Task<bool>> manualActionAsync, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            await manualActionAsync("หมวดหมู่สินค้าใน Excel ว่างอยู่ กรุณาเลือก Category ด้วยตนเองใน Browser แล้วกด Continue");
            return;
        }

        var picker = _page.Locator(_selectors.ProductForm.CategoryPicker).First;
        if (!await picker.IsVisibleAsync())
        {
            await manualActionAsync("ไม่พบส่วนเลือกหมวดหมู่ตาม selector ปัจจุบัน กรุณาเลือก Category ด้วยตนเอง แล้วกด Continue");
            return;
        }

        await picker.ClickAsync();
        await _page.Locator(_selectors.ProductForm.CategorySearchInput).First.FillAsync(category);
        var option = _page.Locator($"text={category}").First;

        if (!await TryIsVisibleAsync($"text={category}", 3000))
        {
            await manualActionAsync($"ไม่พบหมวดหมู่ '{category}' แบบอัตโนมัติ กรุณาเลือกเองใน Browser แล้วกด Continue");
            return;
        }

        await option.ClickAsync();
        await _page.Locator(_selectors.ProductForm.CategoryConfirmButton).First.ClickAsync();
        await WaitForIdleAsync(cancellationToken);
    }

    private async Task UploadImagesAsync(IReadOnlyList<string> imagePaths)
    {
        if (imagePaths.Count == 0)
        {
            return;
        }

        await _page.Locator(_selectors.ProductForm.ImageUploadInput).First.SetInputFilesAsync(imagePaths);
    }

    private async Task FillVariationAsync(ProductRecord product)
    {
        if (string.IsNullOrWhiteSpace(product.VariationName) || string.IsNullOrWhiteSpace(product.VariationOption))
        {
            return;
        }

        await _page.Locator(_selectors.ProductForm.AddVariationButton).First.ClickAsync();
        await FillInputAsync(_selectors.ProductForm.VariationNameInput, product.VariationName);
        await FillInputAsync(_selectors.ProductForm.VariationOptionInput, product.VariationOption);

        if (product.VariationPrice.HasValue)
        {
            await FillInputAsync(_selectors.ProductForm.VariationPriceInput, product.VariationPrice.Value.ToString("0.##"));
        }

        if (product.VariationStock.HasValue)
        {
            await FillInputAsync(_selectors.ProductForm.VariationStockInput, product.VariationStock.Value.ToString());
        }
    }

    private async Task FillInputAsync(string selector, string value)
    {
        var locator = _page.Locator(selector).First;
        await locator.ClickAsync();
        await locator.FillAsync(string.Empty);
        await locator.FillAsync(value);
    }

    private async Task WaitForIdleAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _page.Locator(_selectors.Common.LoadingMask).First.WaitForAsync(new()
            {
                State = WaitForSelectorState.Detached,
                Timeout = 5000
            });
        }
        catch
        {
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    private async Task<bool> TryIsVisibleAsync(string selector, float timeout = 500)
    {
        try
        {
            await _page.Locator(selector).First.WaitForAsync(new()
            {
                Timeout = timeout,
                State = WaitForSelectorState.Visible
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
