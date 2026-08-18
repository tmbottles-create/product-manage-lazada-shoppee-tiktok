namespace ShopeeSellerUploader.App.Forms;

public sealed class AiFillDialog : Form
{
    private readonly TextBox _txtImagePath;
    private readonly TextBox _txtDetails;
    private readonly PictureBox _picturePreview;
    private readonly Label _previewStatusLabel;
    private readonly Button _btnBrowse;
    private readonly Button _btnGetContent;
    private readonly Button _btnCancel;

    public IReadOnlyList<string> SelectedImagePaths =>
        string.IsNullOrWhiteSpace(_txtImagePath.Text)
            ? []
            : [_txtImagePath.Text.Trim()];

    public string AdditionalDetails => _txtDetails.Text.Trim();

    public AiFillDialog(IReadOnlyList<string> initialImagePaths, string initialDetails)
    {
        Text = "AI Fill Assistant";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(920, 700);
        MinimumSize = new Size(920, 700);
        Font = new Font("Segoe UI", 9.5F);
        BackColor = Color.White;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16),
            BackColor = Color.White
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(root);

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 2,
            AutoSize = true,
            Margin = new Padding(0)
        };
        header.Controls.Add(new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Text = "AI Fill",
            Margin = new Padding(0)
        }, 0, 0);
        header.Controls.Add(new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(71, 85, 105),
            Text = "Browse one local image, preview it instantly, add details, then click Get content from AI.",
            Margin = new Padding(0, 6, 0, 0)
        }, 0, 1);
        root.Controls.Add(header, 0, 0);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Margin = new Padding(0, 14, 0, 0)
        };
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
        root.Controls.Add(content, 0, 1);

        content.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "Image",
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 41, 59),
            Margin = new Padding(0, 0, 0, 8)
        }, 0, 0);

        var pathRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        };
        pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        _txtImagePath = new TextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0),
            ReadOnly = true
        };
        if (initialImagePaths.Count > 0)
        {
            _txtImagePath.Text = initialImagePaths[0];
        }

        _btnBrowse = new Button
        {
            Text = "Browse",
            Width = 110,
            Height = 34,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(10, 0, 0, 0)
        };
        _btnBrowse.FlatAppearance.BorderSize = 0;
        _btnBrowse.Click += btnBrowse_Click;

        pathRow.Controls.Add(_txtImagePath, 0, 0);
        pathRow.Controls.Add(_btnBrowse, 1, 0);
        content.Controls.Add(pathRow, 0, 1);

        var previewPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(248, 250, 252),
            Margin = new Padding(0, 0, 0, 14)
        };

        _previewStatusLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 34,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 10, 0),
            ForeColor = Color.FromArgb(71, 85, 105),
            BackColor = Color.FromArgb(241, 245, 249),
            Text = "No image selected yet."
        };

        _picturePreview = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White
        };

        previewPanel.Controls.Add(_picturePreview);
        previewPanel.Controls.Add(_previewStatusLabel);
        content.Controls.Add(previewPanel, 0, 2);

        content.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "Additional Details",
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 41, 59),
            Margin = new Padding(0, 0, 0, 8)
        }, 0, 3);

        _txtDetails = new TextBox
        {
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Text = initialDetails,
            PlaceholderText = "Add more product details, selling points, material, color, size, or instructions for AI."
        };
        content.Controls.Add(_txtDetails, 0, 4);

        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 16, 0, 0)
        };

        _btnGetContent = new Button
        {
            Text = "Get content from AI",
            Width = 170,
            Height = 38,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _btnGetContent.FlatAppearance.BorderSize = 0;
        _btnGetContent.Click += btnGetContent_Click;

        _btnCancel = new Button
        {
            Text = "Cancel",
            Width = 100,
            Height = 38,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(30, 41, 59),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(10, 0, 0, 0)
        };
        _btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        _btnCancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        actionPanel.Controls.Add(_btnGetContent);
        actionPanel.Controls.Add(_btnCancel);
        root.Controls.Add(actionPanel, 0, 2);

        AcceptButton = _btnGetContent;
        CancelButton = _btnCancel;

        TryLoadPreview(_txtImagePath.Text.Trim());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _picturePreview.Image?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void btnBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp;*.gif",
            FilterIndex = 1,
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            RestoreDirectory = true,
            AutoUpgradeEnabled = false,
            AddToRecent = false,
            ValidateNames = true,
            Title = "Select Image"
        };

        var currentPath = _txtImagePath.Text.Trim();
        if (File.Exists(currentPath))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(currentPath);
            dialog.FileName = Path.GetFileName(currentPath);
        }
        else
        {
            ImageBrowseDirectoryState.ApplyDefaultDirectory(dialog);
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _txtImagePath.Text = dialog.FileName;
        ImageBrowseDirectoryState.Remember(dialog.FileName);
        TryLoadPreview(dialog.FileName);
    }

    private void TryLoadPreview(string imagePath)
    {
        _picturePreview.Image?.Dispose();
        _picturePreview.Image = null;

        if (!File.Exists(imagePath))
        {
            _previewStatusLabel.Text = "No image selected yet.";
            return;
        }

        try
        {
            using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var original = Image.FromStream(stream);
            _picturePreview.Image = new Bitmap(original);
            _previewStatusLabel.Text = imagePath;
        }
        catch (Exception ex)
        {
            _previewStatusLabel.Text = "Failed to load preview.";
            MessageBox.Show(this, ex.Message, "Image Preview", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnGetContent_Click(object? sender, EventArgs e)
    {
        var imagePath = _txtImagePath.Text.Trim();
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            MessageBox.Show(this, "Please browse and select one local image before using AI.", "AI Fill", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!File.Exists(imagePath))
        {
            MessageBox.Show(this, "Selected image file could not be found.", "AI Fill", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
