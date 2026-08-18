namespace ShopeeSellerUploader.App.Forms;

partial class ApiKeyPromptForm
{
    private System.ComponentModel.IContainer components = null!;
    private TableLayoutPanel layout = null!;
    private TextBox txtApiKey = null!;
    private CheckBox chkSaveToMachine = null!;
    private CheckBox chkClearExistingKey = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        layout = new TableLayoutPanel();
        txtApiKey = new TextBox();
        chkSaveToMachine = new CheckBox();
        chkClearExistingKey = new CheckBox();
        btnSave = new Button();
        btnCancel = new Button();
        SuspendLayout();

        layout.ColumnCount = 1;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowCount = 5;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.Dock = DockStyle.Fill;
        layout.Padding = new Padding(12);

        var lblInfo = new Label
        {
            AutoSize = true,
            Text = "OpenAI API Key (will be saved encrypted with Windows DPAPI)"
        };
        txtApiKey.Dock = DockStyle.Fill;
        txtApiKey.UseSystemPasswordChar = true;
        chkSaveToMachine.Text = "Save encrypted on this machine";
        chkSaveToMachine.Checked = true;
        chkClearExistingKey.Text = "Clear saved API key";
        chkClearExistingKey.CheckedChanged += chkClearExistingKey_CheckedChanged;

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        btnSave.Text = "OK";
        btnSave.Width = 100;
        btnSave.Click += btnSave_Click;
        btnCancel.Text = "Cancel";
        btnCancel.Width = 100;
        btnCancel.Click += btnCancel_Click;
        buttonPanel.Controls.Add(btnCancel);
        buttonPanel.Controls.Add(btnSave);

        layout.Controls.Add(lblInfo, 0, 0);
        layout.Controls.Add(txtApiKey, 0, 1);
        layout.Controls.Add(chkSaveToMachine, 0, 2);
        layout.Controls.Add(chkClearExistingKey, 0, 3);
        layout.Controls.Add(buttonPanel, 0, 4);
        Controls.Add(layout);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(560, 190);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "OpenAI API Key";
        ResumeLayout(false);
    }
}
