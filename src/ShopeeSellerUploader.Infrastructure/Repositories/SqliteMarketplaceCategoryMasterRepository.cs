using Microsoft.Data.Sqlite;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Infrastructure.Configuration;

namespace ShopeeSellerUploader.Infrastructure.Repositories;

public sealed class SqliteMarketplaceCategoryMasterRepository : IMarketplaceCategoryMasterRepository
{
    private readonly string _connectionString;

    public SqliteMarketplaceCategoryMasterRepository(PathProvider pathProvider)
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
            CREATE TABLE IF NOT EXISTS MarketplaceCategoryMasters (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Marketplace TEXT NOT NULL,
                Name TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                UNIQUE(Marketplace, Name)
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetNamesAsync(string marketplace, CancellationToken cancellationToken = default)
    {
        var results = new List<string>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Name
            FROM MarketplaceCategoryMasters
            WHERE Marketplace = $Marketplace
            ORDER BY Name;
            """;
        command.Parameters.AddWithValue("$Marketplace", marketplace);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(reader.GetString(0));
        }

        return results;
    }

    public async Task ReplaceAllAsync(string marketplace, IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        var normalizedNames = names
            .Select(static name => name?.Trim() ?? string.Empty)
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        var deleteCommand = connection.CreateCommand();
        deleteCommand.Transaction = transaction;
        deleteCommand.CommandText = "DELETE FROM MarketplaceCategoryMasters WHERE Marketplace = $Marketplace;";
        deleteCommand.Parameters.AddWithValue("$Marketplace", marketplace);
        await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

        foreach (var name in normalizedNames)
        {
            var insertCommand = connection.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText =
                """
                INSERT INTO MarketplaceCategoryMasters (Marketplace, Name, UpdatedAt)
                VALUES ($Marketplace, $Name, $UpdatedAt);
                """;
            insertCommand.Parameters.AddWithValue("$Marketplace", marketplace);
            insertCommand.Parameters.AddWithValue("$Name", name);
            insertCommand.Parameters.AddWithValue("$UpdatedAt", DateTimeOffset.Now.ToString("O"));
            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
