using ClosedXML.Excel;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Infrastructure.Configuration;

namespace ShopeeSellerUploader.Infrastructure.Services;

public sealed class TemplateMetadataService : ITemplateMetadataService
{
    private readonly PathProvider _pathProvider;
    private readonly IMarketplaceCategoryMasterRepository _marketplaceCategoryMasterRepository;

    public TemplateMetadataService(
        PathProvider pathProvider,
        IMarketplaceCategoryMasterRepository marketplaceCategoryMasterRepository)
    {
        _pathProvider = pathProvider;
        _marketplaceCategoryMasterRepository = marketplaceCategoryMasterRepository;
    }

    public Task<IReadOnlyList<string>> GetLazadaSheetNamesAsync(CancellationToken cancellationToken = default)
    {
        var masterNames = _marketplaceCategoryMasterRepository.GetNamesAsync("Lazada", cancellationToken).GetAwaiter().GetResult();
        if (masterNames.Count > 0)
        {
            return Task.FromResult<IReadOnlyList<string>>(masterNames
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
                .ToList());
        }

        var candidateDirectories = new List<DirectoryInfo>();

        var templateDirectory = new DirectoryInfo(_pathProvider.TemplateRootDirectory);
        if (templateDirectory.Exists)
        {
            candidateDirectories.Add(templateDirectory);
        }

        var downloadsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");
        if (!string.IsNullOrWhiteSpace(downloadsPath))
        {
            var downloadsDirectory = new DirectoryInfo(downloadsPath);
            if (downloadsDirectory.Exists)
            {
                candidateDirectories.Add(downloadsDirectory);
            }
        }

        var files = candidateDirectories
            .SelectMany(dir => dir.GetFiles("advancedPublish*.xlsx"))
            .Concat(candidateDirectories.SelectMany(dir => dir.GetFiles("*lazada*.xlsx")))
            .GroupBy(static file => file.FullName, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .OrderByDescending(static file => file.LastWriteTimeUtc)
            .ToList();

        var names = files
            .SelectMany(ReadLazadaSheetNames)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static name => name)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<string>>(names);
    }

    public Task<IReadOnlyList<string>> GetShopeeCategoryCodesAsync(CancellationToken cancellationToken = default)
    {
        var names = _marketplaceCategoryMasterRepository.GetNamesAsync("Shopee", cancellationToken).GetAwaiter().GetResult()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<string>>(names);
    }

    public Task<IReadOnlyList<string>> GetTikTokCategoryNamesAsync(CancellationToken cancellationToken = default)
    {
        var masterNames = _marketplaceCategoryMasterRepository.GetNamesAsync("TikTok", cancellationToken).GetAwaiter().GetResult();
        if (masterNames.Count > 0)
        {
            return Task.FromResult<IReadOnlyList<string>>(masterNames
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
                .ToList());
        }

        var candidateDirectories = new List<DirectoryInfo>();

        var templateDirectory = new DirectoryInfo(_pathProvider.TemplateRootDirectory);
        if (templateDirectory.Exists)
        {
            candidateDirectories.Add(templateDirectory);
        }

        var downloadsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");
        if (!string.IsNullOrWhiteSpace(downloadsPath))
        {
            var downloadsDirectory = new DirectoryInfo(downloadsPath);
            if (downloadsDirectory.Exists)
            {
                candidateDirectories.Add(downloadsDirectory);
            }
        }

        var file = candidateDirectories
            .SelectMany(dir => dir.GetFiles("Tiktoksellercenter_batchupload_*_template.xlsx"))
            .OrderByDescending(static x => x.LastWriteTimeUtc)
            .FirstOrDefault()
            ?? candidateDirectories
                .SelectMany(dir => dir.GetFiles("*TikTok*.xlsx"))
                .OrderByDescending(static x => x.LastWriteTimeUtc)
                .FirstOrDefault();

        var templateNames = new List<string>();
        if (file is not null)
        {
            using var workbook = new XLWorkbook(file.FullName);
            var worksheet = workbook.Worksheets.FirstOrDefault(ws =>
                string.Equals(ws.Name, "Category", StringComparison.OrdinalIgnoreCase));

            if (worksheet is not null)
            {
                templateNames = worksheet.Column(1)
                    .CellsUsed()
                    .Select(cell => cell.GetString().Trim())
                    .Where(static name => !string.IsNullOrWhiteSpace(name))
                    .ToList();
            }
        }

        var names = templateNames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static name => name)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<string>>(names);
    }

    public Task<IReadOnlyList<string>> GetLazadaSheetNamesFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return Task.FromResult<IReadOnlyList<string>>([]);
        }

        var names = ReadLazadaSheetNames(new FileInfo(filePath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static name => name)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<string>>(names);
    }

    public Task<IReadOnlyList<string>> GetTikTokCategoryNamesFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return Task.FromResult<IReadOnlyList<string>>([]);
        }

        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.FirstOrDefault(ws =>
            string.Equals(ws.Name, "Category", StringComparison.OrdinalIgnoreCase));

        if (worksheet is null)
        {
            return Task.FromResult<IReadOnlyList<string>>([]);
        }

        var names = worksheet.Column(1)
            .CellsUsed()
            .Select(cell => cell.GetString().Trim())
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static name => name)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<string>>(names);
    }

    private static IReadOnlyList<string> ReadLazadaSheetNames(FileInfo file)
    {
        using var workbook = new XLWorkbook(file.FullName);
        return workbook.Worksheets
            .Select(ws => ws.Name)
            .Where(static name =>
                !name.EndsWith("_hide", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(name, "INDEX", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(name, "สถานะ", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(name, "global_hide", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
