using ShopeeSellerUploader.Contracts.Interfaces;

namespace ShopeeSellerUploader.App.Forms;

public sealed class MarketplaceCategoryMasterForm : Form
{
    private readonly IMarketplaceCategoryMasterRepository _repository;
    private readonly ITemplateMetadataService _templateMetadataService;
    private readonly string _marketplaceKey;
    private readonly string _marketplaceLabel;
    private readonly IReadOnlyList<string> _initialItems;
    private readonly ListBox _listCategories = new();
    private readonly TextBox _txtCategoryName = new();
    private readonly Button _btnAdd = new();
    private readonly Button _btnUpdate = new();
    private readonly Button _btnDelete = new();
    private readonly Button _btnImport = new();
    private readonly Button _btnSave = new();
    private readonly Button _btnClose = new();
    private readonly Label _lblSummary = new();
    private List<string> _items = [];
    private bool _isDirty;

    public MarketplaceCategoryMasterForm(
        string title,
        string marketplaceKey,
        string marketplaceLabel,
        IReadOnlyList<string> initialItems,
        ITemplateMetadataService templateMetadataService,
        IMarketplaceCategoryMasterRepository repository)
    {
        _repository = repository;
        _templateMetadataService = templateMetadataService;
        _marketplaceKey = marketplaceKey;
        _marketplaceLabel = marketplaceLabel;
        _initialItems = initialItems;

        InitializeComponent(title);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        LoadItems();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_isDirty)
        {
            var result = MessageBox.Show(
                this,
                "You have unsaved changes. Close without saving?",
                $"{_marketplaceLabel} Cat Master",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
        }

        base.OnFormClosing(e);
    }

    private void LoadItems()
    {
        _items = _initialItems
            .OrderBy(static value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
        RefreshList();
        SetDirty(false);
    }

    private void InitializeComponent(string title)
    {
        var layout = new TableLayoutPanel();
        var lblHelp = new Label();
        var contentPanel = new TableLayoutPanel();
        var listPanel = new TableLayoutPanel();
        var editorPanel = new Panel();
        var lblCategoryName = new Label();
        var editorCard = new Panel();
        var actionPanel = new FlowLayoutPanel();
        var listHeaderPanel = new FlowLayoutPanel();
        var bottomPanel = new FlowLayoutPanel();

        SuspendLayout();

        layout.ColumnCount = 1;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowCount = 4;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        layout.Dock = DockStyle.Fill;
        layout.Padding = new Padding(18, 16, 18, 16);
        layout.BackColor = Color.FromArgb(248, 250, 252);

        lblHelp.Dock = DockStyle.Fill;
        lblHelp.Text = $"Manage {_marketplaceLabel} master categories for dropdown usage. Add, edit, or delete values here.";
        lblHelp.TextAlign = ContentAlignment.MiddleLeft;
        lblHelp.Font = new Font("Segoe UI", 9F);

        _lblSummary.Dock = DockStyle.Fill;
        _lblSummary.TextAlign = ContentAlignment.MiddleLeft;
        _lblSummary.ForeColor = Color.FromArgb(71, 85, 105);
        _lblSummary.Font = new Font("Segoe UI", 9F);

        contentPanel.ColumnCount = 2;
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F));
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
        contentPanel.Dock = DockStyle.Fill;

        listPanel.ColumnCount = 1;
        listPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        listPanel.RowCount = 2;
        listPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        listPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        listPanel.Dock = DockStyle.Fill;
        listPanel.BackColor = Color.White;
        listPanel.Padding = new Padding(12);
        listPanel.Margin = new Padding(0, 0, 8, 0);
        contentPanel.Controls.Add(listPanel, 0, 0);

        listHeaderPanel.Dock = DockStyle.Fill;
        listHeaderPanel.FlowDirection = FlowDirection.LeftToRight;
        listHeaderPanel.WrapContents = false;
        listHeaderPanel.Margin = new Padding(0);
        listPanel.Controls.Add(listHeaderPanel, 0, 0);

        if (_marketplaceKey is "Lazada" or "TikTok")
        {
            ConfigureSecondaryButton(
                _btnImport,
                _marketplaceKey == "Lazada" ? "Import Lazada File" : "Import TikTok File",
                btnImport_Click);
            _btnImport.Width = 172;
            _btnImport.Margin = new Padding(0, 0, 10, 0);
            listHeaderPanel.Controls.Add(_btnImport);
        }

        _listCategories.Dock = DockStyle.Fill;
        _listCategories.Font = new Font("Segoe UI", 10F);
        _listCategories.BorderStyle = BorderStyle.FixedSingle;
        _listCategories.BackColor = Color.FromArgb(248, 250, 252);
        _listCategories.SelectedIndexChanged += listCategories_SelectedIndexChanged;
        _listCategories.Margin = new Padding(0);
        listPanel.Controls.Add(_listCategories, 0, 1);

        editorPanel.Dock = DockStyle.Fill;
        editorPanel.BackColor = Color.FromArgb(248, 250, 252);
        editorPanel.Margin = new Padding(8, 0, 0, 0);
        contentPanel.Controls.Add(editorPanel, 1, 0);

        editorCard.Dock = DockStyle.Top;
        editorCard.Height = 190;
        editorCard.BackColor = Color.White;
        editorCard.Padding = new Padding(18, 16, 18, 16);
        editorPanel.Controls.Add(editorCard);

        lblCategoryName.AutoSize = true;
        lblCategoryName.Location = new Point(0, 0);
        lblCategoryName.Text = $"{_marketplaceLabel} Category";
        lblCategoryName.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        editorCard.Controls.Add(lblCategoryName);

        _txtCategoryName.Location = new Point(0, 28);
        _txtCategoryName.Width = 342;
        _txtCategoryName.Font = new Font("Segoe UI", 10F);
        editorCard.Controls.Add(_txtCategoryName);

        actionPanel.Location = new Point(0, 72);
        actionPanel.Width = 342;
        actionPanel.Height = 40;
        actionPanel.FlowDirection = FlowDirection.LeftToRight;
        actionPanel.WrapContents = false;
        actionPanel.Margin = new Padding(0);
        editorCard.Controls.Add(actionPanel);

        ConfigurePrimaryButton(_btnAdd, "Add", btnAdd_Click);
        ConfigureSecondaryButton(_btnUpdate, "Edit", btnUpdate_Click);
        ConfigureDangerButton(_btnDelete, "Delete", btnDelete_Click);
        _btnUpdate.Enabled = false;
        _btnDelete.Enabled = false;
        actionPanel.Controls.Add(_btnAdd);
        actionPanel.Controls.Add(_btnUpdate);
        actionPanel.Controls.Add(_btnDelete);

        bottomPanel.Dock = DockStyle.Fill;
        bottomPanel.FlowDirection = FlowDirection.RightToLeft;
        bottomPanel.WrapContents = false;

        ConfigureSecondaryButton(_btnClose, "Close", btnClose_Click);
        ConfigureSuccessButton(_btnSave, "Save", btnSave_Click);
        bottomPanel.Controls.Add(_btnClose);
        bottomPanel.Controls.Add(_btnSave);

        layout.Controls.Add(lblHelp, 0, 0);
        layout.Controls.Add(_lblSummary, 0, 1);
        layout.Controls.Add(contentPanel, 0, 2);
        layout.Controls.Add(bottomPanel, 0, 3);
        Controls.Add(layout);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(860, 480);
        MinimumSize = new Size(780, 420);
        StartPosition = FormStartPosition.CenterParent;
        Text = title;

        ResumeLayout(false);
    }

    private void ConfigurePrimaryButton(Button button, string text, EventHandler onClick)
    {
        button.Text = text;
        button.Width = 96;
        button.Height = 34;
        button.BackColor = Color.FromArgb(37, 99, 235);
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Margin = new Padding(0, 0, 8, 0);
        button.Click += onClick;
    }

    private void ConfigureSecondaryButton(Button button, string text, EventHandler onClick)
    {
        button.Text = text;
        button.Width = 110;
        button.Height = 34;
        button.BackColor = Color.White;
        button.ForeColor = Color.FromArgb(30, 64, 175);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(191, 219, 254);
        button.Margin = new Padding(0, 0, 8, 0);
        button.Click += onClick;
    }

    private void ConfigureDangerButton(Button button, string text, EventHandler onClick)
    {
        button.Text = text;
        button.Width = 96;
        button.Height = 34;
        button.BackColor = Color.FromArgb(220, 38, 38);
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Margin = new Padding(0);
        button.Click += onClick;
    }

    private void ConfigureSuccessButton(Button button, string text, EventHandler onClick)
    {
        button.Text = text;
        button.Width = 104;
        button.Height = 36;
        button.BackColor = Color.FromArgb(22, 163, 74);
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Margin = new Padding(0, 0, 8, 0);
        button.Click += onClick;
    }

    private void RefreshList(string? selectedValue = null)
    {
        _listCategories.BeginUpdate();
        _listCategories.Items.Clear();
        foreach (var item in _items.OrderBy(static value => value, StringComparer.OrdinalIgnoreCase))
        {
            _listCategories.Items.Add(item);
        }

        _listCategories.EndUpdate();

        if (!string.IsNullOrWhiteSpace(selectedValue))
        {
            var selectedIndex = _listCategories.Items.IndexOf(selectedValue);
            if (selectedIndex >= 0)
            {
                _listCategories.SelectedIndex = selectedIndex;
            }
        }

        _lblSummary.Text = $"{_marketplaceLabel} categories: {_items.Count}";
        SyncEditorButtons();
    }

    private void SyncEditorButtons()
    {
        var hasSelection = _listCategories.SelectedIndex >= 0;
        _btnUpdate.Enabled = hasSelection;
        _btnDelete.Enabled = hasSelection;

        if (hasSelection)
        {
            _txtCategoryName.Text = _listCategories.SelectedItem?.ToString() ?? string.Empty;
        }
        else if (!_txtCategoryName.Focused)
        {
            _txtCategoryName.Text = string.Empty;
        }
    }

    private void SetDirty(bool value)
    {
        _isDirty = value;
        _btnSave.Enabled = value;
    }

    private void listCategories_SelectedIndexChanged(object? sender, EventArgs e)
    {
        SyncEditorButtons();
    }

    private void btnAdd_Click(object? sender, EventArgs e)
    {
        var name = _txtCategoryName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(this, "Enter a category name first.", $"{_marketplaceLabel} Cat Master", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_items.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, "This category already exists.", $"{_marketplaceLabel} Cat Master", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _items.Add(name);
        RefreshList(name);
        SetDirty(true);
    }

    private void btnUpdate_Click(object? sender, EventArgs e)
    {
        if (_listCategories.SelectedItem is not string currentValue)
        {
            return;
        }

        var updatedValue = _txtCategoryName.Text.Trim();
        if (string.IsNullOrWhiteSpace(updatedValue))
        {
            MessageBox.Show(this, "Enter a category name first.", $"{_marketplaceLabel} Cat Master", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!string.Equals(currentValue, updatedValue, StringComparison.OrdinalIgnoreCase) &&
            _items.Contains(updatedValue, StringComparer.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, "This category already exists.", $"{_marketplaceLabel} Cat Master", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var index = _items.FindIndex(value => string.Equals(value, currentValue, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return;
        }

        _items[index] = updatedValue;
        RefreshList(updatedValue);
        SetDirty(true);
    }

    private void btnDelete_Click(object? sender, EventArgs e)
    {
        if (_listCategories.SelectedItem is not string currentValue)
        {
            return;
        }

        var confirm = MessageBox.Show(
            this,
            $"Delete '{currentValue}' from {_marketplaceLabel} master?",
            $"{_marketplaceLabel} Cat Master",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        _items.RemoveAll(value => string.Equals(value, currentValue, StringComparison.OrdinalIgnoreCase));
        _txtCategoryName.Clear();
        RefreshList();
        SetDirty(true);
    }

    private async void btnSave_Click(object? sender, EventArgs e)
    {
        await _repository.ReplaceAllAsync(_marketplaceKey, _items);
        SetDirty(false);
        DialogResult = DialogResult.OK;

        MessageBox.Show(
            this,
            $"Saved {_items.Count} {_marketplaceLabel} category value(s).",
            $"{_marketplaceLabel} Cat Master",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void btnClose_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void btnImport_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = _marketplaceKey == "Lazada" ? "Select Lazada Template File" : "Select TikTok Template File",
            Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };
        ImageBrowseDirectoryState.ApplyDefaultDirectory(dialog);

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        ImageBrowseDirectoryState.Remember(dialog.FileName);

        var importedItems = _marketplaceKey == "Lazada"
            ? _templateMetadataService.GetLazadaSheetNamesFromFileAsync(dialog.FileName).GetAwaiter().GetResult()
            : _templateMetadataService.GetTikTokCategoryNamesFromFileAsync(dialog.FileName).GetAwaiter().GetResult();

        if (importedItems.Count == 0)
        {
            MessageBox.Show(
                this,
                $"No {_marketplaceLabel} category values were found in the selected file.",
                $"{_marketplaceLabel} Cat Master",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var addedCount = 0;
        foreach (var importedItem in importedItems)
        {
            if (_items.Contains(importedItem, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            _items.Add(importedItem);
            addedCount++;
        }

        RefreshList();
        SetDirty(true);

        MessageBox.Show(
            this,
            $"Imported {addedCount} new {_marketplaceLabel} value(s).",
            $"{_marketplaceLabel} Cat Master",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}
