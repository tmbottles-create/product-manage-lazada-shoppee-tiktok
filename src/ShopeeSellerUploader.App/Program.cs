using Microsoft.Extensions.Configuration;
using Serilog;
using ShopeeSellerUploader.App.Forms;
using ShopeeSellerUploader.App.Services;
using ShopeeSellerUploader.Contracts.Configuration;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Infrastructure.Configuration;
using ShopeeSellerUploader.Infrastructure.Repositories;
using ShopeeSellerUploader.Infrastructure.Services;

namespace ShopeeSellerUploader.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var settings = configuration.GetSection(AppSettings.SectionName).Get<AppSettings>()
            ?? throw new InvalidOperationException("Unable to load app settings.");

        var pathProvider = new PathProvider(settings.Storage, settings.ProductCatalog);
        Directory.CreateDirectory(pathProvider.LogDirectory);
        Directory.CreateDirectory(pathProvider.ExportDirectory);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.File(Path.Combine(pathProvider.WorkingDirectory, settings.Storage.LogFileName), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var logger = Log.Logger.ForContext("Application", "ShopeeSellerUploader");
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, args) => HandleUnhandledException(logger, args.Exception, "UI thread");
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            HandleUnhandledException(logger, args.ExceptionObject as Exception, "AppDomain");
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            HandleUnhandledException(logger, args.Exception, "TaskScheduler");
            args.SetObserved();
        };

        IProductRepository productRepository = new SqliteProductRepository(pathProvider);
        IProductImageUploadStateRepository productImageUploadStateRepository = new SqliteProductImageUploadStateRepository(pathProvider);
        ICategoryMappingRepository categoryMappingRepository = new SqliteCategoryMappingRepository(pathProvider);
        IMarketplaceCategoryMasterRepository marketplaceCategoryMasterRepository = new SqliteMarketplaceCategoryMasterRepository(pathProvider);
        IMarketplaceExportService exportService = new MarketplaceExportService(pathProvider, settings);
        ITemplateMetadataService templateMetadataService = new TemplateMetadataService(pathProvider, marketplaceCategoryMasterRepository);
        IApiKeyStore apiKeyStore = new DpapiApiKeyStore(pathProvider);
        IOneDriveTokenStore oneDriveTokenStore = new DpapiLazadaTokenStore(pathProvider);
        ISessionStore sessionStore = new DpapiSessionStore(pathProvider);
        IAiProductSuggestionService aiProductSuggestionService = new OpenAiProductSuggestionService(settings.OpenAi, apiKeyStore);
        ILazadaImageUploadService lazadaImageUploadService = new LazadaImageUploadService(
            settings.ImageKit,
            productRepository,
            productImageUploadStateRepository,
            logger);
        IShopeeAutomationService shopeeAutomationService = new ShopeeAutomationService(settings, sessionStore, logger);

        productRepository.InitializeAsync().GetAwaiter().GetResult();
        productImageUploadStateRepository.InitializeAsync().GetAwaiter().GetResult();
        categoryMappingRepository.InitializeAsync().GetAwaiter().GetResult();
        marketplaceCategoryMasterRepository.InitializeAsync().GetAwaiter().GetResult();

        try
        {
            Application.Run(new MdiMainForm(
                settings,
                productRepository,
                categoryMappingRepository,
                marketplaceCategoryMasterRepository,
                exportService,
                templateMetadataService,
                aiProductSuggestionService,
                lazadaImageUploadService,
                shopeeAutomationService,
                apiKeyStore,
                oneDriveTokenStore,
                logger,
                pathProvider));
        }
        catch (Exception ex)
        {
            HandleUnhandledException(logger, ex, "Application.Run");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void HandleUnhandledException(Serilog.ILogger logger, Exception? exception, string source)
    {
        if (exception is null)
        {
            return;
        }

        logger.Error(exception, "Unhandled exception from {Source}", source);

        try
        {
            MessageBox.Show(
                $"The app hit an unexpected error on {source}.{Environment.NewLine}{Environment.NewLine}{exception.Message}",
                "Unexpected Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch
        {
        }
    }
}
