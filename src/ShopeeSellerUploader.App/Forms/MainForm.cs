using Serilog;
using ShopeeSellerUploader.App.Services;
using ShopeeSellerUploader.Contracts.Configuration;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Core.Utilities;
using ShopeeSellerUploader.Infrastructure.Configuration;
using System.Diagnostics;
using System.Globalization;

namespace ShopeeSellerUploader.App.Forms;

public partial class ProductWorkspaceForm : Form
{
    private const string SelectColumnName = "SelectColumn";
    private const string ThumbnailColumnName = "Thumbnail";
    private const int ThumbnailWidth = 72;
    private const int ThumbnailHeight = 72;
    private static readonly HttpClient ThumbnailHttpClient = CreateThumbnailHttpClient();
    private readonly AppSettings _settings;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryMappingRepository _categoryMappingRepository;
    private readonly IMarketplaceExportService _exportService;
    private readonly ITemplateMetadataService _templateMetadataService;
    private readonly IAiProductSuggestionService _aiProductSuggestionService;
    private readonly ILazadaImageUploadService _lazadaImageUploadService;
    private readonly IShopeeAutomationService _shopeeAutomationService;
    private readonly MarketplaceWebPriceService _marketplaceWebPriceService;
    private readonly ILogger _logger;
    private readonly PathProvider _pathProvider;
    private readonly BindingSource _bindingSource = new();
    private readonly List<ProductListRow> _rows = [];
    private readonly Dictionary<string, CategoryMapping> _categoryMappings = new(StringComparer.OrdinalIgnoreCase);
    private string? _lastShopeeExportedFilePath;
    private string? _lastTikTokExportedFilePath;
    private bool _isExporting;
    private bool _isUploadingLazadaImages;

    public ProductWorkspaceForm(
        AppSettings settings,
        IProductRepository productRepository,
        ICategoryMappingRepository categoryMappingRepository,
        IMarketplaceExportService exportService,
        ITemplateMetadataService templateMetadataService,
        IAiProductSuggestionService aiProductSuggestionService,
        ILazadaImageUploadService lazadaImageUploadService,
        IShopeeAutomationService shopeeAutomationService,
        ILogger logger,
        PathProvider pathProvider)
    {
        _settings = settings;
        _productRepository = productRepository;
        _categoryMappingRepository = categoryMappingRepository;
        _exportService = exportService;
        _templateMetadataService = templateMetadataService;
        _aiProductSuggestionService = aiProductSuggestionService;
        _lazadaImageUploadService = lazadaImageUploadService;
        _shopeeAutomationService = shopeeAutomationService;
        _marketplaceWebPriceService = new MarketplaceWebPriceService(logger);
        _logger = logger;
        _pathProvider = pathProvider;

        InitializeComponent();
        ApplyButtonStyles();
        dgvProducts.AutoGenerateColumns = false;
        dgvProducts.DataSource = _bindingSource;
        UpdateSelectAllButtonText();
    }

    public void OpenProductList()
    {
        if (IsDisposed)
        {
            return;
        }

        Show();
        Focus();
    }

    public void OpenAddProductDialog()
    {
        btnAdd_Click(this, EventArgs.Empty);
    }

    public void OpenEditSelectedProduct()
    {
        btnEdit_Click(this, EventArgs.Empty);
    }

    public void OpenCopySelectedProduct()
    {
        btnCopy_Click(this, EventArgs.Empty);
    }

    public void OpenCategoryMappingDialog()
    {
        btnCategoryMapping_Click(this, EventArgs.Empty);
    }

    public void RefreshWorkspace()
    {
        btnRefresh_Click(this, EventArgs.Empty);
    }

    public void ExportShopee()
    {
        btnExportShopee_Click(this, EventArgs.Empty);
    }

    public void ExportLazada()
    {
        btnExportLazada_Click(this, EventArgs.Empty);
    }

    public void ExportTikTok()
    {
        btnExportTikTok_Click(this, EventArgs.Empty);
    }

    public void OpenShopeeUpload()
    {
        btnOpenShopeeUpload_Click(this, EventArgs.Empty);
    }

    public void OpenTikTokUpload()
    {
        btnOpenTikTokUpload_Click(this, EventArgs.Empty);
    }

    public void UploadLazadaImages()
    {
        btnUploadLazadaImages_Click(this, EventArgs.Empty);
    }

    public void CheckProductPrices()
    {
        var productsToCheck = _rows.Where(static row => row.Selected).Select(static row => row.Product).ToList();
        if (productsToCheck.Count == 0)
        {
            productsToCheck = _rows.Select(static row => row.Product).ToList();
        }

        if (productsToCheck.Count == 0)
        {
            MessageBox.Show(this, "No products available to check.", "Check Product Prices", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var issues = GetProductPriceIssues(productsToCheck);
        if (issues.Count == 0)
        {
            var successMessage = $"Checked {productsToCheck.Count} product(s). No price issues were found.";
            AppendLog(successMessage);
            MessageBox.Show(this, successMessage, "Check Product Prices", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        foreach (var issue in issues)
        {
            AppendLog($"Price check | {issue}");
        }

        var details = string.Join(Environment.NewLine, issues.Take(20));
        if (issues.Count > 20)
        {
            details += $"{Environment.NewLine}...and {issues.Count - 20} more";
        }

        MessageBox.Show(
            this,
            $"Found {issues.Count} price issue(s) in {productsToCheck.Count} product(s):{Environment.NewLine}{Environment.NewLine}{details}",
            "Check Product Prices",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    public void CheckWebPrices()
    {
        var productsToCheck = _rows.Where(static row => row.Selected).Select(static row => row.Product).ToList();
        if (productsToCheck.Count == 0)
        {
            var selectedRow = GetSelectedRow();
            if (selectedRow is not null)
            {
                productsToCheck.Add(selectedRow.Product);
            }
        }

        if (productsToCheck.Count == 0)
        {
            MessageBox.Show(this, "Select at least one product first.", "Check Web Prices", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new WebPriceCheckForm(productsToCheck, _marketplaceWebPriceService, AppendLog);
        dialog.ShowDialog(this);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await ReloadCategoryMappingsAsync();
        await ReloadProductsAsync();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        DisposeRows();
        base.OnFormClosing(e);
    }

    private async void btnRefresh_Click(object sender, EventArgs e)
    {
        await ReloadCategoryMappingsAsync();
        await ReloadProductsAsync();
    }

    private void btnSelectAll_Click(object sender, EventArgs e)
    {
        var shouldSelectAll = _rows.Any(static row => !row.Selected);
        SetAllRowsSelected(shouldSelectAll);
    }

    private async void btnAdd_Click(object sender, EventArgs e)
    {
        try
        {
            using var dialog = new ProductEditForm(
                GetMappedCategories(),
                _aiProductSuggestionService,
                _productRepository,
                _lazadaImageUploadService,
                _logger);
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            await _productRepository.SaveAsync(dialog.Product);
            AppendLog($"Added product {dialog.Product.ProductCode}");
            await ReloadProductsAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to open Add Product dialog.");
            MessageBox.Show(this, ex.Message, "Add Product Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnEdit_Click(object sender, EventArgs e)
    {
        try
        {
            var selected = GetSelectedRow();
            if (selected is null)
            {
                MessageBox.Show(this, "Select one product to edit.", "Edit Product", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new ProductEditForm(
                selected.Product,
                GetMappedCategories(),
                _aiProductSuggestionService,
                _productRepository,
                _lazadaImageUploadService,
                _logger);
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            await _productRepository.SaveAsync(dialog.Product);
            AppendLog($"Updated product {dialog.Product.ProductCode}");
            await ReloadProductsAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to open Edit Product dialog.");
            MessageBox.Show(this, ex.Message, "Edit Product Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnCopy_Click(object sender, EventArgs e)
    {
        try
        {
            var selected = GetSelectedRow();
            if (selected is null)
            {
                MessageBox.Show(this, "Select one product to copy.", "Copy Product", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var duplicate = CreateDuplicateProduct(selected.Product);
            using var dialog = new ProductEditForm(
                duplicate,
                GetMappedCategories(),
                _aiProductSuggestionService,
                _productRepository,
                _lazadaImageUploadService,
                _logger);
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            await _productRepository.SaveAsync(dialog.Product);
            AppendLog($"Copied product {selected.Product.ProductCode} to {dialog.Product.ProductCode}");
            await ReloadProductsAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to copy product.");
            MessageBox.Show(this, ex.Message, "Copy Product Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnDelete_Click(object sender, EventArgs e)
    {
        var selected = GetSelectedRow();
        if (selected is null)
        {
            MessageBox.Show(this, "Select one product to delete.", "Delete Product", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(this, $"Delete product {selected.Product.ProductCode}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        await _productRepository.DeleteAsync(selected.Product.Id);
        AppendLog($"Deleted product {selected.Product.ProductCode}");
        await ReloadProductsAsync();
    }

    private async void btnCategoryMapping_Click(object sender, EventArgs e)
    {
        var categories = _rows.Select(static row => row.Product.Category)
            .Where(static category => !string.IsNullOrWhiteSpace(category))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static category => category)
            .ToList();

        var lazadaSheetNames = await _templateMetadataService.GetLazadaSheetNamesAsync();
        var tikTokCategoryNames = await _templateMetadataService.GetTikTokCategoryNamesAsync();
        using var dialog = new CategoryMappingForm(categories, lazadaSheetNames, tikTokCategoryNames, _categoryMappings);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var mappings = dialog.GetMappings();
        await _categoryMappingRepository.SaveManyAsync(mappings);
        await ReloadCategoryMappingsAsync();
        AppendLog("Saved category mappings.");
    }

    private async void btnExportShopee_Click(object sender, EventArgs e)
    {
        await ExportAsync(MarketplaceType.Shopee);
    }

    private async void btnExportLazada_Click(object sender, EventArgs e)
    {
        await ExportAsync(MarketplaceType.Lazada);
    }

    private async void btnExportTikTok_Click(object sender, EventArgs e)
    {
        await ExportAsync(MarketplaceType.TikTok);
    }

    private async void btnOpenShopeeUpload_Click(object sender, EventArgs e)
    {
        try
        {
            var exportFile = ResolveShopeeUploadFilePath();
            if (string.IsNullOrWhiteSpace(exportFile))
            {
                MessageBox.Show(
                    this,
                    "No Shopee export file was found yet. Please export Shopee first.",
                    "Open Shopee File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = exportFile,
                UseShellExecute = true
            });

            AppendLog($"Opened Shopee export file: {exportFile}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to open Shopee export file.");
            MessageBox.Show(this, ex.Message, "Open Shopee File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        await Task.CompletedTask;
    }

    private async void btnOpenTikTokUpload_Click(object sender, EventArgs e)
    {
        try
        {
            var exportFile = ResolveTikTokUploadFilePath();
            if (string.IsNullOrWhiteSpace(exportFile))
            {
                MessageBox.Show(
                    this,
                    "No TikTok export file was found yet. Please export TikTok first.",
                    "Open TikTok File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = exportFile,
                UseShellExecute = true
            });

            AppendLog($"Opened TikTok export file: {exportFile}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to open TikTok export file.");
            MessageBox.Show(this, ex.Message, "Open TikTok File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        await Task.CompletedTask;
    }

    private async void btnUploadLazadaImages_Click(object sender, EventArgs e)
    {
        if (_isUploadingLazadaImages)
        {
            return;
        }

        var selectedProducts = _rows.Where(static row => row.Selected).Select(static row => row.Product).ToList();
        if (selectedProducts.Count == 0)
        {
            MessageBox.Show(this, "Select at least one product first.", "Upload Image for Lazada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            SetLazadaUploadBusy(true);
            AppendLog("Uploading images to Lazada...");
            var result = await _lazadaImageUploadService.UploadAsync(selectedProducts);
            await ReloadProductsAsync();

            foreach (var product in result.Products)
            {
                foreach (var image in product.Images)
                {
                    AppendLog($"Lazada image upload | Product={product.ProductCode} | Seq={image.ImageSequence} | Status={image.Status}" +
                              (string.IsNullOrWhiteSpace(image.LazadaImageUrl) ? string.Empty : $" | Url={image.LazadaImageUrl}") +
                              (string.IsNullOrWhiteSpace(image.ErrorMessage) ? string.Empty : $" | Error={image.ErrorMessage}"));
                }
            }

            var summary = BuildLazadaUploadSummary(result);
            MessageBox.Show(this, summary, "Lazada Image Upload Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to upload Lazada images.");
            MessageBox.Show(this, ex.Message, "Lazada Image Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetLazadaUploadBusy(false);
        }
    }

    private void dgvProducts_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        if (!string.Equals(dgvProducts.Columns[e.ColumnIndex].Name, ThumbnailColumnName, StringComparison.Ordinal))
        {
            return;
        }

        if (dgvProducts.Rows[e.RowIndex].DataBoundItem is not ProductListRow row)
        {
            return;
        }

        ShowImagePreview(row.Product);
    }

    private void dgvProducts_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (dgvProducts.IsCurrentCellDirty)
        {
            dgvProducts.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }

    private void dgvProducts_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        if (!string.Equals(dgvProducts.Columns[e.ColumnIndex].DataPropertyName, nameof(ProductListRow.Selected), StringComparison.Ordinal))
        {
            return;
        }

        UpdateSelectAllButtonText();
        UpdateSelectColumnHeaderText();
        UpdateSummaryLabel();
    }

    private void dgvProducts_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.ColumnIndex < 0)
        {
            return;
        }

        if (!string.Equals(dgvProducts.Columns[e.ColumnIndex].Name, SelectColumnName, StringComparison.Ordinal))
        {
            return;
        }

        var shouldSelectAll = _rows.Any(static row => !row.Selected);
        SetAllRowsSelected(shouldSelectAll);
    }

    private async void dgvProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        btnEdit_Click(sender, EventArgs.Empty);
        await Task.CompletedTask;
    }

    private async Task ReloadProductsAsync()
    {
        dgvProducts.DataSource = null;
        _bindingSource.DataSource = null;
        DisposeRows();
        var products = await _productRepository.GetAllAsync();
        _rows.AddRange(products.Select(product => new ProductListRow(product)));
        _bindingSource.DataSource = _rows;
        dgvProducts.DataSource = _bindingSource;
        UpdateSummaryLabel();
        UpdateSelectAllButtonText();
        UpdateSelectColumnHeaderText();
    }

    private void dgvProducts_DataError(object? sender, DataGridViewDataErrorEventArgs e)
    {
        e.ThrowException = false;
        e.Cancel = false;
        _logger.Warning(
            e.Exception,
            "Product grid data error at row {RowIndex}, column {ColumnIndex}. Context={Context}",
            e.RowIndex,
            e.ColumnIndex,
            e.Context);
    }

    private async Task ReloadCategoryMappingsAsync()
    {
        _categoryMappings.Clear();
        var mappings = await _categoryMappingRepository.GetAllAsync();
        foreach (var mapping in mappings)
        {
            _categoryMappings[mapping.ProductCategory] = mapping;
        }
    }

    private async Task ExportAsync(MarketplaceType marketplace)
    {
        if (_isExporting)
        {
            return;
        }

        var selectedProducts = _rows.Where(static row => row.Selected).Select(static row => row.Product).ToList();
        if (selectedProducts.Count == 0)
        {
            MessageBox.Show(this, "Select at least one product first.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (marketplace == MarketplaceType.Shopee)
        {
            var shopeeValidationErrors = GetShopeeExportErrors(selectedProducts);
            if (shopeeValidationErrors.Count > 0)
            {
                var details = string.Join(Environment.NewLine, shopeeValidationErrors);
                var prompt = $"Shopee export ยังทำต่อไม่ได้ เพราะข้อมูลยังไม่ครบ:{Environment.NewLine}{Environment.NewLine}{details}";

                if (selectedProducts.Count == 1)
                {
                    prompt += $"{Environment.NewLine}{Environment.NewLine}ต้องการเปิดหน้าแก้ไขสินค้านี้ตอนนี้หรือไม่?";
                    var result = MessageBox.Show(
                        this,
                        prompt,
                        "Shopee Export Validation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        btnEdit_Click(this, EventArgs.Empty);
                    }
                }
                else
                {
                    prompt += $"{Environment.NewLine}{Environment.NewLine}กรุณาเปิด Category Mapping เพื่อใส่ Shopee Category Code และเปิด Edit เพื่อกรอก Shopee Image URL อย่างน้อย 1 ช่องต่อสินค้า";
                    MessageBox.Show(
                        this,
                        prompt,
                        "Shopee Export Validation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                return;
            }
        }
        else if (marketplace == MarketplaceType.Lazada)
        {
            var lazadaValidationErrors = GetLazadaExportErrors(selectedProducts, _settings.ProductCatalog.LazadaImageMode);
            if (lazadaValidationErrors.Count > 0)
            {
                var details = string.Join(Environment.NewLine, lazadaValidationErrors);
                MessageBox.Show(
                    this,
                    $"Lazada export cannot continue because required data is missing:{Environment.NewLine}{Environment.NewLine}{details}",
                    "Lazada Export Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
        }
        else
        {
            var tikTokValidationErrors = GetTikTokExportErrors(selectedProducts);
            if (tikTokValidationErrors.Count > 0)
            {
                var details = string.Join(Environment.NewLine, tikTokValidationErrors);
                MessageBox.Show(
                    this,
                    $"TikTok export cannot continue because required data is missing:{Environment.NewLine}{Environment.NewLine}{details}",
                    "TikTok Export Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
        }

        var fileName = $"{marketplace}-Import-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
        var outputPath = GetUniqueExportPath(fileName);

        try
        {
            SetExportBusy(true, marketplace);
            AppendLog($"Starting {marketplace} export for {selectedProducts.Count} product(s)...");
            AppendLog($"Export target: {outputPath}");
            await Task.Yield();
            var exportedFile = await _exportService.ExportAsync(
                marketplace,
                selectedProducts,
                outputPath,
                _categoryMappings);
            AppendLog($"Exported {marketplace} file: {exportedFile}");
            if (marketplace == MarketplaceType.Shopee)
            {
                _lastShopeeExportedFilePath = exportedFile;
                AppendLog("Shopee export completed. Click 'Open Shopee Upload' when you want to attach this file manually.");
            }
            else if (marketplace == MarketplaceType.TikTok)
            {
                _lastTikTokExportedFilePath = exportedFile;
                AppendLog("TikTok export completed. Click 'Open TikTok File' when you want to review or attach this file.");
            }

            MessageBox.Show(this, $"Export complete:{Environment.NewLine}{exportedFile}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to export {Marketplace}", marketplace);
            MessageBox.Show(this, ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetExportBusy(false, marketplace);
        }
    }

    private Task<bool> ShowAutomationPromptAsync(string message)
    {
        var result = MessageBox.Show(
            this,
            message,
            "Shopee Automation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information);

        return Task.FromResult(result == DialogResult.Yes);
    }

    private string? ResolveShopeeUploadFilePath()
    {
        if (!string.IsNullOrWhiteSpace(_lastShopeeExportedFilePath) &&
            File.Exists(_lastShopeeExportedFilePath))
        {
            return _lastShopeeExportedFilePath;
        }

        if (!Directory.Exists(_pathProvider.ExportDirectory))
        {
            return null;
        }

        return Directory.GetFiles(_pathProvider.ExportDirectory, "Shopee-Import-*.xlsx")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private string? ResolveTikTokUploadFilePath()
    {
        if (!string.IsNullOrWhiteSpace(_lastTikTokExportedFilePath) &&
            File.Exists(_lastTikTokExportedFilePath))
        {
            return _lastTikTokExportedFilePath;
        }

        if (!Directory.Exists(_pathProvider.ExportDirectory))
        {
            return null;
        }

        return Directory.GetFiles(_pathProvider.ExportDirectory, "TikTok-Import-*.xlsx")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private ProductListRow? GetSelectedRow()
    {
        return dgvProducts.CurrentRow?.DataBoundItem as ProductListRow;
    }

    private void SetAllRowsSelected(bool selected)
    {
        foreach (var row in _rows)
        {
            row.Selected = selected;
        }

        _bindingSource.ResetBindings(false);
        UpdateSelectAllButtonText();
        UpdateSelectColumnHeaderText();
        UpdateSummaryLabel();
    }

    private void UpdateSelectAllButtonText()
    {
        btnSelectAll.Text = _rows.Count > 0 && _rows.All(static row => row.Selected)
            ? "Clear All"
            : "Select All";
    }

    private void UpdateSelectColumnHeaderText()
    {
        if (!dgvProducts.Columns.Contains(SelectColumnName))
        {
            return;
        }

        dgvProducts.Columns[SelectColumnName].HeaderText = _rows.Count > 0 && _rows.All(static row => row.Selected)
            ? "Clear All"
            : "Select All";
    }

    private void ShowImagePreview(ProductItem product)
    {
        var imageSource = GetPreviewSource(product);
        if (string.IsNullOrWhiteSpace(imageSource))
        {
            MessageBox.Show(this, "No image source found for this product.", "Image Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            using var dialog = new ImagePreviewForm(imageSource, product.ProductCode);
            dialog.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to preview image for product {ProductCode}", product.ProductCode);
            MessageBox.Show(this, ex.Message, "Image Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DisposeRows()
    {
        foreach (var row in _rows)
        {
            row.Dispose();
        }

        _rows.Clear();
    }

    private static string GetPreviewSource(ProductItem product)
    {
        foreach (var value in product.GetImagePaths())
        {
            if (IsReadableImageSource(value))
            {
                return value.Trim();
            }
        }

        for (var index = 0; index < 4; index++)
        {
            var value = product.GetSharedImageUrl(index);
            if (IsReadableImageSource(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }

    private static List<string> GetProductPriceIssues(IReadOnlyList<ProductItem> products)
    {
        var issues = new List<string>();

        foreach (var product in products)
        {
            if (product.Price <= 0)
            {
                issues.Add($"{product.ProductCode}: base price must be greater than 0");
            }

            if (product.VariationPrice.HasValue && product.VariationPrice.Value <= 0)
            {
                issues.Add($"{product.ProductCode}: variation price must be greater than 0");
            }

            foreach (var variationIssue in GetVariationPriceIssues(product))
            {
                issues.Add(variationIssue);
            }
        }

        return issues;
    }

    private static IEnumerable<string> GetVariationPriceIssues(ProductItem product)
    {
        if (string.IsNullOrWhiteSpace(product.VariationOption))
        {
            yield break;
        }

        var lines = product.VariationOption
            .Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        foreach (var line in lines)
        {
            if (!line.Contains('|'))
            {
                continue;
            }

            var parts = line.Split('|', StringSplitOptions.TrimEntries);
            var option = parts.ElementAtOrDefault(0)?.Trim() ?? "(unknown)";
            var priceText = parts.ElementAtOrDefault(1)?.Trim();

            if (string.IsNullOrWhiteSpace(priceText))
            {
                yield return $"{product.ProductCode}: variation '{option}' is missing a price";
                continue;
            }

            if (!TryParseDecimal(priceText, out var parsedPrice))
            {
                yield return $"{product.ProductCode}: variation '{option}' has an invalid price '{priceText}'";
                continue;
            }

            if (parsedPrice <= 0)
            {
                yield return $"{product.ProductCode}: variation '{option}' price must be greater than 0";
            }
        }
    }

    private static bool TryParseDecimal(string value, out decimal parsed)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed) ||
               decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out parsed);
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

        return product.SKU.Trim();
    }

    private static string BuildShopeeSearchUrl(string query)
    {
        return $"https://shopee.co.th/search?keyword={Uri.EscapeDataString(query)}";
    }

    private static string BuildLazadaSearchUrl(string query)
    {
        return $"https://www.lazada.co.th/catalog/?q={Uri.EscapeDataString(query)}";
    }

    private static string BuildTikTokSearchUrl(string query)
    {
        return $"https://www.tiktok.com/search?q={Uri.EscapeDataString(query)}";
    }

    private static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    private static bool IsReadableImageSource(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               (File.Exists(value) || IsHttpUrl(value));
    }

    private static Image? LoadThumbnail(string imageSource)
    {
        if (string.IsNullOrWhiteSpace(imageSource))
        {
            return null;
        }

        if (IsHttpUrl(imageSource))
        {
            return LoadThumbnailFromUrl(imageSource);
        }

        if (!File.Exists(imageSource))
        {
            return null;
        }

        using var stream = new FileStream(imageSource, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var original = Image.FromStream(stream);
        return CreateThumbnail(original, ThumbnailWidth, ThumbnailHeight);
    }

    private static Image? LoadThumbnailFromUrl(string imageUrl)
    {
        try
        {
            using var response = ThumbnailHttpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            using var responseStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            using var memoryStream = new MemoryStream();
            responseStream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            using var original = Image.FromStream(memoryStream);
            return CreateThumbnail(original, ThumbnailWidth, ThumbnailHeight);
        }
        catch
        {
            return null;
        }
    }

    private static Image CreateThumbnail(Image original, int maxWidth, int maxHeight)
    {
        var scale = Math.Min((double)maxWidth / original.Width, (double)maxHeight / original.Height);
        scale = Math.Min(scale, 1D);
        var width = Math.Max(1, (int)Math.Round(original.Width * scale));
        var height = Math.Max(1, (int)Math.Round(original.Height * scale));
        var bitmap = new Bitmap(maxWidth, maxHeight);

        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.WhiteSmoke);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        var x = (maxWidth - width) / 2;
        var y = (maxHeight - height) / 2;
        graphics.DrawImage(original, x, y, width, height);
        return bitmap;
    }

    private void AppendLog(string message)
    {
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private void ApplyButtonStyles()
    {
        StyleButton(btnRefresh, Color.White, Color.FromArgb(37, 99, 235), Color.FromArgb(191, 219, 254));
        StyleButton(btnAdd, Color.FromArgb(37, 99, 235), Color.White, Color.FromArgb(37, 99, 235));
        StyleButton(btnCopy, Color.FromArgb(8, 145, 178), Color.White, Color.FromArgb(8, 145, 178));
        StyleButton(btnEdit, Color.FromArgb(15, 118, 110), Color.White, Color.FromArgb(15, 118, 110));
        StyleButton(btnDelete, Color.FromArgb(220, 38, 38), Color.White, Color.FromArgb(220, 38, 38));
        StyleButton(btnExportShopee, Color.FromArgb(249, 115, 22), Color.White, Color.FromArgb(249, 115, 22));
        StyleButton(btnOpenShopeeUpload, Color.FromArgb(14, 116, 144), Color.White, Color.FromArgb(14, 116, 144));
        StyleButton(btnExportLazada, Color.FromArgb(124, 58, 237), Color.White, Color.FromArgb(124, 58, 237));
        StyleButton(btnExportTikTok, Color.FromArgb(17, 24, 39), Color.White, Color.FromArgb(17, 24, 39));
        StyleButton(btnOpenTikTokUpload, Color.FromArgb(31, 41, 55), Color.White, Color.FromArgb(31, 41, 55));
        StyleButton(btnUploadLazadaImages, Color.FromArgb(2, 132, 199), Color.White, Color.FromArgb(2, 132, 199));
    }

    private static void StyleButton(Button button, Color backColor, Color foreColor, Color borderColor)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = borderColor;
        button.FlatAppearance.MouseOverBackColor = Lighten(backColor, 0.08F);
        button.FlatAppearance.MouseDownBackColor = Lighten(backColor, 0.16F);
        button.BackColor = backColor;
        button.ForeColor = foreColor;
        button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        button.Margin = new Padding(6, 4, 0, 4);
        button.Padding = new Padding(8, 2, 8, 2);
        button.Cursor = Cursors.Hand;
        button.Height = 34;
        button.AutoSize = false;
    }

    private void UpdateSummaryLabel()
    {
        var selectedCount = _rows.Count(static row => row.Selected);
        lblSummary.Text = $"Products: {_rows.Count} total | Selected: {selectedCount} | Mappings: {_categoryMappings.Count}";
    }

    private static Color Lighten(Color color, float amount)
    {
        var clampedAmount = Math.Max(0F, Math.Min(1F, amount));
        var r = color.R + (int)((255 - color.R) * clampedAmount);
        var g = color.G + (int)((255 - color.G) * clampedAmount);
        var b = color.B + (int)((255 - color.B) * clampedAmount);
        return Color.FromArgb(r, g, b);
    }

    private IReadOnlyList<string> GetMappedCategories()
    {
        return _categoryMappings.Keys
            .Where(static category => !string.IsNullOrWhiteSpace(category))
            .OrderBy(static category => category)
            .ToList();
    }

    private void SetExportBusy(bool isBusy, MarketplaceType marketplace)
    {
        _isExporting = isBusy;
        UseWaitCursor = isBusy;
        btnExportShopee.Enabled = !isBusy;
        btnOpenShopeeUpload.Enabled = !isBusy;
        btnExportLazada.Enabled = !isBusy;
        btnExportTikTok.Enabled = !isBusy;
        btnOpenTikTokUpload.Enabled = !isBusy;
        btnUploadLazadaImages.Enabled = !isBusy && !_isUploadingLazadaImages;
        btnAdd.Enabled = !isBusy;
        btnCopy.Enabled = !isBusy;
        btnEdit.Enabled = !isBusy;
        btnDelete.Enabled = !isBusy;
        btnRefresh.Enabled = !isBusy;
        dgvProducts.Enabled = !isBusy;

        if (isBusy)
        {
            lblSummary.Text = $"Exporting {marketplace}...";
        }
        else
        {
            UpdateSummaryLabel();
        }
    }

    private void SetLazadaUploadBusy(bool isBusy)
    {
        _isUploadingLazadaImages = isBusy;
        UseWaitCursor = isBusy;
        btnUploadLazadaImages.Enabled = !isBusy && !_isExporting;
        btnExportShopee.Enabled = !isBusy && !_isExporting;
        btnOpenShopeeUpload.Enabled = !isBusy && !_isExporting;
        btnExportLazada.Enabled = !isBusy && !_isExporting;
        btnExportTikTok.Enabled = !isBusy && !_isExporting;
        btnOpenTikTokUpload.Enabled = !isBusy && !_isExporting;
        btnAdd.Enabled = !isBusy;
        btnCopy.Enabled = !isBusy;
        btnEdit.Enabled = !isBusy;
        btnDelete.Enabled = !isBusy;
        btnRefresh.Enabled = !isBusy;
        dgvProducts.Enabled = !isBusy;
        lblSummary.Text = isBusy
            ? "Uploading images to Lazada..."
            : $"Products: {_rows.Count} total | Selected: {_rows.Count(static row => row.Selected)} | Mappings: {_categoryMappings.Count}";
    }

    private static string BuildLazadaUploadSummary(LazadaImageUploadBatchResult result)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Lazada Image Upload Completed");
        builder.AppendLine();
        builder.AppendLine($"Products : {result.Products.Count}");
        builder.AppendLine($"Total    : {result.TotalImages}");
        builder.AppendLine($"Success  : {result.SuccessCount}");
        builder.AppendLine($"Failed   : {result.FailedCount}");
        builder.AppendLine($"Skipped  : {result.SkippedCount}");

        var failedImages = result.Products
            .SelectMany(product => product.Images.Select(image => new { product.ProductCode, Image = image }))
            .Where(static item => item.Image.Status == LazadaUploadStatus.Failed)
            .ToList();

        if (failedImages.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Errors:");
            foreach (var failed in failedImages)
            {
                builder.AppendLine($"- {failed.ProductCode} image {failed.Image.ImageSequence}: {failed.Image.ErrorMessage}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    private List<string> GetShopeeExportErrors(IReadOnlyList<ProductItem> products)
    {
        var errors = new List<string>();

        foreach (var product in products)
        {
            var productErrors = new List<string>();
            var resolvedCategoryCode = ResolveShopeeCategoryCode(product);

            if (string.IsNullOrWhiteSpace(resolvedCategoryCode))
            {
                productErrors.Add("Shopee Category Code mapping ยังว่าง");
            }

            if (product.GetShopeeImageUrls().Count == 0)
            {
                productErrors.Add("Shopee Image URL ต้องมีอย่างน้อย 1 ช่อง");
            }

            if (productErrors.Count > 0)
            {
                errors.Add($"{product.ProductCode}: {string.Join(", ", productErrors)}");
            }
        }

        return errors;
    }

    private string ResolveShopeeCategoryCode(ProductItem product)
    {
        if (_categoryMappings.TryGetValue(product.Category, out var mapping))
        {
            var mappedCode = ShopeeCategoryCodeParser.Normalize(mapping.ShopeeCategoryCode);
            if (!string.IsNullOrWhiteSpace(mappedCode))
            {
                return mappedCode;
            }
        }

        return ShopeeCategoryCodeParser.Normalize(product.ShopeeCategoryCode);
    }

    private static List<string> GetLazadaExportErrors(IReadOnlyList<ProductItem> products, LazadaImageMode imageMode)
    {
        var errors = new List<string>();

        foreach (var product in products)
        {
            var productErrors = new List<string>();
            var imageValues = GetPreferredLazadaImageValues(product, imageMode);
            if (imageValues.Count == 0)
            {
                productErrors.Add(imageMode == LazadaImageMode.LocalFilePath
                    ? "At least one local image path or public image URL is required"
                    : "At least one Lazada Image URL is required");
            }
            else if (imageMode == LazadaImageMode.PublicImageUrl)
            {
                var invalidValues = imageValues
                    .Where(static url => !IsHttpUrl(url))
                    .Select(static url => $"'{url}'")
                    .ToArray();

                if (invalidValues.Length > 0)
                {
                    productErrors.Add($"Lazada Image URL must start with http:// or https://. Current value: {string.Join(", ", invalidValues)}");
                }
            }
            else
            {
                var invalidValues = imageValues
                    .Where(static value => !IsHttpUrl(value) && !File.Exists(value))
                    .Select(static value => $"'{value}'")
                    .ToArray();

                if (invalidValues.Length > 0)
                {
                    productErrors.Add($"Local image path must point to an existing file, or use a valid http:// or https:// URL. Current value: {string.Join(", ", invalidValues)}");
                }
            }

            if (string.IsNullOrWhiteSpace(product.Brand))
            {
                productErrors.Add("Brand is required for the current Lazada template");
            }

            if (string.IsNullOrWhiteSpace(product.BabyMaterial))
            {
                productErrors.Add("Baby Material is required for the current Lazada template");
            }

            if (string.IsNullOrWhiteSpace(product.CountryOfOrigin))
            {
                productErrors.Add("Country Of Origin is required for the current Lazada template");
            }

            if (string.IsNullOrWhiteSpace(product.WarrantyType))
            {
                productErrors.Add("Warranty Type is required for the current Lazada template");
            }

            if (string.IsNullOrWhiteSpace(product.SKU))
            {
                productErrors.Add("SKU is required");
            }

            if (product.Price <= 0)
            {
                productErrors.Add("Price must be greater than 0");
            }

            if (product.Weight <= 0)
            {
                productErrors.Add("Weight must be greater than 0");
            }

            if (product.Length <= 0 || product.Width <= 0 || product.Height <= 0)
            {
                productErrors.Add("Length, Width, and Height must all be greater than 0");
            }

            if (productErrors.Count > 0)
            {
                errors.Add($"{product.ProductCode}: {string.Join(", ", productErrors)}");
            }
        }

        return errors;
    }

    private static List<string> GetTikTokExportErrors(IReadOnlyList<ProductItem> products)
    {
        var errors = new List<string>();

        foreach (var product in products)
        {
            var productErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                productErrors.Add("Product Name is required");
            }

            if (string.IsNullOrWhiteSpace(product.SKU))
            {
                productErrors.Add("SKU is required");
            }

            if (product.Price <= 0 && (!product.VariationPrice.HasValue || product.VariationPrice.Value <= 0))
            {
                productErrors.Add("Price must be greater than 0");
            }

            if (product.Weight <= 0)
            {
                productErrors.Add("Weight must be greater than 0");
            }

            if (product.GetImagePaths().Count == 0 && Enumerable.Range(0, 4).All(index => string.IsNullOrWhiteSpace(product.GetSharedImageUrl(index))))
            {
                productErrors.Add("At least one image path or image URL is required");
            }

            if (productErrors.Count > 0)
            {
                errors.Add($"{product.ProductCode}: {string.Join(", ", productErrors)}");
            }
        }

        return errors;
    }

    private string GetUniqueExportPath(string fileName)
    {
        Directory.CreateDirectory(_pathProvider.ExportDirectory);
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var candidate = Path.Combine(_pathProvider.ExportDirectory, fileName);
        var counter = 1;

        while (File.Exists(candidate))
        {
            candidate = Path.Combine(_pathProvider.ExportDirectory, $"{baseName}-{counter}{extension}");
            counter++;
        }

        return candidate;
    }

    private static ProductItem CreateDuplicateProduct(ProductItem source)
    {
        return new ProductItem
        {
            Id = 0,
            ProductCode = AppendCopySuffix(source.ProductCode, "COPY"),
            ProductName = source.ProductName,
            Description = source.Description,
            Category = source.Category,
            ShopeeCategoryCode = source.ShopeeCategoryCode,
            Price = source.Price,
            Stock = source.Stock,
            Weight = source.Weight,
            Length = source.Length,
            Width = source.Width,
            Height = source.Height,
            SKU = AppendCopySuffix(source.SKU, "COPY"),
            Image1 = source.Image1,
            Image2 = source.Image2,
            Image3 = source.Image3,
            Image4 = source.Image4,
            ShopeeImage1Url = source.ShopeeImage1Url,
            ShopeeImage2Url = source.ShopeeImage2Url,
            ShopeeImage3Url = source.ShopeeImage3Url,
            ShopeeImage4Url = source.ShopeeImage4Url,
            LazadaImage1Url = source.LazadaImage1Url,
            LazadaImage2Url = source.LazadaImage2Url,
            LazadaImage3Url = source.LazadaImage3Url,
            LazadaImage4Url = source.LazadaImage4Url,
            VariationName = source.VariationName,
            VariationOption = source.VariationOption,
            VariationPrice = source.VariationPrice,
            VariationStock = source.VariationStock,
            Brand = source.Brand,
            BabyMaterial = source.BabyMaterial,
            CountryOfOrigin = source.CountryOfOrigin,
            WarrantyType = source.WarrantyType,
            WarrantyPeriod = source.WarrantyPeriod,
            ColorFamily = source.ColorFamily,
            DangerousGoods = source.DangerousGoods,
            DeliveryStandard = source.DeliveryStandard,
            CreatedAt = DateTimeOffset.Now,
            UpdatedAt = DateTimeOffset.Now
        };
    }

    private static string AppendCopySuffix(string value, string suffix)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(trimmed) ? suffix : $"{trimmed}-{suffix}";
    }

    private static IReadOnlyList<string> GetPreferredLazadaImageValues(ProductItem product, LazadaImageMode imageMode)
    {
        if (imageMode == LazadaImageMode.LocalFilePath)
        {
            var localPaths = product.GetImagePaths();
            if (localPaths.Count > 0)
            {
                return localPaths;
            }
        }

        return GetPreferredLazadaImageUrls(product);
    }

    private static IReadOnlyList<string> GetPreferredLazadaImageUrls(ProductItem product)
    {
        var lazadaUrls = product.GetLazadaImageUrls();
        if (lazadaUrls.Count > 0)
        {
            return lazadaUrls;
        }

        return product.GetShopeeImageUrls();
    }

    private static bool IsHttpUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static HttpClient CreateThumbnailHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(12)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("ShopeeLazadaProductStudio/1.0");
        return client;
    }

    private sealed class ProductListRow : IDisposable
    {
        private Image? _thumbnail;

        public ProductListRow(ProductItem product)
        {
            Product = product;
            _thumbnail = LoadThumbnail(GetPreviewSource(product));
        }

        public bool Selected { get; set; }
        public ProductItem Product { get; }

        public Image? Thumbnail => _thumbnail;
        public string ProductCode => Product.ProductCode;
        public string ProductName => Product.ProductName;
        public string Category => Product.Category;
        public string Brand => Product.Brand;
        public decimal Price => Product.Price;
        public int Stock => Product.Stock;
        public string SKU => Product.SKU;
        public string UpdatedAt => Product.UpdatedAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm");

        public void Dispose()
        {
            _thumbnail?.Dispose();
            _thumbnail = null;
        }
    }
}
