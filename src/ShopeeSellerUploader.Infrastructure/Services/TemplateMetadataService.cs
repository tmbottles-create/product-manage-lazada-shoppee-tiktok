using ClosedXML.Excel;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Infrastructure.Configuration;

namespace ShopeeSellerUploader.Infrastructure.Services;

public sealed class TemplateMetadataService : ITemplateMetadataService
{
    private readonly PathProvider _pathProvider;

    public TemplateMetadataService(PathProvider pathProvider)
    {
        _pathProvider = pathProvider;
    }

    public Task<IReadOnlyList<string>> GetLazadaSheetNamesAsync(CancellationToken cancellationToken = default)
    {
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

        if (files.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<string>>([]);
        }

        var names = files
            .SelectMany(ReadLazadaSheetNames)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static name => name)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<string>>(names);
    }

    public Task<IReadOnlyList<string>> GetTikTokCategoryNamesAsync(CancellationToken cancellationToken = default)
    {
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

        if (file is null)
        {
            return Task.FromResult<IReadOnlyList<string>>([]);
        }

        using var workbook = new XLWorkbook(file.FullName);
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
