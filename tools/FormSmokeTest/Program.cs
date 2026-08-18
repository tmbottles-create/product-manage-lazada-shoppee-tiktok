using ShopeeSellerUploader.App.Forms;
using ShopeeSellerUploader.Contracts.Configuration;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Infrastructure.Configuration;
using ShopeeSellerUploader.Infrastructure.Services;

ApplicationConfiguration.Initialize();

var appBase = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "ShopeeSellerUploader.App", "bin", "Debug", "net8.0-windows"));
AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", appBase);

var storage = new StorageOptions
{
    WorkingDirectory = "Data",
    SessionFileName = "session.dat",
    CheckpointFileName = "checkpoint.json",
    ResultSnapshotFileName = "result-snapshot.json",
    TemplateFileName = "ShopeeProductTemplate.xlsx",
    LogFileName = @"logs\product-catalog-.log",
    OpenAiApiKeyFileName = "openai-api-key.bin"
};

var catalog = new ProductCatalogOptions
{
    DatabaseFileName = "product-catalog.db",
    ExportDirectoryName = "exports",
    TemplateRootDirectory = @"D:\shoppee-lazada-templete"
};

var openAi = new OpenAiOptions();
var pathProvider = new PathProvider(storage, catalog);
IApiKeyStore apiKeyStore = new DpapiApiKeyStore(pathProvider);
IAiProductSuggestionService aiService = new OpenAiProductSuggestionService(openAi, apiKeyStore);

using var form = new ProductEditForm(aiService, apiKeyStore);
Console.WriteLine("OK: ProductEditForm created successfully");
