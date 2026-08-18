using System.Text.Json;
using ShopeeSellerUploader.Contracts.Configuration;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Infrastructure.Configuration;
using ShopeeSellerUploader.Infrastructure.Repositories;
using ShopeeSellerUploader.Infrastructure.Services;

var appBaseDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "ShopeeSellerUploader.App", "bin", "Debug", "net8.0-windows"));

AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", appBaseDirectory);

var storage = new StorageOptions
{
    WorkingDirectory = "Data",
    SessionFileName = "session.dat",
    CheckpointFileName = "checkpoint.json",
    ResultSnapshotFileName = "result-snapshot.json",
    TemplateFileName = "ShopeeProductTemplate.xlsx",
    LogFileName = @"logs\product-catalog-.log"
};

var catalog = new ProductCatalogOptions
{
    DatabaseFileName = "product-catalog.db",
    ExportDirectoryName = "exports",
    TemplateRootDirectory = @"D:\shoppee-lazada-templete"
};

var pathProvider = new PathProvider(storage, catalog);
IProductRepository productRepository = new SqliteProductRepository(pathProvider);
ICategoryMappingRepository mappingRepository = new SqliteCategoryMappingRepository(pathProvider);
IMarketplaceExportService exportService = new MarketplaceExportService(pathProvider);

await productRepository.InitializeAsync();
await mappingRepository.InitializeAsync();

var products = await productRepository.GetAllAsync();
var mappings = await mappingRepository.GetAllAsync();
var mappingDictionary = mappings.ToDictionary(x => x.ProductCategory, x => x.LazadaSheetName, StringComparer.OrdinalIgnoreCase);

string? outputPath = null;
if (products.Count > 0)
{
    outputPath = Path.Combine(pathProvider.ExportDirectory, $"Verification-Lazada-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx");
    await exportService.ExportAsync(MarketplaceType.Lazada, products, outputPath, mappingDictionary);
}

var result = new
{
    DatabaseFilePath = pathProvider.DatabaseFilePath,
    ExportFilePath = outputPath,
    ProductCount = products.Count,
    Status = products.Count > 0 ? "Exported" : "NoProductsInDatabase",
    Products = products.Select(product => new
    {
        product.ProductCode,
        product.ProductName,
        product.Category,
        product.Brand,
        product.WarrantyType,
        product.WarrantyPeriod,
        product.ColorFamily,
        product.DangerousGoods,
        product.DeliveryStandard,
        product.SKU,
        product.Price,
        product.Stock
    }),
    CategoryMappings = mappings.Select(mapping => new
    {
        mapping.ProductCategory,
        mapping.LazadaSheetName
    })
};

Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
{
    WriteIndented = true
}));
