using ShopeeSellerUploader.Contracts.Configuration;

namespace ShopeeSellerUploader.App.Forms;

public sealed class LazadaImageModeForm : Form
{
    private readonly ComboBox _modeComboBox;

    public LazadaImageModeForm(LazadaImageMode currentMode)
    {
        Text = "Lazada Image Mode";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 240);
        MinimumSize = new Size(520, 240);
        Font = new Font("Segoe UI", 10F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var titleLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
            Text = "Choose how Lazada export should write product images"
        };

        var descriptionLabel = new Label
        {
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 10),
            ForeColor = Color.FromArgb(70, 70, 70),
            MaximumSize = new Size(470, 0),
            Text = "Public Image URL uses http/https links. Local File Path writes image paths from your computer into the Lazada template."
        };

        _modeComboBox = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(0, 0, 0, 16),
            Height = 34,
            IntegralHeight = false,
            DropDownHeight = 120
        };
        _modeComboBox.Items.Add(new ModeOption(LazadaImageMode.PublicImageUrl, "Public Image URL"));
        _modeComboBox.Items.Add(new ModeOption(LazadaImageMode.LocalFilePath, "Local File Path"));
        _modeComboBox.SelectedItem = _modeComboBox.Items
            .OfType<ModeOption>()
            .FirstOrDefault(option => option.Mode == currentMode)
            ?? _modeComboBox.Items[0];

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = false,
            WrapContents = false,
            Margin = new Padding(0),
            Padding = new Padding(0),
            Height = 42
        };

        var saveButton = new Button
        {
            Text = "Save",
            DialogResult = DialogResult.OK,
            Width = 96,
            Height = 34,
            BackColor = Color.FromArgb(47, 95, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        saveButton.FlatAppearance.BorderSize = 0;

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Width = 96,
            Height = 34,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(40, 40, 40),
            FlatStyle = FlatStyle.Flat
        };
        cancelButton.FlatAppearance.BorderColor = Color.FromArgb(190, 198, 212);

        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);

        layout.Controls.Add(titleLabel, 0, 0);
        layout.Controls.Add(descriptionLabel, 0, 1);
        layout.Controls.Add(_modeComboBox, 0, 2);
        layout.Controls.Add(buttonPanel, 0, 3);

        Controls.Add(layout);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    public LazadaImageMode SelectedMode =>
        (_modeComboBox.SelectedItem as ModeOption)?.Mode ?? LazadaImageMode.PublicImageUrl;

    private sealed record ModeOption(LazadaImageMode Mode, string Label)
    {
        public override string ToString() => Label;
    }
}
