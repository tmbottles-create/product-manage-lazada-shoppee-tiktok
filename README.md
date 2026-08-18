# Shopee Lazada Product Manager

Windows app for managing a local product catalog and generating Excel files for Shopee and Lazada import workflows.

## Database Choice

This app uses `SQLite`.

Why SQLite:
- free and open source
- no separate server to install
- data is stored in a single file
- easy backup, copy, and restore
- perfect for a desktop CRUD app

The database file is created automatically in:

`src/ShopeeSellerUploader.App/bin/Debug/net8.0-windows/Data/product-catalog.db`

## Solution Structure

- `src/ShopeeSellerUploader.App`
  WinForms UI for product list, add/edit/delete, select products, and export buttons
- `src/ShopeeSellerUploader.Contracts`
  shared interfaces and configuration contracts
- `src/ShopeeSellerUploader.Core`
  product models and validation
- `src/ShopeeSellerUploader.Infrastructure`
  SQLite repository, Excel export service, and supporting infrastructure

## Template Directory

The app looks for templates in:

`D:\shoppee-lazada-templete`

Expected files:
- `ShopeeTemplate.xlsx`
- `LazadaTemplate.xlsx`

If a template file is missing, the app will still generate a workbook using default headers.

## Features

- local product database
- product list view
- add product
- edit product
- delete product
- select multiple products
- export selected products to Shopee Excel
- export selected products to Lazada Excel

## Run

```powershell
$env:APPDATA = (Join-Path (Get-Location) '.appdata')
$env:LOCALAPPDATA = (Join-Path (Get-Location) '.localappdata')
$env:DOTNET_CLI_HOME = (Get-Location).Path
dotnet restore .\ShopeeSellerUploader.sln --configfile .\NuGet.Config
dotnet run --project .\src\ShopeeSellerUploader.App\ShopeeSellerUploader.App.csproj
```

## Notes

- export files are written by default under the app `Data\exports` folder
- you can change the template root path in `src/ShopeeSellerUploader.App/appsettings.json`
- the current app is designed for product data management and Excel generation, not browser automation
