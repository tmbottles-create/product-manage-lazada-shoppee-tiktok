using ShopeeSellerUploader.Contracts.Interfaces;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Core.Validation;
using System.Globalization;
using Serilog;

namespace ShopeeSellerUploader.App.Forms;

public partial class ProductEditForm : Form
{
    private sealed record VariationEditorEntry(string Option, decimal? Price, int? Stock, string Image);

    private static readonly string[] BrandOptions = ["No Brand", "OEM", "Generic", "Unbranded"];
    private static readonly string[] BabyMaterialOptions =
    [
        "ไม้",
        "พลาสติก",
        "ซิลิโคน",
        "เซรามิก",
        "กระจก",
        "สแตนเลส",
        "โลหะ",
        "ผ้า",
        "ยาง",
        "อื่นๆ"
    ];
    private static readonly string[] CountryOfOriginOptions =
    [
        "จีน",
        "ไทย",
        "สหรัฐอเมริกา",
        "ญี่ปุ่น",
        "เกาหลีใต้",
        "ไต้หวัน",
        "เวียดนาม",
        "มาเลเซีย"
    ];
    private static readonly string[] WarrantyTypeOptions =
    [
        "ไม่มีการรับประกัน",
        "การรับประกันจากซัพพลายเออร์ในพื้นที่",
        "การรับประกันจากผู้ผลิตระดับสากล",
        "การรับประกันโดยผู้ขาย",
        "การรับประกันโดยผู้ให้บริการ",
        "มีการรับประกัน",
        "การรับประกันผู้ขายระหว่างประเทศ",
        "การรับประกันจากผู้ผลิตในพื้นที่",
        "การรับประกันการซ่อมแซมโดยผู้ขาย"
    ];
    private static readonly string[] ColorFamilyOptions =
    [
        "คละสี",
        "ดำ",
        "ขาว",
        "แดง",
        "น้ำเงิน",
        "เขียว",
        "เหลือง",
        "ชมพู",
        "ม่วง",
        "ส้ม",
        "เทา",
        "น้ำตาล",
        "ทอง",
        "เงิน",
        "ใส"
    ];
    private readonly IAiProductSuggestionService _aiProductSuggestionService;
    private readonly IProductRepository _productRepository;
    private readonly ILazadaImageUploadService _lazadaImageUploadService;
    private readonly ILogger _logger;
    private readonly List<string> _categories;
    private CancellationTokenSource? _aiFillCancellationTokenSource;
    private bool _isSyncingVariationEditor;
    private bool _isUploadingLazadaImages;

    public ProductItem Product { get; private set; }

    public ProductEditForm(
        IReadOnlyList<string> categories,
        IAiProductSuggestionService aiProductSuggestionService,
        IProductRepository productRepository,
        ILazadaImageUploadService lazadaImageUploadService,
        ILogger logger)
        : this(
            new ProductItem
            {
                Price = 1,
                Weight = 1,
                Length = 30,
                Width = 15,
                Height = 20,
                CountryOfOrigin = CountryOfOriginOptions[0]
            },
            categories,
            aiProductSuggestionService,
            productRepository,
            lazadaImageUploadService,
            logger)
    {
    }

    public ProductEditForm(
        ProductItem product,
        IReadOnlyList<string> categories,
        IAiProductSuggestionService aiProductSuggestionService,
        IProductRepository productRepository,
        ILazadaImageUploadService lazadaImageUploadService,
        ILogger logger)
    {
        Product = Clone(product);
        _categories = categories
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value)
            .ToList();
        _aiProductSuggestionService = aiProductSuggestionService;
        _productRepository = productRepository;
        _lazadaImageUploadService = lazadaImageUploadService;
        _logger = logger;
        InitializeComponent();
        ConfigureLazadaValueCombos();
        WireVariationEditor();
        BindCategories(Product.Category);
        BindProduct();
    }

    private void BindCategories(string selectedCategory)
    {
        if (!string.IsNullOrWhiteSpace(selectedCategory) &&
            !_categories.Contains(selectedCategory, StringComparer.OrdinalIgnoreCase))
        {
            _categories.Add(selectedCategory.Trim());
            _categories.Sort(StringComparer.OrdinalIgnoreCase);
        }

        cboCategory.Items.Clear();
        cboCategory.Items.AddRange(_categories.Cast<object>().ToArray());

        if (!string.IsNullOrWhiteSpace(selectedCategory))
        {
            cboCategory.SelectedItem = cboCategory.Items
                .Cast<object>()
                .FirstOrDefault(item => string.Equals(item?.ToString(), selectedCategory, StringComparison.OrdinalIgnoreCase));
        }

        if (cboCategory.SelectedIndex < 0 && cboCategory.Items.Count > 0)
        {
            cboCategory.SelectedIndex = 0;
        }
    }

    private void BindProduct()
    {
        txtProductCode.Text = Product.ProductCode;
        txtProductName.Text = Product.ProductName;
        txtDescription.Text = Product.Description;
        cboCategory.Text = Product.Category;
        numPrice.Value = Product.Price > 0 ? Product.Price : 1;
        numWeight.Value = Product.Weight > 0 ? Product.Weight : 1;
        numLength.Value = Product.Length;
        numWidth.Value = Product.Width;
        numHeight.Value = Product.Height;
        txtSku.Text = Product.SKU;
        txtImage1.Text = Product.Image1;
        txtImage2.Text = Product.Image2;
        txtImage3.Text = Product.Image3;
        txtImage4.Text = Product.Image4;
        RememberLocalImagePath(txtImage1, Product.Image1);
        RememberLocalImagePath(txtImage2, Product.Image2);
        RememberLocalImagePath(txtImage3, Product.Image3);
        RememberLocalImagePath(txtImage4, Product.Image4);
        Product.SynchronizeMarketplaceImageUrls();
        txtVariationName.Text = Product.VariationName;
        txtVariationOption.Text = Product.VariationOption;
        SyncVariationGridFromText();
        numVariationPrice.Value = Product.VariationPrice ?? 0;
        numVariationStock.Value = Product.VariationStock ?? 0;
        SetComboValue(cboBrand, Product.Brand, "No Brand");
        SetComboValue(cboBabyMaterial, Product.BabyMaterial);
        SetComboValue(cboCountryOfOrigin, Product.CountryOfOrigin);
        SetComboValue(cboWarrantyType, Product.WarrantyType, "ไม่มีการรับประกัน");
        txtWarrantyPeriod.Text = Product.WarrantyPeriod;
        SetComboValue(cboColorFamily, Product.ColorFamily, "คละสี");
        SetComboValue(cboDangerousGoods, Product.DangerousGoods, "No");
        SetComboValue(cboDeliveryStandard, Product.DeliveryStandard, "Yes");
        txtAiNotes.Text = "AI can help create a draft from product images. For multi-color items, use one line per option: สี | ราคา | สต็อก | รูป";
    }

    private async void btnAiFill_Click(object sender, EventArgs e)
    {
        if (_aiFillCancellationTokenSource is not null)
        {
            _aiFillCancellationTokenSource.Cancel();
            return;
        }

        using var dialog = new AiFillDialog(
            new[]
            {
                GetSelectedLocalImagePath(txtImage1),
                GetSelectedLocalImagePath(txtImage2),
                GetSelectedLocalImagePath(txtImage3),
                GetSelectedLocalImagePath(txtImage4)
            },
            txtAiHint.Text.Trim());

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        ApplyAiDialogSelection(dialog);
        var imagePaths = dialog.SelectedImagePaths.ToList();

        _aiFillCancellationTokenSource = new CancellationTokenSource();
        SetAiBusyState(true);
        txtAiNotes.Text = "AI is analyzing the selected image. Click 'Cancel AI' if it takes too long.";
        await Task.Yield();

        try
        {
            var suggestion = await _aiProductSuggestionService.SuggestAsync(new AiProductSuggestionRequest
            {
                ImagePaths = imagePaths,
                UserHint = dialog.AdditionalDetails,
                ExistingCategory = cboCategory.Text.Trim(),
                ExistingBrand = cboBrand.Text.Trim()
            }, _aiFillCancellationTokenSource.Token);

            ApplySuggestion(suggestion);
            txtAiNotes.Text = string.IsNullOrWhiteSpace(suggestion.Notes)
                ? "AI filled a draft successfully. Please review Product Name, Description, Category, Price, Weight, and SKU before saving."
                : suggestion.Notes;
        }
        catch (OperationCanceledException)
        {
            txtAiNotes.Text = "AI request was canceled.";
        }
        catch (Exception ex)
        {
            txtAiNotes.Text = ex.Message;
            MessageBox.Show(this, ex.Message, "AI Fill Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _aiFillCancellationTokenSource?.Dispose();
            _aiFillCancellationTokenSource = null;
            SetAiBusyState(false);
        }
    }

    private void ApplyAiDialogSelection(AiFillDialog dialog)
    {
        var imagePaths = dialog.SelectedImagePaths.ToArray();
        ApplySelectedLocalImage(txtImage1, imagePaths.ElementAtOrDefault(0));
        txtAiHint.Text = dialog.AdditionalDetails;
    }

    private static void ApplySelectedLocalImage(TextBox target, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        target.Text = path.Trim();
        RememberLocalImagePath(target, path);
    }

    private void ApplySuggestion(AiProductSuggestion suggestion)
    {
        txtProductCode.Text = PickText(suggestion.ProductCode, txtProductCode.Text);
        txtProductName.Text = PickText(suggestion.ProductName, txtProductName.Text);
        txtDescription.Text = PickText(suggestion.Description, txtDescription.Text);
        var suggestedCategory = PickText(suggestion.Category, cboCategory.Text);
        if (!string.Equals(suggestedCategory, cboCategory.Text, StringComparison.Ordinal))
        {
            BindCategories(suggestedCategory);
        }
        txtSku.Text = PickText(suggestion.SKU, txtSku.Text);
        txtVariationName.Text = PickText(suggestion.VariationName, txtVariationName.Text);
        txtVariationOption.Text = NormalizeVariationInput(PickText(suggestion.VariationOption, txtVariationOption.Text));
        SyncVariationGridFromText();
        SetComboValue(cboBrand, PickText(suggestion.Brand, cboBrand.Text), "No Brand");
        SetComboValue(cboBabyMaterial, PickText(suggestion.BabyMaterial, cboBabyMaterial.Text));
        SetComboValue(cboWarrantyType, PickText(suggestion.WarrantyType, cboWarrantyType.Text), "ไม่มีการรับประกัน");
        txtWarrantyPeriod.Text = PickText(suggestion.WarrantyPeriod, txtWarrantyPeriod.Text);
        SetComboValue(cboColorFamily, PickText(suggestion.ColorFamily, cboColorFamily.Text), "คละสี");

        if (suggestion.Price is > 0)
        {
            numPrice.Value = ClampNumeric(numPrice, suggestion.Price.Value);
        }

        if (suggestion.Weight is > 0)
        {
            numWeight.Value = ClampNumeric(numWeight, suggestion.Weight.Value);
        }

        if (suggestion.Length is > 0)
        {
            numLength.Value = ClampNumeric(numLength, suggestion.Length.Value);
        }

        if (suggestion.Width is > 0)
        {
            numWidth.Value = ClampNumeric(numWidth, suggestion.Width.Value);
        }

        if (suggestion.Height is > 0)
        {
            numHeight.Value = ClampNumeric(numHeight, suggestion.Height.Value);
        }

        if (suggestion.VariationPrice is > 0)
        {
            numVariationPrice.Value = ClampNumeric(numVariationPrice, suggestion.VariationPrice.Value);
        }

        if (suggestion.VariationStock is >= 0)
        {
            numVariationStock.Value = ClampNumeric(numVariationStock, suggestion.VariationStock.Value);
        }

        if (!string.IsNullOrWhiteSpace(suggestion.DangerousGoods))
        {
            SetComboValue(cboDangerousGoods, suggestion.DangerousGoods, "No");
        }

        if (!string.IsNullOrWhiteSpace(suggestion.DeliveryStandard))
        {
            SetComboValue(cboDeliveryStandard, suggestion.DeliveryStandard, "Yes");
        }
    }

    private void ConfigureLazadaValueCombos()
    {
        LoadComboOptions(cboBrand, BrandOptions);
        LoadComboOptions(cboBabyMaterial, BabyMaterialOptions);
        LoadComboOptions(cboCountryOfOrigin, CountryOfOriginOptions);
        LoadComboOptions(cboWarrantyType, WarrantyTypeOptions);
        LoadComboOptions(cboColorFamily, ColorFamilyOptions);
    }

    private static void LoadComboOptions(ComboBox comboBox, IEnumerable<string> values)
    {
        comboBox.Items.Clear();
        comboBox.Items.AddRange(values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<object>()
            .ToArray());
        comboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
    }

    private static void SetComboValue(ComboBox comboBox, string? value, string fallback = "")
    {
        var finalValue = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        if (!string.IsNullOrWhiteSpace(finalValue) &&
            !comboBox.Items.Cast<object>().Any(item => string.Equals(item?.ToString(), finalValue, StringComparison.OrdinalIgnoreCase)))
        {
            comboBox.Items.Add(finalValue);
        }

        comboBox.Text = finalValue;
    }

    private static string PickText(string suggested, string current)
    {
        return string.IsNullOrWhiteSpace(suggested) ? current : suggested.Trim();
    }

    private static string NormalizeVariationInput(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (value.Contains('|') || value.Contains('\n') || value.Contains('\r'))
        {
            var normalizedLines = value
                .Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(static line => string.Join(" | ", line
                    .Split('|', StringSplitOptions.TrimEntries)
                    .Where(static part => !string.IsNullOrWhiteSpace(part))))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return string.Join(Environment.NewLine, normalizedLines);
        }

        return string.Join(", ", value
            .Split([',', ';', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private void WireVariationEditor()
    {
        txtVariationOption.Leave += (_, _) => SyncVariationGridFromText();
        dgvVariationOptions.CellValueChanged += (_, _) => SyncVariationTextFromGrid();
        dgvVariationOptions.RowsRemoved += (_, _) => SyncVariationTextFromGrid();
        dgvVariationOptions.UserDeletedRow += (_, _) => SyncVariationTextFromGrid();
        dgvVariationOptions.CellContentClick += dgvVariationOptions_CellContentClick;
        dgvVariationOptions.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (dgvVariationOptions.IsCurrentCellDirty)
            {
                dgvVariationOptions.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
    }

    private void btnAddVariantRow_Click(object? sender, EventArgs e)
    {
        dgvVariationOptions.Rows.Add(string.Empty, string.Empty, string.Empty, string.Empty);
        var rowIndex = dgvVariationOptions.Rows.Count - 1;
        if (rowIndex >= 0)
        {
            dgvVariationOptions.CurrentCell = dgvVariationOptions.Rows[rowIndex].Cells[0];
            dgvVariationOptions.BeginEdit(true);
        }

        SyncVariationTextFromGrid();
    }

    private void btnRemoveVariantRow_Click(object? sender, EventArgs e)
    {
        if (dgvVariationOptions.SelectedRows.Count == 0)
        {
            return;
        }

        foreach (DataGridViewRow row in dgvVariationOptions.SelectedRows)
        {
            if (!row.IsNewRow)
            {
                dgvVariationOptions.Rows.Remove(row);
            }
        }

        SyncVariationTextFromGrid();
    }

    private void btnUpdateAllVariantPrice_Click(object? sender, EventArgs e)
    {
        ApplyBulkVariantValue(
            columnIndex: 1,
            formattedValue: numVariationPrice.Value > 0
                ? numVariationPrice.Value.ToString("0.##", CultureInfo.InvariantCulture)
                : string.Empty);
    }

    private void btnUpdateAllVariantStock_Click(object? sender, EventArgs e)
    {
        ApplyBulkVariantValue(
            columnIndex: 2,
            formattedValue: numVariationStock.Value > 0
                ? numVariationStock.Value.ToString(CultureInfo.InvariantCulture)
                : string.Empty);
    }

    private void ApplyBulkVariantValue(int columnIndex, string formattedValue)
    {
        foreach (DataGridViewRow row in dgvVariationOptions.Rows)
        {
            if (row.IsNewRow)
            {
                continue;
            }

            var option = row.Cells[0].Value?.ToString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(option))
            {
                continue;
            }

            row.Cells[columnIndex].Value = formattedValue;
        }

        SyncVariationTextFromGrid();
    }

    private void SyncVariationGridFromText()
    {
        if (_isSyncingVariationEditor)
        {
            return;
        }

        _isSyncingVariationEditor = true;
        try
        {
            var entries = ParseVariationEntries(txtVariationOption.Text);
            dgvVariationOptions.Rows.Clear();
            foreach (var entry in entries)
            {
                dgvVariationOptions.Rows.Add(
                    entry.Option,
                    entry.Price?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty,
                    entry.Stock?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                    entry.Image);
            }
        }
        finally
        {
            _isSyncingVariationEditor = false;
        }
    }

    private void SyncVariationTextFromGrid()
    {
        if (_isSyncingVariationEditor)
        {
            return;
        }

        _isSyncingVariationEditor = true;
        try
        {
            var entries = ReadVariationEntriesFromGrid();
            txtVariationOption.Text = entries.Count == 0
                ? string.Empty
                : string.Join(Environment.NewLine, entries.Select(FormatVariationEntry));
        }
        finally
        {
            _isSyncingVariationEditor = false;
        }
    }

    private static List<VariationEditorEntry> ParseVariationEntries(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var lines = value
            .Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (lines.Count > 1 || value.Contains('|'))
        {
            return lines
                .Select(ParseVariationEntryLine)
                .Where(static entry => !string.IsNullOrWhiteSpace(entry.Option))
                .DistinctBy(static entry => entry.Option, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return value
            .Split([',', ';', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(static item => new VariationEditorEntry(item, null, null, string.Empty))
            .ToList();
    }

    private static VariationEditorEntry ParseVariationEntryLine(string line)
    {
        var parts = line.Split('|', StringSplitOptions.TrimEntries);
        var option = parts.ElementAtOrDefault(0)?.Trim() ?? string.Empty;
        var price = TryParseDecimal(parts.ElementAtOrDefault(1));
        var stock = TryParseInt(parts.ElementAtOrDefault(2));
        var image = parts.ElementAtOrDefault(3)?.Trim() ?? string.Empty;
        return new VariationEditorEntry(option, price, stock, image);
    }

    private List<VariationEditorEntry> ReadVariationEntriesFromGrid()
    {
        var entries = new List<VariationEditorEntry>();
        foreach (DataGridViewRow row in dgvVariationOptions.Rows)
        {
            if (row.IsNewRow)
            {
                continue;
            }

            var option = (row.Cells[0].Value?.ToString() ?? string.Empty).Trim();
            var priceText = (row.Cells[1].Value?.ToString() ?? string.Empty).Trim();
            var stockText = (row.Cells[2].Value?.ToString() ?? string.Empty).Trim();
            var image = (row.Cells[3].Value?.ToString() ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(option) &&
                string.IsNullOrWhiteSpace(priceText) &&
                string.IsNullOrWhiteSpace(stockText) &&
                string.IsNullOrWhiteSpace(image))
            {
                continue;
            }

            entries.Add(new VariationEditorEntry(option, TryParseDecimal(priceText), TryParseInt(stockText), image));
        }

        return entries
            .Where(static entry => !string.IsNullOrWhiteSpace(entry.Option))
            .DistinctBy(static entry => entry.Option, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async void dgvVariationOptions_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        if (!string.Equals(dgvVariationOptions.Columns[e.ColumnIndex].Name, "UploadVariantImage", StringComparison.Ordinal))
        {
            return;
        }

        var row = dgvVariationOptions.Rows[e.RowIndex];
        await UploadVariantImageForRowAsync(row);
    }

    private async void btnUploadVariantImage_Click(object? sender, EventArgs e)
    {
        var row = dgvVariationOptions.SelectedRows.Count > 0
            ? dgvVariationOptions.SelectedRows[0]
            : dgvVariationOptions.CurrentRow;

        if (row is null || row.IsNewRow)
        {
            MessageBox.Show(this, "Please select a variant row before uploading an image.", "Variant Image Upload", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        await UploadVariantImageForRowAsync(row);
    }

    private async Task UploadVariantImageForRowAsync(DataGridViewRow row)
    {
        var option = (row.Cells[0].Value?.ToString() ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(option))
        {
            MessageBox.Show(this, "Please enter Variant Option before uploading an image.", "Variant Image Upload", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedPath = SelectVariationImageFile();
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        try
        {
            _isUploadingLazadaImages = true;
            SetImageUploadButtonsEnabled(false);
            btnSave.Enabled = false;
            btnUploadVariantImage.Enabled = false;
            dgvVariationOptions.Enabled = false;
            UseWaitCursor = true;
            txtAiNotes.Text = $"Uploading variant image for '{option}'...";
            await Task.Yield();

            var productCode = string.IsNullOrWhiteSpace(txtProductCode.Text) ? "variant" : txtProductCode.Text.Trim();
            var uploadedUrl = await _lazadaImageUploadService.UploadExternalImageAsync($"{productCode}-{option}", selectedPath);
            row.Cells[3].Value = uploadedUrl;
            txtAiNotes.Text = $"Uploaded variant image for '{option}' successfully.";
            SyncVariationTextFromGrid();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to upload variant image.");
            txtAiNotes.Text = ex.Message;
            MessageBox.Show(this, ex.Message, "Variant Image Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _isUploadingLazadaImages = false;
            SetImageUploadButtonsEnabled(true);
            btnSave.Enabled = true;
            btnUploadVariantImage.Enabled = true;
            dgvVariationOptions.Enabled = true;
            UseWaitCursor = false;
        }
    }

    private string SelectVariationImageFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp;*.gif",
            FilterIndex = 1,
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            DereferenceLinks = true,
            RestoreDirectory = true,
            AutoUpgradeEnabled = false,
            AddToRecent = false,
            ValidateNames = true,
            Title = "Select Variant Image"
        };

        ApplyDefaultImageDirectory(dialog);

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return string.Empty;
        }

        ImageBrowseDirectoryState.Remember(dialog.FileName);
        return dialog.FileName;
    }

    private static string FormatVariationEntry(VariationEditorEntry entry)
    {
        return string.Join(" | ", new[]
        {
            entry.Option.Trim(),
            entry.Price?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty,
            entry.Stock?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            entry.Image.Trim()
        });
    }

    private static decimal? TryParseDecimal(string? value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ||
               decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out parsed)
            ? parsed
            : null;
    }

    private static int? TryParseInt(string? value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ||
               int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out parsed)
            ? parsed
            : null;
    }

    private static decimal ClampNumeric(NumericUpDown control, decimal value)
    {
        if (value < control.Minimum)
        {
            return control.Minimum;
        }

        if (value > control.Maximum)
        {
            return control.Maximum;
        }

        return value;
    }

    private void SetAiBusyState(bool busy)
    {
        btnAiFill.Enabled = true;
        btnAiFill.Text = busy ? "Cancel AI" : "AI Fill";
        btnSave.Enabled = !busy;
        UseWaitCursor = busy;
    }

    private string SelectImageFile(TextBox target)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp;*.gif",
            FilterIndex = 1,
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            DereferenceLinks = true,
            RestoreDirectory = true,
            AutoUpgradeEnabled = false,
            AddToRecent = false,
            ValidateNames = true,
            Title = "Select Product Image"
        };

        var currentPath = GetSelectedLocalImagePath(target);
        if (File.Exists(currentPath))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(currentPath);
            dialog.FileName = Path.GetFileName(currentPath);
        }
        else
        {
            ApplyDefaultImageDirectory(dialog);
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return string.Empty;
        }

        ImageBrowseDirectoryState.Remember(dialog.FileName);
        return dialog.FileName;
    }

    private static void ApplyDefaultImageDirectory(OpenFileDialog dialog)
    {
        ImageBrowseDirectoryState.ApplyDefaultDirectory(dialog);
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (!TryApplyFormToProduct())
        {
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        if (_aiFillCancellationTokenSource is not null)
        {
            _aiFillCancellationTokenSource.Cancel();
        }

        DialogResult = DialogResult.Cancel;
        Close();
    }

    private static string NormalizeOptionalUrl(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        return IsPlaceholderUrl(trimmed) ? string.Empty : trimmed;
    }

    private static bool IsPlaceholderUrl(string value)
    {
        return string.Equals(value, "https://...", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "http://...", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "https://...jpg", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "http://...jpg", StringComparison.OrdinalIgnoreCase);
    }

    private static ProductItem Clone(ProductItem source)
    {
        return new ProductItem
        {
            Id = source.Id,
            ProductCode = source.ProductCode,
            ProductName = source.ProductName,
            Description = source.Description,
            Category = source.Category,
            ShopeeCategoryCode = source.ShopeeCategoryCode,
            Price = source.Price,
            Stock = source.Stock,
            Weight = source.Weight,
            Length = source.Length,
            Width = source.Width,
            Height = source.Height,
            SKU = source.SKU,
            Image1 = source.Image1,
            Image2 = source.Image2,
            Image3 = source.Image3,
            Image4 = source.Image4,
            ShopeeImage1Url = source.ShopeeImage1Url,
            ShopeeImage2Url = source.ShopeeImage2Url,
            ShopeeImage3Url = source.ShopeeImage3Url,
            ShopeeImage4Url = source.ShopeeImage4Url,
            LazadaImage1Url = source.LazadaImage1Url,
            LazadaImage2Url = source.LazadaImage2Url,
            LazadaImage3Url = source.LazadaImage3Url,
            LazadaImage4Url = source.LazadaImage4Url,
            VariationName = source.VariationName,
            VariationOption = source.VariationOption,
            VariationPrice = source.VariationPrice,
            VariationStock = source.VariationStock,
            VariationImageUrl = source.VariationImageUrl,
            Brand = source.Brand,
            BabyMaterial = source.BabyMaterial,
            CountryOfOrigin = source.CountryOfOrigin,
            WarrantyType = source.WarrantyType,
            WarrantyPeriod = source.WarrantyPeriod,
            ColorFamily = source.ColorFamily,
            DangerousGoods = source.DangerousGoods,
            DeliveryStandard = source.DeliveryStandard,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    private async void btnUploadImage1_Click(object? sender, EventArgs e) => await UploadSingleImageAsync(1, txtImage1);

    private async void btnUploadImage2_Click(object? sender, EventArgs e) => await UploadSingleImageAsync(2, txtImage2);

    private async void btnUploadImage3_Click(object? sender, EventArgs e) => await UploadSingleImageAsync(3, txtImage3);

    private async void btnUploadImage4_Click(object? sender, EventArgs e) => await UploadSingleImageAsync(4, txtImage4);

    private void btnViewImage1_Click(object? sender, EventArgs e) => ViewImage(txtImage1);

    private void btnViewImage2_Click(object? sender, EventArgs e) => ViewImage(txtImage2);

    private void btnViewImage3_Click(object? sender, EventArgs e) => ViewImage(txtImage3);

    private void btnViewImage4_Click(object? sender, EventArgs e) => ViewImage(txtImage4);

    private async Task UploadSingleImageAsync(int imageSequence, TextBox imagePathTextBox)
    {
        if (_isUploadingLazadaImages)
        {
            return;
        }

        var selectedPath = SelectImageFile(imagePathTextBox);
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        imagePathTextBox.Text = selectedPath;
        RememberLocalImagePath(imagePathTextBox, selectedPath);

        if (!TryApplyFormToProduct())
        {
            return;
        }

        try
        {
            _isUploadingLazadaImages = true;
            SetImageUploadButtonsEnabled(false);
            btnSave.Enabled = false;
            UseWaitCursor = true;
            txtAiNotes.Text = $"Uploading Image {imageSequence} and syncing Shopee/Lazada URL...";
            await Task.Yield();

            Product = await _productRepository.SaveAsync(Product);
            var imageResult = await _lazadaImageUploadService.UploadSingleAsync(Product, imageSequence);
            Product.SynchronizeMarketplaceImageUrls();
            var uploadedUrl = Product.GetSharedImageUrl(imageSequence - 1);
            imagePathTextBox.Text = uploadedUrl;
            Product = await _productRepository.SaveAsync(Product);

            var summary = BuildSingleImageUploadSummary(Product, imageResult);
            txtAiNotes.Text = summary;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to upload Lazada images from Product Editor.");
            txtAiNotes.Text = ex.Message;
        }
        finally
        {
            _isUploadingLazadaImages = false;
            SetImageUploadButtonsEnabled(true);
            btnSave.Enabled = true;
            UseWaitCursor = false;
        }
    }

    private void SetImageUploadButtonsEnabled(bool enabled)
    {
        btnUploadImage1.Enabled = enabled;
        btnUploadImage2.Enabled = enabled;
        btnUploadImage3.Enabled = enabled;
        btnUploadImage4.Enabled = enabled;
        btnViewImage1.Enabled = enabled;
        btnViewImage2.Enabled = enabled;
        btnViewImage3.Enabled = enabled;
        btnViewImage4.Enabled = enabled;
    }

    private void ViewImage(TextBox imagePathTextBox)
    {
        var previewSource = GetPreviewSource(imagePathTextBox);
        if (string.IsNullOrWhiteSpace(previewSource))
        {
            MessageBox.Show(this, "No image available to preview.", "View Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            using var preview = new ImagePreviewForm(previewSource, Product.ProductCode);
            preview.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "View Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool TryApplyFormToProduct()
    {
        SyncVariationTextFromGrid();

        if (numPrice.Value <= 0)
        {
            MessageBox.Show(this, "Price must be greater than zero.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            numPrice.Focus();
            numPrice.Select(0, numPrice.Text.Length);
            return false;
        }

        var originalImage1 = Product.Image1;
        var originalImage2 = Product.Image2;
        var originalImage3 = Product.Image3;
        var originalImage4 = Product.Image4;
        Product.ProductCode = txtProductCode.Text.Trim();
        Product.ProductName = txtProductName.Text.Trim();
        Product.Description = txtDescription.Text.Trim();
        Product.Category = cboCategory.Text.Trim();
        Product.Price = numPrice.Value;
        Product.Weight = numWeight.Value;
        Product.Length = numLength.Value;
        Product.Width = numWidth.Value;
        Product.Height = numHeight.Value;
        Product.SKU = txtSku.Text.Trim();
        Product.Image1 = txtImage1.Text.Trim();
        Product.Image2 = txtImage2.Text.Trim();
        Product.Image3 = txtImage3.Text.Trim();
        Product.Image4 = txtImage4.Text.Trim();
        ApplyImageFieldValue(0, originalImage1, Product.Image1);
        ApplyImageFieldValue(1, originalImage2, Product.Image2);
        ApplyImageFieldValue(2, originalImage3, Product.Image3);
        ApplyImageFieldValue(3, originalImage4, Product.Image4);
        Product.SynchronizeMarketplaceImageUrls();
        Product.VariationName = txtVariationName.Text.Trim();
        Product.VariationOption = NormalizeVariationInput(txtVariationOption.Text.Trim());
        Product.VariationPrice = numVariationPrice.Value > 0 ? numVariationPrice.Value : null;
        Product.VariationStock = numVariationStock.Value > 0 ? (int)numVariationStock.Value : null;
        Product.Brand = cboBrand.Text.Trim();
        Product.BabyMaterial = cboBabyMaterial.Text.Trim();
        Product.CountryOfOrigin = cboCountryOfOrigin.Text.Trim();
        Product.WarrantyType = cboWarrantyType.Text.Trim();
        Product.WarrantyPeriod = txtWarrantyPeriod.Text.Trim();
        Product.ColorFamily = cboColorFamily.Text.Trim();
        Product.DangerousGoods = string.IsNullOrWhiteSpace(cboDangerousGoods.Text) ? "No" : cboDangerousGoods.Text.Trim();
        Product.DeliveryStandard = string.IsNullOrWhiteSpace(cboDeliveryStandard.Text) ? "Yes" : cboDeliveryStandard.Text.Trim();

        var validation = ProductItemValidator.Validate(Product);
        if (!validation.IsValid)
        {
            MessageBox.Show(this, string.Join(Environment.NewLine, validation.Errors), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            if (validation.Errors.Any(static error => error.Contains("Price", StringComparison.OrdinalIgnoreCase)))
            {
                numPrice.Focus();
                numPrice.Select(0, numPrice.Text.Length);
            }

            return false;
        }

        return true;
    }

    private void ApplyImageFieldValue(int index, string previousPath, string currentPath)
    {
        if (string.IsNullOrWhiteSpace(currentPath))
        {
            Product.ClearSharedImageUrl(index);
            return;
        }

        if (IsHttpUrl(currentPath))
        {
            Product.SetSharedImageUrl(index, currentPath);
            return;
        }

        if (!string.Equals(previousPath?.Trim(), currentPath.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            Product.ClearSharedImageUrl(index);
        }
    }

    private static bool IsHttpUrl(string? value)
    {
        return Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static void RememberLocalImagePath(TextBox target, string? path)
    {
        target.Tag = File.Exists(path ?? string.Empty) ? path : null;
    }

    private static string GetSelectedLocalImagePath(TextBox target)
    {
        if (target.Tag is string taggedPath && File.Exists(taggedPath))
        {
            return taggedPath;
        }

        var text = target.Text.Trim();
        return File.Exists(text) ? text : string.Empty;
    }

    private static string GetPreviewSource(TextBox target)
    {
        var text = target.Text.Trim();
        if (IsHttpUrl(text) || File.Exists(text))
        {
            return text;
        }

        return GetSelectedLocalImagePath(target);
    }

    private static string BuildLazadaUploadSummary(LazadaImageUploadBatchResult result)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Lazada Image Upload Completed");
        builder.AppendLine();
        builder.AppendLine($"Products : {result.Products.Count}");
        builder.AppendLine($"Total    : {result.TotalImages}");
        builder.AppendLine($"Success  : {result.SuccessCount}");
        builder.AppendLine($"Failed   : {result.FailedCount}");
        builder.AppendLine($"Skipped  : {result.SkippedCount}");

        var failedImages = result.Products
            .SelectMany(product => product.Images.Select(image => new { product.ProductCode, Image = image }))
            .Where(static item => item.Image.Status == LazadaUploadStatus.Failed)
            .ToList();

        if (failedImages.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Errors:");
            foreach (var failed in failedImages)
            {
                builder.AppendLine($"- {failed.ProductCode} image {failed.Image.ImageSequence}: {failed.Image.ErrorMessage}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static string BuildSingleImageUploadSummary(ProductItem product, LazadaImageUploadImageResult imageResult)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine($"Image {imageResult.ImageSequence} Upload Completed");
        builder.AppendLine();
        builder.AppendLine($"Product : {product.ProductCode}");
        builder.AppendLine($"Status  : {imageResult.Status}");

        if (!string.IsNullOrWhiteSpace(imageResult.LazadaImageUrl))
        {
            builder.AppendLine($"URL     : {imageResult.LazadaImageUrl}");
        }

        if (!string.IsNullOrWhiteSpace(imageResult.ErrorMessage))
        {
            builder.AppendLine($"Error   : {imageResult.ErrorMessage}");
        }

        return builder.ToString().TrimEnd();
    }
}
