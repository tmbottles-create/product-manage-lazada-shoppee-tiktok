namespace ShopeeSellerUploader.App.Forms;

partial class CategoryMappingForm
{
    private System.ComponentModel.IContainer components = null!;
    private DataGridView dgvMappings = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;
    private Button btnImportLazadaSheets = null!;
    private TableLayoutPanel layout = null!;
    private Label lblHelp = null!;

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
        dgvMappings = new DataGridView();
        btnSave = new Button();
        btnCancel = new Button();
        btnImportLazadaSheets = new Button();
        layout = new TableLayoutPanel();
        lblHelp = new Label();
        ((System.ComponentModel.ISupportInitialize)dgvMappings).BeginInit();
        SuspendLayout();

        layout.ColumnCount = 2;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
        layout.RowCount = 3;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.Dock = DockStyle.Fill;
        layout.Padding = new Padding(12);

        lblHelp.AutoSize = true;
        lblHelp.Dock = DockStyle.Fill;
        lblHelp.Text = "Map Lazada, Shopee, and TikTok categories here. TikTok Category should match the TikTok template.";
        lblHelp.TextAlign = ContentAlignment.MiddleLeft;

        dgvMappings.AllowUserToAddRows = false;
        dgvMappings.AllowUserToDeleteRows = false;
        dgvMappings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvMappings.Dock = DockStyle.Fill;
        dgvMappings.EditMode = DataGridViewEditMode.EditOnEnter;
        dgvMappings.RowHeadersVisible = false;
        dgvMappings.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "ProductCategory",
            HeaderText = "Product Category",
            ReadOnly = true
        });
        dgvMappings.Columns.Add(new DataGridViewComboBoxColumn
        {
            Name = "LazadaSheetColumn",
            DataPropertyName = "LazadaSheetName",
            HeaderText = "Lazada Sheet"
        });
        dgvMappings.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ShopeeCategoryColumn",
            DataPropertyName = "ShopeeCategoryCode",
            HeaderText = "Shopee Category ID (Paste)"
        });
        dgvMappings.Columns.Add(new DataGridViewComboBoxColumn
        {
            Name = "TikTokCategoryColumn",
            DataPropertyName = "TikTokCategoryName",
            HeaderText = "TikTok Category"
        });

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        btnSave.Text = "Save";
        btnSave.Width = 100;
        btnSave.Click += btnSave_Click;
        btnCancel.Text = "Cancel";
        btnCancel.Width = 100;
        btnCancel.Click += btnCancel_Click;
        btnImportLazadaSheets.Text = "Import Lazada Sheets";
        btnImportLazadaSheets.Width = 160;
        btnImportLazadaSheets.Click += btnImportLazadaSheets_Click;
        panel.Controls.Add(btnCancel);
        panel.Controls.Add(btnSave);
        panel.Controls.Add(btnImportLazadaSheets);

        layout.Controls.Add(lblHelp, 0, 0);
        layout.Controls.Add(dgvMappings, 0, 1);
        layout.Controls.Add(panel, 1, 2);
        layout.SetColumnSpan(lblHelp, 2);
        layout.SetColumnSpan(dgvMappings, 2);
        Controls.Add(layout);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1180, 520);
        MinimumSize = new Size(980, 420);
        Text = "Category Mapping";
        ((System.ComponentModel.ISupportInitialize)dgvMappings).EndInit();
        ResumeLayout(false);
    }
}
