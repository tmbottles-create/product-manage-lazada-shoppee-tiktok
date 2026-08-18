using Serilog;
using ShopeeSellerUploader.Contracts.Configuration;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Infrastructure.Configuration;
using ShopeeSellerUploader.Infrastructure.Services;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ShopeeSellerUploader.App.Forms;

public partial class MdiMainForm : Form
{
    private readonly AppSettings _settings;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryMappingRepository _categoryMappingRepository;
    private readonly IMarketplaceCategoryMasterRepository _marketplaceCategoryMasterRepository;
    private readonly IMarketplaceExportService _exportService;
    private readonly ITemplateMetadataService _templateMetadataService;
    private readonly IAiProductSuggestionService _aiProductSuggestionService;
    private readonly ILazadaImageUploadService _lazadaImageUploadService;
    private readonly IShopeeAutomationService _shopeeAutomationService;
    private readonly IApiKeyStore _apiKeyStore;
    private readonly ILogger _logger;
    private readonly PathProvider _pathProvider;
    private ProductWorkspaceForm? _workspaceForm;

    public MdiMainForm(
        AppSettings settings,
        IProductRepository productRepository,
        ICategoryMappingRepository categoryMappingRepository,
        IMarketplaceCategoryMasterRepository marketplaceCategoryMasterRepository,
        IMarketplaceExportService exportService,
        ITemplateMetadataService templateMetadataService,
        IAiProductSuggestionService aiProductSuggestionService,
        ILazadaImageUploadService lazadaImageUploadService,
        IShopeeAutomationService shopeeAutomationService,
        IApiKeyStore apiKeyStore,
        IOneDriveTokenStore lazadaTokenStore,
        ILogger logger,
        PathProvider pathProvider)
    {
        _settings = settings;
        _productRepository = productRepository;
        _categoryMappingRepository = categoryMappingRepository;
        _marketplaceCategoryMasterRepository = marketplaceCategoryMasterRepository;
        _exportService = exportService;
        _templateMetadataService = templateMetadataService;
        _aiProductSuggestionService = aiProductSuggestionService;
        _lazadaImageUploadService = lazadaImageUploadService;
        _shopeeAutomationService = shopeeAutomationService;
        _apiKeyStore = apiKeyStore;
        _logger = logger;
        _pathProvider = pathProvider;

        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        OpenWorkspace();
    }

    private ProductWorkspaceForm OpenWorkspace()
    {
        if (_workspaceForm is not null && !_workspaceForm.IsDisposed)
        {
            _workspaceForm.OpenProductList();
            _workspaceForm.WindowState = FormWindowState.Maximized;
            return _workspaceForm;
        }

        _workspaceForm = new ProductWorkspaceForm(
            _settings,
            _productRepository,
            _categoryMappingRepository,
            _exportService,
            _templateMetadataService,
            _aiProductSuggestionService,
            _lazadaImageUploadService,
            _shopeeAutomationService,
            _logger,
            _pathProvider)
        {
            MdiParent = this,
            WindowState = FormWindowState.Maximized
        };

        _workspaceForm.Show();
        return _workspaceForm;
    }

    protected override async void OnFormClosed(FormClosedEventArgs e)
    {
        await _shopeeAutomationService.DisposeAsync();
        base.OnFormClosed(e);
    }

    private void menuProductList_Click(object? sender, EventArgs e)
    {
        OpenWorkspace();
        SetStatus("Product list is ready.");
    }

    private void menuProductAdd_Click(object? sender, EventArgs e)
    {
        OpenWorkspace().OpenAddProductDialog();
        SetStatus("Opened Add Product dialog.");
    }

    private void menuProductCopy_Click(object? sender, EventArgs e)
    {
        OpenWorkspace().OpenCopySelectedProduct();
        SetStatus("Opened Copy Product dialog.");
    }

    private void menuProductRefresh_Click(object? sender, EventArgs e)
    {
        OpenWorkspace().RefreshWorkspace();
        SetStatus("Refreshing products and category mappings.");
    }

    private void menuMasterCategory_Click(object? sender, EventArgs e)
    {
        OpenWorkspace().OpenCategoryMappingDialog();
        SetStatus("Opened Category Mapping.");
    }

    private async void menuMasterLazadaCategory_Click(object? sender, EventArgs e)
    {
        var names = await _templateMetadataService.GetLazadaSheetNamesAsync();
        using var dialog = new MarketplaceCategoryMasterForm(
            "Lazada Cat Master",
            "Lazada",
            "Lazada",
            names,
            _templateMetadataService,
            _marketplaceCategoryMasterRepository);
        dialog.ShowDialog(this);
        SetStatus("Opened Lazada Cat Master.");
    }

    private async void menuMasterTikTokCategory_Click(object? sender, EventArgs e)
    {
        var names = await _templateMetadataService.GetTikTokCategoryNamesAsync();
        using var dialog = new MarketplaceCategoryMasterForm(
            "TikTok Cat Master",
            "TikTok",
            "TikTok",
            names,
            _templateMetadataService,
            _marketplaceCategoryMasterRepository);
        dialog.ShowDialog(this);
        SetStatus("Opened TikTok Cat Master.");
    }

    private async void menuMasterShopeeCategory_Click(object? sender, EventArgs e)
    {
        var names = await _templateMetadataService.GetShopeeCategoryCodesAsync();
        using var dialog = new MarketplaceCategoryMasterForm(
            "Shopee Cat Master",
            "Shopee",
            "Shopee",
            names,
            _templateMetadataService,
            _marketplaceCategoryMasterRepository);
        dialog.ShowDialog(this);
        SetStatus("Opened Shopee Cat Master.");
    }

    private void menuExportShopee_Click(object? sender, EventArgs e)
    {
        OpenWorkspace().ExportShopee();
        SetStatus("Started Shopee export.");
    }

    private void menuExportLazada_Click(object? sender, EventArgs e)
    {
        OpenWorkspace().ExportLazada();
        SetStatus("Started Lazada export.");
    }

    private void menuExportTikTok_Click(object? sender, EventArgs e)
    {
        OpenWorkspace().ExportTikTok();
        SetStatus("Started TikTok export.");
    }

    private void menuConfigImageKitSetup_Click(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new ImageKitSetupForm(_settings.ImageKit);

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            ApplyImageKitSettings(dialog);
            SaveImageKitSettings(_settings.ImageKit);

            MessageBox.Show(
                this,
                "Saved ImageKit settings. Lazada image upload will use these values on the next upload.",
                "ImageKit Setup",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            SetStatus("Saved ImageKit setup.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "ImageKit Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Failed to save ImageKit setup.");
        }
    }

    private async void menuConfigApiKey_Click(object? sender, EventArgs e)
    {
        using var dialog = new ApiKeyPromptForm();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            SetStatus("Saving OpenAI API key...");
            await Task.Yield();

            if (dialog.ClearExistingKey)
            {
                await _apiKeyStore.DeleteAsync();
                MessageBox.Show(this, "Cleared saved OpenAI API key from this machine.", "OpenAI API Key", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus("Cleared OpenAI API key.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(dialog.ApiKey) && dialog.SaveToMachine)
            {
                await _apiKeyStore.SaveAsync(dialog.ApiKey);
                MessageBox.Show(this, "Saved OpenAI API key securely on this machine.", "OpenAI API Key", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus("Saved OpenAI API key.");
                return;
            }

            MessageBox.Show(this, "API key was not saved. Enable 'save to machine' if you want to persist it securely.", "OpenAI API Key", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SetStatus("OpenAI API key was not saved.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "OpenAI API Key Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Failed to update OpenAI API key.");
        }
    }

    private void menuConfigPaths_Click(object? sender, EventArgs e)
    {
        using var dialog = new ConfigPathForm(
            _settings.ProductCatalog,
            _pathProvider.DatabaseFilePath,
            _pathProvider.TemplateRootDirectory,
            _pathProvider.ExportDirectory);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            SavePathSettings(dialog.DatabasePath, dialog.TemplatePath, dialog.ExportPath);

            _settings.ProductCatalog.DatabaseFileName = dialog.DatabasePath;
            _settings.ProductCatalog.TemplateRootDirectory = dialog.TemplatePath;
            _settings.ProductCatalog.ExportDirectoryName = dialog.ExportPath;

            Directory.CreateDirectory(Path.GetDirectoryName(dialog.DatabasePath) ?? AppContext.BaseDirectory);
            Directory.CreateDirectory(dialog.TemplatePath);
            Directory.CreateDirectory(dialog.ExportPath);

            MessageBox.Show(
                this,
                "Saved path settings successfully.",
                "Config Path",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            SetStatus("Saved config paths.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Config Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Failed to save config paths.");
        }
    }

    private void menuConfigLazadaImageMode_Click(object? sender, EventArgs e)
    {
        using var dialog = new LazadaImageModeForm(_settings.ProductCatalog.LazadaImageMode);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            _settings.ProductCatalog.LazadaImageMode = dialog.SelectedMode;
            SaveLazadaImageMode(dialog.SelectedMode);
            MessageBox.Show(
                this,
                $"Lazada Image Mode saved as {dialog.SelectedMode}.",
                "Lazada Image Mode",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            SetStatus($"Lazada Image Mode is now {dialog.SelectedMode}.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lazada Image Mode", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus("Failed to save Lazada Image Mode.");
        }
    }

    private void menuConfigCheckPrices_Click(object? sender, EventArgs e)
    {
        OpenWorkspace().CheckWebPrices();
        SetStatus("Opened marketplace web price checks.");
    }

    private void menuWindowCascade_Click(object? sender, EventArgs e)
    {
        LayoutMdi(MdiLayout.Cascade);
    }

    private void menuWindowTileHorizontal_Click(object? sender, EventArgs e)
    {
        LayoutMdi(MdiLayout.TileHorizontal);
    }

    private void menuWindowTileVertical_Click(object? sender, EventArgs e)
    {
        LayoutMdi(MdiLayout.TileVertical);
    }

    private void SetStatus(string message)
    {
        toolStripStatusLabel.Text = message;
    }

    private void ApplyImageKitSettings(ImageKitSetupForm dialog)
    {
        _settings.ImageKit.UploadApiUrl = dialog.UploadApiUrl;
        _settings.ImageKit.UrlEndpoint = dialog.UrlEndpoint;
        _settings.ImageKit.UploadFolderPath = dialog.UploadFolderPath;
        _settings.ImageKit.PrivateKey = dialog.PrivateKey;
        _settings.ImageKit.PrivateKeyEnvironmentVariable = dialog.PrivateKeyEnvironmentVariable;
        _settings.ImageKit.TimeoutSeconds = dialog.TimeoutSeconds;
        _settings.ImageKit.MaxUploadSizeMb = dialog.MaxUploadSizeMb;
        _settings.ImageKit.UseUniqueFileName = dialog.UseUniqueFileName;
    }

    private static void SaveLazadaImageMode(LazadaImageMode mode)
    {
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var root = JsonNode.Parse(File.ReadAllText(settingsPath))
            ?? throw new InvalidOperationException("Unable to read appsettings.json.");

        var appSettings = root["AppSettings"]?.AsObject()
            ?? throw new InvalidOperationException("AppSettings section was not found.");

        var productCatalog = appSettings["ProductCatalog"]?.AsObject()
            ?? throw new InvalidOperationException("ProductCatalog section was not found.");

        productCatalog["LazadaImageMode"] = mode.ToString();

        File.WriteAllText(settingsPath, root.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    private static void SaveImageKitSettings(ImageKitOptions options)
    {
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var root = JsonNode.Parse(File.ReadAllText(settingsPath))
            ?? throw new InvalidOperationException("Unable to read appsettings.json.");

        var appSettings = root["AppSettings"]?.AsObject()
            ?? throw new InvalidOperationException("AppSettings section was not found.");

        var imageKit = appSettings["ImageKit"]?.AsObject()
            ?? throw new InvalidOperationException("ImageKit section was not found.");

        imageKit["UploadApiUrl"] = options.UploadApiUrl;
        imageKit["UrlEndpoint"] = options.UrlEndpoint;
        imageKit["UploadFolderPath"] = options.UploadFolderPath;
        imageKit["PrivateKey"] = options.PrivateKey;
        imageKit["PrivateKeyEnvironmentVariable"] = options.PrivateKeyEnvironmentVariable;
        imageKit["TimeoutSeconds"] = options.TimeoutSeconds;
        imageKit["MaxUploadSizeMb"] = options.MaxUploadSizeMb;
        imageKit["UseUniqueFileName"] = options.UseUniqueFileName;

        File.WriteAllText(settingsPath, root.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    private static void SavePathSettings(string databasePath, string templatePath, string exportPath)
    {
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var root = JsonNode.Parse(File.ReadAllText(settingsPath))
            ?? throw new InvalidOperationException("Unable to read appsettings.json.");

        var appSettings = root["AppSettings"]?.AsObject()
            ?? throw new InvalidOperationException("AppSettings section was not found.");

        var productCatalog = appSettings["ProductCatalog"]?.AsObject()
            ?? throw new InvalidOperationException("ProductCatalog section was not found.");

        productCatalog["DatabaseFileName"] = databasePath;
        productCatalog["TemplateRootDirectory"] = templatePath;
        productCatalog["ExportDirectoryName"] = exportPath;

        File.WriteAllText(settingsPath, root.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}
