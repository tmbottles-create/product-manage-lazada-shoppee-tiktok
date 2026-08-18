using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Core.Utilities;

namespace ShopeeSellerUploader.App.Forms;

public partial class CategoryMappingForm : Form
{
    private readonly BindingSource _bindingSource = new();
    private readonly IReadOnlyList<string> _lazadaSheetNames;

    public CategoryMappingForm(
        IReadOnlyList<string> productCategories,
        IReadOnlyList<string> lazadaSheetNames,
        IReadOnlyList<string> tikTokCategoryNames,
        IReadOnlyDictionary<string, CategoryMapping> currentMappings)
    {
        InitializeComponent();
        _lazadaSheetNames = lazadaSheetNames;

        var rows = productCategories.Select(category => new MappingRow
        {
            ProductCategory = category,
            LazadaSheetName = currentMappings.TryGetValue(category, out var mapped) ? mapped.LazadaSheetName : lazadaSheetNames.FirstOrDefault() ?? string.Empty,
            ShopeeCategoryCode = currentMappings.TryGetValue(category, out mapped) ? mapped.ShopeeCategoryCode : string.Empty,
            TikTokCategoryName = currentMappings.TryGetValue(category, out mapped) ? mapped.TikTokCategoryName : string.Empty
        }).ToList();

        _bindingSource.DataSource = rows;
        dgvMappings.AutoGenerateColumns = false;
        dgvMappings.DataSource = _bindingSource;

        if (dgvMappings.Columns["LazadaSheetColumn"] is DataGridViewComboBoxColumn comboColumn)
        {
            comboColumn.DataSource = lazadaSheetNames.ToList();
        }

        if (dgvMappings.Columns["TikTokCategoryColumn"] is DataGridViewComboBoxColumn tikTokColumn)
        {
            tikTokColumn.DataSource = tikTokCategoryNames.ToList();
        }
    }

    public IReadOnlyList<CategoryMapping> GetMappings()
    {
        return _bindingSource.List.Cast<MappingRow>()
            .Where(static row => !string.IsNullOrWhiteSpace(row.ProductCategory) && !string.IsNullOrWhiteSpace(row.LazadaSheetName))
            .Select(row => new CategoryMapping
            {
                ProductCategory = row.ProductCategory,
                LazadaSheetName = row.LazadaSheetName,
                ShopeeCategoryCode = ShopeeCategoryCodeParser.Normalize(row.ShopeeCategoryCode),
                TikTokCategoryName = row.TikTokCategoryName?.Trim() ?? string.Empty
            })
            .ToList();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void btnImportLazadaSheets_Click(object? sender, EventArgs e)
    {
        var rows = _bindingSource.List.Cast<MappingRow>().ToList();
        var existingCategories = rows
            .Select(static row => row.ProductCategory)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var addedCount = 0;
        foreach (var sheetName in _lazadaSheetNames)
        {
            if (string.IsNullOrWhiteSpace(sheetName) || existingCategories.Contains(sheetName))
            {
                continue;
            }

            rows.Add(new MappingRow
            {
                ProductCategory = sheetName,
                LazadaSheetName = sheetName,
                ShopeeCategoryCode = string.Empty,
                TikTokCategoryName = string.Empty
            });
            existingCategories.Add(sheetName);
            addedCount++;
        }

        if (addedCount == 0)
        {
            MessageBox.Show(
                this,
                "No new Lazada sheets were found to import.",
                "Import Lazada Sheets",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        _bindingSource.DataSource = rows
            .OrderBy(static row => row.ProductCategory, StringComparer.OrdinalIgnoreCase)
            .ToList();
        dgvMappings.DataSource = _bindingSource;

        MessageBox.Show(
            this,
            $"Imported {addedCount} Lazada sheet(s) into master category mapping.",
            "Import Lazada Sheets",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private sealed class MappingRow
    {
        public string ProductCategory { get; set; } = string.Empty;
        public string LazadaSheetName { get; set; } = string.Empty;
        public string ShopeeCategoryCode { get; set; } = string.Empty;
        public string TikTokCategoryName { get; set; } = string.Empty;
    }
}
