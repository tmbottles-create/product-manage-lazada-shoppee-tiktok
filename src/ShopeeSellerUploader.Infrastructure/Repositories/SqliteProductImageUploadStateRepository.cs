using Microsoft.Data.Sqlite;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Infrastructure.Configuration;

namespace ShopeeSellerUploader.Infrastructure.Repositories;

public sealed class SqliteProductImageUploadStateRepository : IProductImageUploadStateRepository
{
    private readonly string _connectionString;

    public SqliteProductImageUploadStateRepository(PathProvider pathProvider)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = pathProvider.DatabaseFilePath
        }.ToString();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS ProductImageUploadStates (
                ProductId INTEGER NOT NULL,
                ImageSequence INTEGER NOT NULL,
                LocalImagePath TEXT NOT NULL DEFAULT '',
                LazadaImageUrl TEXT NOT NULL DEFAULT '',
                UploadStatus TEXT NOT NULL DEFAULT 'Waiting',
                UploadError TEXT NOT NULL DEFAULT '',
                UploadedAt TEXT NULL,
                UpdatedAt TEXT NOT NULL,
                PRIMARY KEY (ProductId, ImageSequence)
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductImageUploadState>> GetByProductIdAsync(long productId, CancellationToken cancellationToken = default)
    {
        var results = new List<ProductImageUploadState>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT ProductId, ImageSequence, LocalImagePath, LazadaImageUrl, UploadStatus, UploadError, UploadedAt, UpdatedAt
            FROM ProductImageUploadStates
            WHERE ProductId = $ProductId
            ORDER BY ImageSequence;
            """;
        command.Parameters.AddWithValue("$ProductId", productId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ProductImageUploadState
            {
                ProductId = reader.GetInt64(0),
                ImageSequence = reader.GetInt32(1),
                LocalImagePath = reader.GetString(2),
                LazadaImageUrl = reader.GetString(3),
                Status = Enum.TryParse<LazadaUploadStatus>(reader.GetString(4), true, out var status) ? status : LazadaUploadStatus.Waiting,
                UploadError = reader.GetString(5),
                UploadedAt = reader.IsDBNull(6) ? null : DateTimeOffset.Parse(reader.GetString(6)),
                UpdatedAt = DateTimeOffset.Parse(reader.GetString(7))
            });
        }

        return results;
    }

    public async Task SaveAsync(ProductImageUploadState state, CancellationToken cancellationToken = default)
    {
        state.UpdatedAt = DateTimeOffset.Now;

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO ProductImageUploadStates (
                ProductId, ImageSequence, LocalImagePath, LazadaImageUrl, UploadStatus, UploadError, UploadedAt, UpdatedAt
            )
            VALUES (
                $ProductId, $ImageSequence, $LocalImagePath, $LazadaImageUrl, $UploadStatus, $UploadError, $UploadedAt, $UpdatedAt
            )
            ON CONFLICT(ProductId, ImageSequence) DO UPDATE SET
                LocalImagePath = excluded.LocalImagePath,
                LazadaImageUrl = excluded.LazadaImageUrl,
                UploadStatus = excluded.UploadStatus,
                UploadError = excluded.UploadError,
                UploadedAt = excluded.UploadedAt,
                UpdatedAt = excluded.UpdatedAt;
            """;
        command.Parameters.AddWithValue("$ProductId", state.ProductId);
        command.Parameters.AddWithValue("$ImageSequence", state.ImageSequence);
        command.Parameters.AddWithValue("$LocalImagePath", state.LocalImagePath);
        command.Parameters.AddWithValue("$LazadaImageUrl", state.LazadaImageUrl);
        command.Parameters.AddWithValue("$UploadStatus", state.Status.ToString());
        command.Parameters.AddWithValue("$UploadError", state.UploadError);
        command.Parameters.AddWithValue("$UploadedAt", state.UploadedAt?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$UpdatedAt", state.UpdatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
