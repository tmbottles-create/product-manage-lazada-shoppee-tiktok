$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$appBase = Join-Path $root 'src\ShopeeSellerUploader.App\bin\Debug\net8.0-windows'

$assemblies = @(
    (Join-Path $appBase 'ShopeeSellerUploader.Core.dll'),
    (Join-Path $appBase 'ShopeeSellerUploader.Contracts.dll'),
    (Join-Path $appBase 'ShopeeSellerUploader.Infrastructure.dll'),
    (Join-Path $appBase 'Microsoft.Data.Sqlite.dll'),
    (Join-Path $appBase 'ClosedXML.dll'),
    (Join-Path $appBase 'DocumentFormat.OpenXml.dll'),
    (Join-Path $appBase 'ExcelNumberFormat.dll'),
    (Join-Path $appBase 'SixLabors.Fonts.dll'),
    (Join-Path $appBase 'RBush.dll')
)

foreach ($assembly in $assemblies) {
    Add-Type -Path $assembly
}

$storage = [ShopeeSellerUploader.Contracts.Configuration.StorageOptions]::new()
$storage.WorkingDirectory = 'Data'
$storage.SessionFileName = 'session.dat'
$storage.CheckpointFileName = 'checkpoint.json'
$storage.ResultSnapshotFileName = 'result-snapshot.json'
$storage.TemplateFileName = 'ShopeeProductTemplate.xlsx'
$storage.LogFileName = 'logs\product-catalog-.log'

$catalog = [ShopeeSellerUploader.Contracts.Configuration.ProductCatalogOptions]::new()
$catalog.DatabaseFileName = 'product-catalog.db'
$catalog.ExportDirectoryName = 'exports'
$catalog.TemplateRootDirectory = 'D:\shoppee-lazada-templete'

[System.AppDomain]::CurrentDomain.SetData('APP_CONTEXT_BASE_DIRECTORY', "$appBase\")
$pathProvider = [ShopeeSellerUploader.Infrastructure.Configuration.PathProvider]::new($storage, $catalog)
$productRepository = [ShopeeSellerUploader.Infrastructure.Repositories.SqliteProductRepository]::new($pathProvider)
$mappingRepository = [ShopeeSellerUploader.Infrastructure.Repositories.SqliteCategoryMappingRepository]::new($pathProvider)
$exportService = [ShopeeSellerUploader.Infrastructure.Services.MarketplaceExportService]::new($pathProvider)

$productRepository.InitializeAsync().GetAwaiter().GetResult()
$mappingRepository.InitializeAsync().GetAwaiter().GetResult()

$products = $productRepository.GetAllAsync().GetAwaiter().GetResult()
$mappings = $mappingRepository.GetAllAsync().GetAwaiter().GetResult()

$mappingDictionary = [System.Collections.Generic.Dictionary[string,string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($mapping in $mappings) {
    $mappingDictionary[$mapping.ProductCategory] = $mapping.LazadaSheetName
}

$selectedProducts = [System.Collections.Generic.List[ShopeeSellerUploader.Core.Models.ProductItem]]::new()
foreach ($product in $products) {
    $selectedProducts.Add($product)
}

$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$outputPath = Join-Path $pathProvider.ExportDirectory "Verification-Lazada-$timestamp.xlsx"
$null = $exportService.ExportAsync(
    [ShopeeSellerUploader.Core.Models.MarketplaceType]::Lazada,
    $selectedProducts,
    $outputPath,
    $mappingDictionary).GetAwaiter().GetResult()

$productSummary = foreach ($product in $products) {
    [PSCustomObject]@{
        ProductCode = $product.ProductCode
        ProductName = $product.ProductName
        Category = $product.Category
        Brand = $product.Brand
        SKU = $product.SKU
        Price = $product.Price
        Stock = $product.Stock
        DangerousGoods = $product.DangerousGoods
        DeliveryStandard = $product.DeliveryStandard
    }
}

$mappingSummary = foreach ($mapping in $mappings) {
    [PSCustomObject]@{
        ProductCategory = $mapping.ProductCategory
        LazadaSheetName = $mapping.LazadaSheetName
    }
}

[PSCustomObject]@{
    DatabaseFilePath = $pathProvider.DatabaseFilePath
    ExportFilePath = $outputPath
    ProductCount = $products.Count
    Products = $productSummary
    CategoryMappings = $mappingSummary
} | ConvertTo-Json -Depth 6
