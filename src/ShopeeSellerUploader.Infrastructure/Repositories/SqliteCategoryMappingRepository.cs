using Microsoft.Data.Sqlite;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Infrastructure.Configuration;

namespace ShopeeSellerUploader.Infrastructure.Repositories;

public sealed class SqliteCategoryMappingRepository : ICategoryMappingRepository
{
    private readonly string _connectionString;

    public SqliteCategoryMappingRepository(PathProvider pathProvider)
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
            CREATE TABLE IF NOT EXISTS CategoryMappings (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductCategory TEXT NOT NULL UNIQUE,
                LazadaSheetName TEXT NOT NULL,
                ShopeeCategoryCode TEXT NOT NULL DEFAULT '',
                TikTokCategoryName TEXT NOT NULL DEFAULT '',
                UpdatedAt TEXT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);

        var ensureColumn = connection.CreateCommand();
        ensureColumn.CommandText = "PRAGMA table_info(CategoryMappings);";
        await using var reader = await ensureColumn.ExecuteReaderAsync(cancellationToken);
        var hasShopeeCategoryCode = false;
        var hasTikTokCategoryName = false;
        while (await reader.ReadAsync(cancellationToken))
        {
            var columnName = reader.GetString(1);
            if (string.Equals(columnName, "ShopeeCategoryCode", StringComparison.OrdinalIgnoreCase))
            {
                hasShopeeCategoryCode = true;
            }

            if (string.Equals(columnName, "TikTokCategoryName", StringComparison.OrdinalIgnoreCase))
            {
                hasTikTokCategoryName = true;
            }
        }

        await reader.CloseAsync();

        if (!hasShopeeCategoryCode)
        {
            var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE CategoryMappings ADD COLUMN ShopeeCategoryCode TEXT NOT NULL DEFAULT '';";
            await alter.ExecuteNonQueryAsync(cancellationToken);
        }

        if (!hasTikTokCategoryName)
        {
            var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE CategoryMappings ADD COLUMN TikTokCategoryName TEXT NOT NULL DEFAULT '';";
            await alter.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<CategoryMapping>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<CategoryMapping>();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ProductCategory, LazadaSheetName, ShopeeCategoryCode, TikTokCategoryName, UpdatedAt FROM CategoryMappings ORDER BY ProductCategory;";
        await using var reader2 = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader2.ReadAsync(cancellationToken))
        {
            results.Add(new CategoryMapping
            {
                Id = reader2.GetInt64(0),
                ProductCategory = reader2.GetString(1),
                LazadaSheetName = reader2.GetString(2),
                ShopeeCategoryCode = reader2.GetString(3),
                TikTokCategoryName = reader2.GetString(4),
                UpdatedAt = DateTimeOffset.Parse(reader2.GetString(5))
            });
        }

        return results;
    }

    public async Task SaveManyAsync(IEnumerable<CategoryMapping> mappings, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        foreach (var mapping in mappings)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                """
                INSERT INTO CategoryMappings (ProductCategory, LazadaSheetName, ShopeeCategoryCode, TikTokCategoryName, UpdatedAt)
                VALUES ($ProductCategory, $LazadaSheetName, $ShopeeCategoryCode, $TikTokCategoryName, $UpdatedAt)
                ON CONFLICT(ProductCategory) DO UPDATE SET
                    LazadaSheetName = excluded.LazadaSheetName,
                    ShopeeCategoryCode = excluded.ShopeeCategoryCode,
                    TikTokCategoryName = excluded.TikTokCategoryName,
                    UpdatedAt = excluded.UpdatedAt;
                """;
            command.Parameters.AddWithValue("$ProductCategory", mapping.ProductCategory);
            command.Parameters.AddWithValue("$LazadaSheetName", mapping.LazadaSheetName);
            command.Parameters.AddWithValue("$ShopeeCategoryCode", mapping.ShopeeCategoryCode);
            command.Parameters.AddWithValue("$TikTokCategoryName", mapping.TikTokCategoryName);
            command.Parameters.AddWithValue("$UpdatedAt", DateTimeOffset.Now.ToString("O"));
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
