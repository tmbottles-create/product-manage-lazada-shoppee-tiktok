using Microsoft.Data.Sqlite;
using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Infrastructure.Configuration;

namespace ShopeeSellerUploader.Infrastructure.Repositories;

public sealed class SqliteProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public SqliteProductRepository(PathProvider pathProvider)
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

        var createCommand = connection.CreateCommand();
        createCommand.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductCode TEXT NOT NULL,
                ProductName TEXT NOT NULL,
                Description TEXT NOT NULL,
                Category TEXT NOT NULL,
                ShopeeCategoryCode TEXT NOT NULL DEFAULT '',
                Price REAL NOT NULL,
                Stock INTEGER NOT NULL,
                Weight REAL NOT NULL,
                Length REAL NOT NULL,
                Width REAL NOT NULL,
                Height REAL NOT NULL,
                SKU TEXT NOT NULL,
                Image1 TEXT NOT NULL,
                Image2 TEXT NOT NULL,
                Image3 TEXT NOT NULL,
                Image4 TEXT NOT NULL DEFAULT '',
                ShopeeImage1Url TEXT NOT NULL DEFAULT '',
                ShopeeImage2Url TEXT NOT NULL DEFAULT '',
                ShopeeImage3Url TEXT NOT NULL DEFAULT '',
                ShopeeImage4Url TEXT NOT NULL DEFAULT '',
                LazadaImage1Url TEXT NOT NULL DEFAULT '',
                LazadaImage2Url TEXT NOT NULL DEFAULT '',
                LazadaImage3Url TEXT NOT NULL DEFAULT '',
                LazadaImage4Url TEXT NOT NULL DEFAULT '',
                VariationName TEXT NOT NULL,
                VariationOption TEXT NOT NULL,
                VariationPrice REAL NULL,
                VariationStock INTEGER NULL,
                Brand TEXT NOT NULL DEFAULT '',
                BabyMaterial TEXT NOT NULL DEFAULT '',
                CountryOfOrigin TEXT NOT NULL DEFAULT '',
                WarrantyType TEXT NOT NULL DEFAULT '',
                WarrantyPeriod TEXT NOT NULL DEFAULT '',
                ColorFamily TEXT NOT NULL DEFAULT '',
                DangerousGoods TEXT NOT NULL DEFAULT 'No',
                DeliveryStandard TEXT NOT NULL DEFAULT 'Yes',
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            """;
        await createCommand.ExecuteNonQueryAsync(cancellationToken);

        await EnsureColumnAsync(connection, "Products", "Brand", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "BabyMaterial", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "WarrantyType", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "WarrantyPeriod", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "ColorFamily", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "DangerousGoods", "TEXT NOT NULL DEFAULT 'No'", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "DeliveryStandard", "TEXT NOT NULL DEFAULT 'Yes'", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "ShopeeCategoryCode", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "ShopeeImage1Url", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "ShopeeImage2Url", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "ShopeeImage3Url", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "Image4", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "ShopeeImage4Url", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "LazadaImage1Url", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "LazadaImage2Url", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "LazadaImage3Url", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "LazadaImage4Url", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "Products", "CountryOfOrigin", "TEXT NOT NULL DEFAULT ''", cancellationToken);
    }

    public async Task<IReadOnlyList<ProductItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<ProductItem>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                Id, ProductCode, ProductName, Description, Category, ShopeeCategoryCode,
                Price, Stock, Weight, Length, Width, Height, SKU,
                Image1, Image2, Image3, Image4, ShopeeImage1Url, ShopeeImage2Url, ShopeeImage3Url, ShopeeImage4Url,
                LazadaImage1Url, LazadaImage2Url, LazadaImage3Url, LazadaImage4Url,
                VariationName, VariationOption, VariationPrice, VariationStock, Brand, BabyMaterial, CountryOfOrigin, WarrantyType, WarrantyPeriod,
                ColorFamily, DangerousGoods, DeliveryStandard, CreatedAt, UpdatedAt
            FROM Products
            ORDER BY UpdatedAt DESC, Id DESC;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ProductItem
            {
                Id = reader.GetInt64(0),
                ProductCode = reader.GetString(1),
                ProductName = reader.GetString(2),
                Description = reader.GetString(3),
                Category = reader.GetString(4),
                ShopeeCategoryCode = reader.GetString(5),
                Price = reader.GetDecimal(6),
                Stock = reader.GetInt32(7),
                Weight = reader.GetDecimal(8),
                Length = reader.GetDecimal(9),
                Width = reader.GetDecimal(10),
                Height = reader.GetDecimal(11),
                SKU = reader.GetString(12),
                Image1 = reader.GetString(13),
                Image2 = reader.GetString(14),
                Image3 = reader.GetString(15),
                Image4 = reader.GetString(16),
                ShopeeImage1Url = reader.GetString(17),
                ShopeeImage2Url = reader.GetString(18),
                ShopeeImage3Url = reader.GetString(19),
                ShopeeImage4Url = reader.GetString(20),
                LazadaImage1Url = reader.GetString(21),
                LazadaImage2Url = reader.GetString(22),
                LazadaImage3Url = reader.GetString(23),
                LazadaImage4Url = reader.GetString(24),
                VariationName = reader.GetString(25),
                VariationOption = reader.GetString(26),
                VariationPrice = reader.IsDBNull(27) ? null : reader.GetDecimal(27),
                VariationStock = reader.IsDBNull(28) ? null : reader.GetInt32(28),
                Brand = reader.GetString(29),
                BabyMaterial = reader.GetString(30),
                CountryOfOrigin = reader.GetString(31),
                WarrantyType = reader.GetString(32),
                WarrantyPeriod = reader.GetString(33),
                ColorFamily = reader.GetString(34),
                DangerousGoods = reader.GetString(35),
                DeliveryStandard = reader.GetString(36),
                CreatedAt = DateTimeOffset.Parse(reader.GetString(37)),
                UpdatedAt = DateTimeOffset.Parse(reader.GetString(38))
            });
        }

        return results;
    }

    public async Task<ProductItem> SaveAsync(ProductItem product, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        if (product.Id <= 0)
        {
            product.CreatedAt = DateTimeOffset.Now;
            product.UpdatedAt = product.CreatedAt;

            var insert = connection.CreateCommand();
            insert.CommandText =
                """
                INSERT INTO Products (
                    ProductCode, ProductName, Description, Category, ShopeeCategoryCode, Price, Stock,
                    Weight, Length, Width, Height, SKU, Image1, Image2, Image3, Image4,
                    ShopeeImage1Url, ShopeeImage2Url, ShopeeImage3Url, ShopeeImage4Url,
                    LazadaImage1Url, LazadaImage2Url, LazadaImage3Url, LazadaImage4Url,
                    VariationName, VariationOption, VariationPrice, VariationStock,
                    Brand, BabyMaterial, CountryOfOrigin, WarrantyType, WarrantyPeriod, ColorFamily, DangerousGoods,
                    DeliveryStandard, CreatedAt, UpdatedAt
                )
                VALUES (
                    $ProductCode, $ProductName, $Description, $Category, $ShopeeCategoryCode, $Price, $Stock,
                    $Weight, $Length, $Width, $Height, $SKU, $Image1, $Image2, $Image3, $Image4,
                    $ShopeeImage1Url, $ShopeeImage2Url, $ShopeeImage3Url, $ShopeeImage4Url,
                    $LazadaImage1Url, $LazadaImage2Url, $LazadaImage3Url, $LazadaImage4Url,
                    $VariationName, $VariationOption, $VariationPrice, $VariationStock,
                    $Brand, $BabyMaterial, $CountryOfOrigin, $WarrantyType, $WarrantyPeriod, $ColorFamily, $DangerousGoods,
                    $DeliveryStandard, $CreatedAt, $UpdatedAt
                );
                SELECT last_insert_rowid();
                """;
            AddParameters(insert, product);
            product.Id = (long)(await insert.ExecuteScalarAsync(cancellationToken) ?? 0L);
            return product;
        }

        product.UpdatedAt = DateTimeOffset.Now;

        var update = connection.CreateCommand();
        update.CommandText =
            """
            UPDATE Products
            SET
                ProductCode = $ProductCode,
                ProductName = $ProductName,
                Description = $Description,
                Category = $Category,
                ShopeeCategoryCode = $ShopeeCategoryCode,
                Price = $Price,
                Stock = $Stock,
                Weight = $Weight,
                Length = $Length,
                Width = $Width,
                Height = $Height,
                SKU = $SKU,
                Image1 = $Image1,
                Image2 = $Image2,
                Image3 = $Image3,
                Image4 = $Image4,
                ShopeeImage1Url = $ShopeeImage1Url,
                ShopeeImage2Url = $ShopeeImage2Url,
                ShopeeImage3Url = $ShopeeImage3Url,
                ShopeeImage4Url = $ShopeeImage4Url,
                LazadaImage1Url = $LazadaImage1Url,
                LazadaImage2Url = $LazadaImage2Url,
                LazadaImage3Url = $LazadaImage3Url,
                LazadaImage4Url = $LazadaImage4Url,
                VariationName = $VariationName,
                VariationOption = $VariationOption,
                VariationPrice = $VariationPrice,
                VariationStock = $VariationStock,
                Brand = $Brand,
                BabyMaterial = $BabyMaterial,
                CountryOfOrigin = $CountryOfOrigin,
                WarrantyType = $WarrantyType,
                WarrantyPeriod = $WarrantyPeriod,
                ColorFamily = $ColorFamily,
                DangerousGoods = $DangerousGoods,
                DeliveryStandard = $DeliveryStandard,
                UpdatedAt = $UpdatedAt
            WHERE Id = $Id;
            """;
        AddParameters(update, product);
        update.Parameters.AddWithValue("$Id", product.Id);
        await update.ExecuteNonQueryAsync(cancellationToken);
        return product;
    }

    public async Task DeleteAsync(long productId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Products WHERE Id = $Id;";
        command.Parameters.AddWithValue("$Id", productId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddParameters(SqliteCommand command, ProductItem product)
    {
        command.Parameters.AddWithValue("$ProductCode", product.ProductCode);
        command.Parameters.AddWithValue("$ProductName", product.ProductName);
        command.Parameters.AddWithValue("$Description", product.Description);
        command.Parameters.AddWithValue("$Category", product.Category);
        command.Parameters.AddWithValue("$ShopeeCategoryCode", product.ShopeeCategoryCode);
        command.Parameters.AddWithValue("$Price", product.Price);
        command.Parameters.AddWithValue("$Stock", product.Stock);
        command.Parameters.AddWithValue("$Weight", product.Weight);
        command.Parameters.AddWithValue("$Length", product.Length);
        command.Parameters.AddWithValue("$Width", product.Width);
        command.Parameters.AddWithValue("$Height", product.Height);
        command.Parameters.AddWithValue("$SKU", product.SKU);
        command.Parameters.AddWithValue("$Image1", product.Image1);
        command.Parameters.AddWithValue("$Image2", product.Image2);
        command.Parameters.AddWithValue("$Image3", product.Image3);
        command.Parameters.AddWithValue("$Image4", product.Image4);
        command.Parameters.AddWithValue("$ShopeeImage1Url", product.ShopeeImage1Url);
        command.Parameters.AddWithValue("$ShopeeImage2Url", product.ShopeeImage2Url);
        command.Parameters.AddWithValue("$ShopeeImage3Url", product.ShopeeImage3Url);
        command.Parameters.AddWithValue("$ShopeeImage4Url", product.ShopeeImage4Url);
        command.Parameters.AddWithValue("$LazadaImage1Url", product.LazadaImage1Url);
        command.Parameters.AddWithValue("$LazadaImage2Url", product.LazadaImage2Url);
        command.Parameters.AddWithValue("$LazadaImage3Url", product.LazadaImage3Url);
        command.Parameters.AddWithValue("$LazadaImage4Url", product.LazadaImage4Url);
        command.Parameters.AddWithValue("$VariationName", product.VariationName);
        command.Parameters.AddWithValue("$VariationOption", product.VariationOption);
        command.Parameters.AddWithValue("$VariationPrice", product.VariationPrice.HasValue ? product.VariationPrice.Value : DBNull.Value);
        command.Parameters.AddWithValue("$VariationStock", product.VariationStock.HasValue ? product.VariationStock.Value : DBNull.Value);
        command.Parameters.AddWithValue("$Brand", product.Brand);
        command.Parameters.AddWithValue("$BabyMaterial", product.BabyMaterial);
        command.Parameters.AddWithValue("$CountryOfOrigin", product.CountryOfOrigin);
        command.Parameters.AddWithValue("$WarrantyType", product.WarrantyType);
        command.Parameters.AddWithValue("$WarrantyPeriod", product.WarrantyPeriod);
        command.Parameters.AddWithValue("$ColorFamily", product.ColorFamily);
        command.Parameters.AddWithValue("$DangerousGoods", product.DangerousGoods);
        command.Parameters.AddWithValue("$DeliveryStandard", product.DeliveryStandard);
        command.Parameters.AddWithValue("$CreatedAt", product.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$UpdatedAt", product.UpdatedAt.ToString("O"));
    }

    private static async Task EnsureColumnAsync(SqliteConnection connection, string tableName, string columnName, string columnDefinition, CancellationToken cancellationToken)
    {
        var pragma = connection.CreateCommand();
        pragma.CommandText = $"PRAGMA table_info({tableName});";
        await using var reader = await pragma.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        await reader.CloseAsync();

        var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};";
        await alter.ExecuteNonQueryAsync(cancellationToken);
    }
}
