namespace ShopeeSellerUploader.App.Forms;

partial class CategoryMappingForm
{
    private System.ComponentModel.IContainer components = null!;
    private DataGridView dgvMappings = null!;
    private Button btnAdd = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;
    private TableLayoutPanel layout = null!;
    private Label lblHelp = null!;
    private FlowLayoutPanel topActionsPanel = null!;
    private FlowLayoutPanel bottomActionsPanel = null!;

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
        btnAdd = new Button();
        btnSave = new Button();
        btnCancel = new Button();
        layout = new TableLayoutPanel();
        lblHelp = new Label();
        topActionsPanel = new FlowLayoutPanel();
        bottomActionsPanel = new FlowLayoutPanel();
        ((System.ComponentModel.ISupportInitialize)dgvMappings).BeginInit();
        SuspendLayout();

        layout.ColumnCount = 1;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowCount = 4;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
        layout.Dock = DockStyle.Fill;
        layout.Padding = new Padding(16, 14, 16, 14);
        layout.BackColor = Color.FromArgb(248, 250, 252);

        lblHelp.AutoSize = true;
        lblHelp.Dock = DockStyle.Fill;
        lblHelp.Text = "Map Lazada, Shopee, and TikTok categories here. TikTok Category should match the TikTok template.";
        lblHelp.TextAlign = ContentAlignment.MiddleLeft;
        lblHelp.Font = new Font("Segoe UI", 9F);

        topActionsPanel.Dock = DockStyle.Fill;
        topActionsPanel.FlowDirection = FlowDirection.LeftToRight;
        topActionsPanel.WrapContents = false;
        topActionsPanel.Margin = new Padding(0, 0, 0, 8);

        bottomActionsPanel.Dock = DockStyle.Fill;
        bottomActionsPanel.FlowDirection = FlowDirection.RightToLeft;
        bottomActionsPanel.WrapContents = false;
        bottomActionsPanel.Margin = new Padding(0, 10, 0, 0);

        dgvMappings.AllowUserToAddRows = false;
        dgvMappings.AllowUserToDeleteRows = false;
        dgvMappings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvMappings.Dock = DockStyle.Fill;
        dgvMappings.EditMode = DataGridViewEditMode.EditOnEnter;
        dgvMappings.BackgroundColor = Color.White;
        dgvMappings.BorderStyle = BorderStyle.None;
        dgvMappings.EnableHeadersVisualStyles = false;
        dgvMappings.ColumnHeadersHeight = 34;
        dgvMappings.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
        dgvMappings.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
        dgvMappings.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        dgvMappings.GridColor = Color.FromArgb(226, 232, 240);
        dgvMappings.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        dgvMappings.RowHeadersVisible = false;
        dgvMappings.RowTemplate.Height = 30;
        dgvMappings.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "ProductCategory",
            HeaderText = "Product Category"
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
        btnAdd.Text = "Add";
        btnAdd.Width = 104;
        btnAdd.Height = 34;
        btnAdd.BackColor = Color.FromArgb(37, 99, 235);
        btnAdd.ForeColor = Color.White;
        btnAdd.FlatStyle = FlatStyle.Flat;
        btnAdd.FlatAppearance.BorderSize = 0;
        btnAdd.Margin = new Padding(0, 0, 8, 0);
        btnAdd.Click += btnAdd_Click;
        btnSave.Text = "Save";
        btnSave.Width = 100;
        btnSave.Height = 34;
        btnSave.BackColor = Color.FromArgb(22, 163, 74);
        btnSave.ForeColor = Color.White;
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Margin = new Padding(8, 0, 0, 0);
        btnSave.Click += btnSave_Click;
        btnCancel.Text = "Cancel";
        btnCancel.Width = 100;
        btnCancel.Height = 34;
        btnCancel.BackColor = Color.White;
        btnCancel.ForeColor = Color.FromArgb(51, 65, 85);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        btnCancel.Margin = new Padding(8, 0, 0, 0);
        btnCancel.Click += btnCancel_Click;
        topActionsPanel.Controls.Add(btnAdd);
        bottomActionsPanel.Controls.Add(btnCancel);
        bottomActionsPanel.Controls.Add(btnSave);

        layout.Controls.Add(lblHelp, 0, 0);
        layout.Controls.Add(topActionsPanel, 0, 1);
        layout.Controls.Add(dgvMappings, 0, 2);
        layout.Controls.Add(bottomActionsPanel, 0, 3);
        Controls.Add(layout);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1180, 560);
        MinimumSize = new Size(980, 460);
        Text = "Category Mapping";
        ((System.ComponentModel.ISupportInitialize)dgvMappings).EndInit();
        ResumeLayout(false);
    }
}
