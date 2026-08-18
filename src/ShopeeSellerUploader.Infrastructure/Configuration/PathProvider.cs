using ShopeeSellerUploader.Contracts.Configuration;

namespace ShopeeSellerUploader.Infrastructure.Configuration;

public sealed class PathProvider
{
    private readonly StorageOptions _storageOptions;
    private readonly ProductCatalogOptions _productCatalogOptions;

    public PathProvider(StorageOptions storageOptions, ProductCatalogOptions productCatalogOptions)
    {
        _storageOptions = storageOptions;
        _productCatalogOptions = productCatalogOptions;
        WorkingDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, _storageOptions.WorkingDirectory));
        Directory.CreateDirectory(WorkingDirectory);
    }

    public string WorkingDirectory { get; }
    public string SessionFilePath => Path.Combine(WorkingDirectory, _storageOptions.SessionFileName);
    public string CheckpointFilePath => Path.Combine(WorkingDirectory, _storageOptions.CheckpointFileName);
    public string ResultSnapshotFilePath => Path.Combine(WorkingDirectory, _storageOptions.ResultSnapshotFileName);
    public string TemplateFilePath => Path.Combine(WorkingDirectory, _storageOptions.TemplateFileName);
    public string OpenAiApiKeyFilePath => Path.Combine(WorkingDirectory, _storageOptions.OpenAiApiKeyFileName);
    public string LazadaTokenFilePath => Path.Combine(WorkingDirectory, _storageOptions.LazadaTokenFileName);
    public string LogDirectory => Path.Combine(WorkingDirectory, "logs");
    public string DatabaseFilePath => Path.Combine(WorkingDirectory, _productCatalogOptions.DatabaseFileName);
    public string ExportDirectory => Path.Combine(WorkingDirectory, _productCatalogOptions.ExportDirectoryName);
    public string TemplateRootDirectory => _productCatalogOptions.TemplateRootDirectory;
}
