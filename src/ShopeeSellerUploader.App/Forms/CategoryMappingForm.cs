using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Core.Utilities;
using ShopeeSellerUploader.Contracts.Interfaces;

namespace ShopeeSellerUploader.App.Forms;

public partial class CategoryMappingForm : Form
{
    private readonly BindingSource _bindingSource = new();
    private readonly List<string> _lazadaSheetNames;
    private readonly List<string> _shopeeCategoryCodes;
    private readonly List<string> _tikTokCategoryNames;

    public CategoryMappingForm(
        IReadOnlyList<string> productCategories,
        IReadOnlyList<string> lazadaSheetNames,
        IReadOnlyList<string> shopeeCategoryCodes,
        IReadOnlyList<string> tikTokCategoryNames,
        IReadOnlyDictionary<string, CategoryMapping> currentMappings)
    {
        InitializeComponent();
        _lazadaSheetNames = lazadaSheetNames
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
        _shopeeCategoryCodes = shopeeCategoryCodes
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
        _tikTokCategoryNames = tikTokCategoryNames
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rows = productCategories.Select(category => new MappingRow
        {
            ProductCategory = category,
            LazadaSheetName = currentMappings.TryGetValue(category, out var mapped) ? mapped.LazadaSheetName : _lazadaSheetNames.FirstOrDefault() ?? string.Empty,
            ShopeeCategoryCode = currentMappings.TryGetValue(category, out mapped) ? mapped.ShopeeCategoryCode : string.Empty,
            TikTokCategoryName = currentMappings.TryGetValue(category, out mapped) ? mapped.TikTokCategoryName : string.Empty
        }).ToList();

        _bindingSource.DataSource = rows;
        dgvMappings.AutoGenerateColumns = false;
        dgvMappings.DataSource = _bindingSource;

        if (dgvMappings.Columns["LazadaSheetColumn"] is DataGridViewComboBoxColumn comboColumn)
        {
            comboColumn.DataSource = _lazadaSheetNames.ToList();
        }

        if (dgvMappings.Columns["TikTokCategoryColumn"] is DataGridViewComboBoxColumn tikTokColumn)
        {
            tikTokColumn.DataSource = _tikTokCategoryNames.ToList();
        }

        dgvMappings.EditingControlShowing += dgvMappings_EditingControlShowing;
        dgvMappings.CellValidating += dgvMappings_CellValidating;
    }

    public IReadOnlyList<CategoryMapping> GetMappings()
    {
        return dgvMappings.Rows.Cast<DataGridViewRow>()
            .Where(static row => !row.IsNewRow)
            .Select(static row => new
            {
                ProductCategory = row.Cells[0].Value?.ToString()?.Trim() ?? string.Empty,
                LazadaSheetName = row.Cells[1].Value?.ToString()?.Trim() ?? string.Empty,
                ShopeeCategoryCode = row.Cells[2].Value?.ToString() ?? string.Empty,
                TikTokCategoryName = row.Cells[3].Value?.ToString()?.Trim() ?? string.Empty
            })
            .Where(static row => !string.IsNullOrWhiteSpace(row.ProductCategory) && !string.IsNullOrWhiteSpace(row.LazadaSheetName))
            .Select(row => new CategoryMapping
            {
                ProductCategory = row.ProductCategory,
                LazadaSheetName = row.LazadaSheetName,
                ShopeeCategoryCode = ShopeeCategoryCodeParser.Normalize(row.ShopeeCategoryCode),
                TikTokCategoryName = row.TikTokCategoryName
            })
            .ToList();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        ValidateChildren();
        dgvMappings.EndEdit();
        _bindingSource.EndEdit();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void btnAdd_Click(object? sender, EventArgs e)
    {
        var rows = _bindingSource.List.Cast<MappingRow>().ToList();
        rows.Add(new MappingRow
        {
            ProductCategory = string.Empty,
            LazadaSheetName = string.Empty,
            ShopeeCategoryCode = string.Empty,
            TikTokCategoryName = string.Empty
        });

        _bindingSource.DataSource = rows;
        dgvMappings.DataSource = _bindingSource;

        var newRowIndex = rows.Count - 1;
        if (newRowIndex < 0)
        {
            return;
        }

        dgvMappings.ClearSelection();
        dgvMappings.CurrentCell = dgvMappings.Rows[newRowIndex].Cells[0];
        dgvMappings.BeginEdit(true);
    }

    private void dgvMappings_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
    {
        if (dgvMappings.CurrentCell is null)
        {
            return;
        }

        var columnName = dgvMappings.Columns[dgvMappings.CurrentCell.ColumnIndex].Name;
        if ((columnName == "LazadaSheetColumn" || columnName == "TikTokCategoryColumn") &&
            e.Control is ComboBox comboBox)
        {
            comboBox.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
        }
        else if (columnName == "ShopeeCategoryColumn" && e.Control is TextBox textBox)
        {
            var autoCompleteValues = new AutoCompleteStringCollection();
            autoCompleteValues.AddRange(_shopeeCategoryCodes.ToArray());
            textBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBox.AutoCompleteCustomSource = autoCompleteValues;
        }
    }

    private void dgvMappings_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        var columnName = dgvMappings.Columns[e.ColumnIndex].Name;
        var enteredValue = e.FormattedValue?.ToString()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(enteredValue))
        {
            return;
        }

        if (columnName == "LazadaSheetColumn")
        {
            EnsureComboValueExists(_lazadaSheetNames, enteredValue, "LazadaSheetColumn");
        }
        else if (columnName == "ShopeeCategoryColumn")
        {
            EnsureTextValueExists(_shopeeCategoryCodes, enteredValue);
        }
        else if (columnName == "TikTokCategoryColumn")
        {
            EnsureComboValueExists(_tikTokCategoryNames, enteredValue, "TikTokCategoryColumn");
        }
    }

    private void EnsureComboValueExists(List<string> values, string enteredValue, string columnName)
    {
        if (values.Contains(enteredValue, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        values.Add(enteredValue);
        values.Sort(StringComparer.OrdinalIgnoreCase);

        if (dgvMappings.Columns[columnName] is DataGridViewComboBoxColumn comboColumn)
        {
            comboColumn.DataSource = null;
            comboColumn.DataSource = values.ToList();
        }
    }

    private static void EnsureTextValueExists(List<string> values, string enteredValue)
    {
        if (values.Contains(enteredValue, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        values.Add(enteredValue);
        values.Sort(StringComparer.OrdinalIgnoreCase);
    }

    private void RefreshComboDataSources()
    {
        if (dgvMappings.Columns["LazadaSheetColumn"] is DataGridViewComboBoxColumn lazadaColumn)
        {
            lazadaColumn.DataSource = null;
            lazadaColumn.DataSource = _lazadaSheetNames.ToList();
        }

        if (dgvMappings.Columns["TikTokCategoryColumn"] is DataGridViewComboBoxColumn tikTokColumn)
        {
            tikTokColumn.DataSource = null;
            tikTokColumn.DataSource = _tikTokCategoryNames.ToList();
        }
    }

    private sealed class MappingRow
    {
        public string ProductCategory { get; set; } = string.Empty;
        public string LazadaSheetName { get; set; } = string.Empty;
        public string ShopeeCategoryCode { get; set; } = string.Empty;
        public string TikTokCategoryName { get; set; } = string.Empty;
    }
}
