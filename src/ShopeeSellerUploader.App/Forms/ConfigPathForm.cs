using ShopeeSellerUploader.Contracts.Configuration;

namespace ShopeeSellerUploader.App.Forms;

public sealed class ConfigPathForm : Form
{
    private readonly TextBox _txtDatabasePath = new();
    private readonly TextBox _txtTemplatePath = new();
    private readonly TextBox _txtExportPath = new();
    private readonly Button _btnSave = new();

    public ConfigPathForm(ProductCatalogOptions options, string databasePath, string templatePath, string exportPath)
    {
        DatabasePath = databasePath;
        TemplatePath = templatePath;
        ExportPath = exportPath;

        InitializeComponent();

        _txtDatabasePath.Text = databasePath;
        _txtTemplatePath.Text = templatePath;
        _txtExportPath.Text = exportPath;
    }

    public string DatabasePath { get; private set; }
    public string TemplatePath { get; private set; }
    public string ExportPath { get; private set; }

    private void InitializeComponent()
    {
        var layout = new TableLayoutPanel();
        var lblHelp = new Label();
        var contentPanel = new Panel();
        var fieldsPanel = new TableLayoutPanel();
        var bottomPanel = new FlowLayoutPanel();
        var btnClose = new Button();

        SuspendLayout();

        layout.ColumnCount = 1;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowCount = 3;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        layout.Dock = DockStyle.Fill;
        layout.Padding = new Padding(18, 16, 18, 16);
        layout.BackColor = Color.FromArgb(248, 250, 252);

        lblHelp.Dock = DockStyle.Fill;
        lblHelp.Text = "Update the main working paths here. You can browse to a new database file, template folder, or export folder.";
        lblHelp.TextAlign = ContentAlignment.MiddleLeft;
        lblHelp.Font = new Font("Segoe UI", 9F);

        contentPanel.Dock = DockStyle.Fill;
        contentPanel.Padding = new Padding(0, 10, 0, 0);
        contentPanel.BackColor = Color.FromArgb(248, 250, 252);

        fieldsPanel.ColumnCount = 3;
        fieldsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128F));
        fieldsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        fieldsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104F));
        fieldsPanel.RowCount = 3;
        fieldsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        fieldsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        fieldsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        fieldsPanel.Dock = DockStyle.Top;
        fieldsPanel.AutoSize = true;
        fieldsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        fieldsPanel.BackColor = Color.White;
        fieldsPanel.Padding = new Padding(18);
        fieldsPanel.Margin = new Padding(0);

        AddPathRow(fieldsPanel, 0, "Database Path", _txtDatabasePath, btnBrowseDatabase_Click);
        AddPathRow(fieldsPanel, 1, "Template Path", _txtTemplatePath, btnBrowseTemplate_Click);
        AddPathRow(fieldsPanel, 2, "Export Path", _txtExportPath, btnBrowseExport_Click);
        contentPanel.Controls.Add(fieldsPanel);

        bottomPanel.Dock = DockStyle.Fill;
        bottomPanel.FlowDirection = FlowDirection.RightToLeft;
        bottomPanel.WrapContents = false;

        ConfigureSecondaryButton(btnClose, "Close", btnClose_Click);
        ConfigureSuccessButton(_btnSave, "Save", btnSave_Click);
        bottomPanel.Controls.Add(btnClose);
        bottomPanel.Controls.Add(_btnSave);

        layout.Controls.Add(lblHelp, 0, 0);
        layout.Controls.Add(contentPanel, 0, 1);
        layout.Controls.Add(bottomPanel, 0, 2);
        Controls.Add(layout);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(980, 320);
        MinimumSize = new Size(860, 320);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Config Path";

        ResumeLayout(false);
    }

    private void AddPathRow(TableLayoutPanel parent, int rowIndex, string labelText, TextBox textBox, EventHandler browseHandler)
    {
        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            Margin = new Padding(0, 0, 12, 12)
        };

        textBox.Dock = DockStyle.Fill;
        textBox.Font = new Font("Segoe UI", 10F);
        textBox.Margin = new Padding(0, 0, 12, 12);
        textBox.Height = 34;

        var browseButton = new Button();
        ConfigureSecondaryButton(browseButton, "Browse", browseHandler);
        browseButton.Width = 92;
        browseButton.Margin = new Padding(0, 0, 0, 12);

        parent.Controls.Add(label, 0, rowIndex);
        parent.Controls.Add(textBox, 1, rowIndex);
        parent.Controls.Add(browseButton, 2, rowIndex);
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

    private void btnBrowseDatabase_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Select Database File",
            Filter = "Database Files (*.db)|*.db|All Files (*.*)|*.*",
            FileName = Path.GetFileName(_txtDatabasePath.Text),
            InitialDirectory = ResolveInitialDirectory(_txtDatabasePath.Text)
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _txtDatabasePath.Text = dialog.FileName;
        }
    }

    private void btnBrowseTemplate_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select Template Folder",
            InitialDirectory = ResolveInitialDirectory(_txtTemplatePath.Text),
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _txtTemplatePath.Text = dialog.SelectedPath;
        }
    }

    private void btnBrowseExport_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select Export Folder",
            InitialDirectory = ResolveInitialDirectory(_txtExportPath.Text),
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _txtExportPath.Text = dialog.SelectedPath;
        }
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        var databasePath = _txtDatabasePath.Text.Trim();
        var templatePath = _txtTemplatePath.Text.Trim();
        var exportPath = _txtExportPath.Text.Trim();

        if (string.IsNullOrWhiteSpace(databasePath) ||
            string.IsNullOrWhiteSpace(templatePath) ||
            string.IsNullOrWhiteSpace(exportPath))
        {
            MessageBox.Show(this, "Please fill in all paths first.", "Config Path", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DatabasePath = databasePath;
        TemplatePath = templatePath;
        ExportPath = exportPath;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnClose_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private static string ResolveInitialDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            return path;
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
        {
            return directory;
        }

        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}
