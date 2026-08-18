using System.Text.Json;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Infrastructure.Configuration;

namespace ShopeeSellerUploader.Infrastructure.Repositories;

public sealed class JsonCheckpointRepository : ICheckpointRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly PathProvider _pathProvider;

    public JsonCheckpointRepository(PathProvider pathProvider)
    {
        _pathProvider = pathProvider;
    }

    public async Task<CheckpointState?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_pathProvider.CheckpointFilePath))
        {
            return null;
        }

        await using var stream = File.OpenRead(_pathProvider.CheckpointFilePath);
        return await JsonSerializer.DeserializeAsync<CheckpointState>(stream, JsonOptions, cancellationToken);
    }

    public async Task SaveAsync(CheckpointState state, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_pathProvider.CheckpointFilePath)!);
        await using var stream = File.Create(_pathProvider.CheckpointFilePath);
        await JsonSerializer.SerializeAsync(stream, state, JsonOptions, cancellationToken);
    }
}
