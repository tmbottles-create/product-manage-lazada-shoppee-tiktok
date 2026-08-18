namespace ShopeeSellerUploader.App.Forms;

public sealed class ImagePreviewForm : Form
{
    private readonly string _imageSource;
    private readonly PictureBox _pictureBox;
    private readonly Label _statusLabel;

    public ImagePreviewForm(string imageSource, string productCode)
    {
        _imageSource = imageSource;
        _pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Black
        };
        _statusLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 32,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(24, 24, 27),
            Padding = new Padding(12, 7, 12, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            Text = "Loading image..."
        };

        Controls.Add(_pictureBox);
        Controls.Add(_statusLabel);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.Black;
        ClientSize = new Size(980, 720);
        MinimumSize = new Size(700, 500);
        StartPosition = FormStartPosition.CenterParent;
        Text = $"Image Preview - {productCode}";
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        LoadPreviewImage();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pictureBox.Image?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void LoadPreviewImage()
    {
        if (Uri.TryCreate(_imageSource, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            _statusLabel.Text = uri.ToString();
            _pictureBox.LoadCompleted += (_, args) =>
            {
                if (args.Error is not null)
                {
                    MessageBox.Show(this, args.Error.Message, "Image Preview", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }

                _statusLabel.Text = "Loaded from URL";
            };
            _pictureBox.LoadAsync(uri.ToString());
            return;
        }

        if (!File.Exists(_imageSource))
        {
            throw new FileNotFoundException("Image file not found.", _imageSource);
        }

        _statusLabel.Text = _imageSource;
        using var stream = new FileStream(_imageSource, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var original = Image.FromStream(stream);
        _pictureBox.Image = new Bitmap(original);
    }
}
