using ShopeeSellerUploader.Contracts.Configuration;

namespace ShopeeSellerUploader.App.Forms;

public sealed class ImageKitSetupForm : Form
{
    private readonly TextBox _txtUploadApiUrl;
    private readonly TextBox _txtUrlEndpoint;
    private readonly TextBox _txtUploadFolderPath;
    private readonly TextBox _txtPrivateKey;
    private readonly TextBox _txtPrivateKeyEnvironmentVariable;
    private readonly NumericUpDown _numTimeoutSeconds;
    private readonly NumericUpDown _numMaxUploadSizeMb;
    private readonly CheckBox _chkUseUniqueFileName;

    public ImageKitSetupForm(ImageKitOptions options)
    {
        Text = "ImageKit Setup";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(820, 520);
        MinimumSize = new Size(820, 520);
        Font = new Font("Segoe UI", 9.5F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(16),
            BackColor = Color.White
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = new Label
        {
            AutoSize = true,
            Text = "Configure ImageKit upload settings for Lazada image hosting.",
            Font = new Font("Segoe UI", 11F, FontStyle.Regular),
            ForeColor = Color.FromArgb(31, 41, 55),
            Margin = new Padding(0, 0, 0, 12)
        };

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            ColumnCount = 2,
            RowCount = 8,
            BackColor = Color.White
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        _txtUploadApiUrl = CreateTextBox(options.UploadApiUrl);
        _txtUrlEndpoint = CreateTextBox(options.UrlEndpoint);
        _txtUploadFolderPath = CreateTextBox(options.UploadFolderPath);
        _txtPrivateKey = CreateTextBox(options.PrivateKey, usePassword: true);
        _txtPrivateKeyEnvironmentVariable = CreateTextBox(options.PrivateKeyEnvironmentVariable);
        _numTimeoutSeconds = CreateNumeric(options.TimeoutSeconds, 15, 600);
        _numMaxUploadSizeMb = CreateNumeric(options.MaxUploadSizeMb, 1, 100);
        _chkUseUniqueFileName = new CheckBox
        {
            Text = "Generate unique file names on upload",
            AutoSize = true,
            Checked = options.UseUniqueFileName,
            Margin = new Padding(0, 8, 0, 8)
        };

        AddRow(fields, 0, "Upload API URL", _txtUploadApiUrl);
        AddRow(fields, 1, "URL Endpoint", _txtUrlEndpoint);
        AddRow(fields, 2, "Upload Folder Path", _txtUploadFolderPath);
        AddRow(fields, 3, "Private API Key", _txtPrivateKey);
        AddRow(fields, 4, "Private Key Env Var", _txtPrivateKeyEnvironmentVariable);
        AddRow(fields, 5, "Timeout Seconds", _numTimeoutSeconds);
        AddRow(fields, 6, "Max Upload Size (MB)", _numMaxUploadSizeMb);
        AddRow(fields, 7, "Options", _chkUseUniqueFileName);

        var help = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(760, 0),
            ForeColor = Color.FromArgb(75, 85, 99),
            Text = "Use either Private API Key in this form or the environment variable. URL Endpoint example: https://ik.imagekit.io/yzx2xk3aq",
            Margin = new Padding(0, 8, 0, 12)
        };

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };

        var btnSave = new Button
        {
            Text = "Save",
            Width = 110,
            Height = 36,
            DialogResult = DialogResult.OK
        };
        btnSave.Click += btnSave_Click;

        var btnCancel = new Button
        {
            Text = "Cancel",
            Width = 110,
            Height = 36,
            DialogResult = DialogResult.Cancel
        };

        buttons.Controls.Add(btnCancel);
        buttons.Controls.Add(btnSave);

        AcceptButton = btnSave;
        CancelButton = btnCancel;

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(fields, 0, 1);
        root.Controls.Add(help, 0, 2);
        root.Controls.Add(buttons, 0, 3);
        Controls.Add(root);
    }

    public string UploadApiUrl => _txtUploadApiUrl.Text.Trim();
    public string UrlEndpoint => _txtUrlEndpoint.Text.Trim();
    public string UploadFolderPath => _txtUploadFolderPath.Text.Trim();
    public string PrivateKey => _txtPrivateKey.Text.Trim();
    public string PrivateKeyEnvironmentVariable => _txtPrivateKeyEnvironmentVariable.Text.Trim();
    public int TimeoutSeconds => Decimal.ToInt32(_numTimeoutSeconds.Value);
    public int MaxUploadSizeMb => Decimal.ToInt32(_numMaxUploadSizeMb.Value);
    public bool UseUniqueFileName => _chkUseUniqueFileName.Checked;

    private static TextBox CreateTextBox(string value, bool usePassword = false)
    {
        return new TextBox
        {
            Text = value,
            Dock = DockStyle.Top,
            UseSystemPasswordChar = usePassword,
            Height = 34
        };
    }

    private static NumericUpDown CreateNumeric(int value, int minimum, int maximum)
    {
        return new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = Math.Clamp(value, minimum, maximum),
            Width = 120,
            Height = 34
        };
    }

    private static void AddRow(TableLayoutPanel table, int rowIndex, string labelText, Control control)
    {
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var label = new Label
        {
            Text = labelText,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true,
            Margin = new Padding(0, 8, 12, 0)
        };

        control.Margin = new Padding(0, 4, 0, 4);
        table.Controls.Add(label, 0, rowIndex);
        table.Controls.Add(control, 1, rowIndex);
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UploadApiUrl))
        {
            MessageBox.Show(this, "Upload API URL is required.", "ImageKit Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.None;
            return;
        }

        if (string.IsNullOrWhiteSpace(UrlEndpoint))
        {
            MessageBox.Show(this, "URL Endpoint is required.", "ImageKit Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.None;
            return;
        }

        if (string.IsNullOrWhiteSpace(PrivateKey) && string.IsNullOrWhiteSpace(PrivateKeyEnvironmentVariable))
        {
            MessageBox.Show(this, "Provide Private API Key or a Private Key environment variable name.", "ImageKit Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.None;
        }
    }
}
