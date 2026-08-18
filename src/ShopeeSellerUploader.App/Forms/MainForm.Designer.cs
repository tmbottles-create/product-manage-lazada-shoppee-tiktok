namespace ShopeeSellerUploader.App.Forms;

partial class ProductWorkspaceForm
{
    private System.ComponentModel.IContainer components = null!;
    private TableLayoutPanel rootLayout = null!;
    private Button btnSelectAll = null!;
    private Button btnAdd = null!;
    private Button btnCopy = null!;
    private Button btnEdit = null!;
    private Button btnDelete = null!;
    private Button btnRefresh = null!;
    private Button btnExportShopee = null!;
    private Button btnOpenShopeeUpload = null!;
    private Button btnExportLazada = null!;
    private Button btnExportTikTok = null!;
    private Button btnOpenTikTokUpload = null!;
    private Button btnUploadLazadaImages = null!;
    private DataGridView dgvProducts = null!;
    private TextBox txtLog = null!;
    private Label lblSummary = null!;

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
        rootLayout = new TableLayoutPanel();
        btnSelectAll = new Button();
        btnAdd = new Button();
        btnCopy = new Button();
        btnEdit = new Button();
        btnDelete = new Button();
        btnRefresh = new Button();
        btnExportShopee = new Button();
        btnOpenShopeeUpload = new Button();
        btnExportLazada = new Button();
        btnExportTikTok = new Button();
        btnOpenTikTokUpload = new Button();
        btnUploadLazadaImages = new Button();
        dgvProducts = new DataGridView();
        txtLog = new TextBox();
        lblSummary = new Label();
        ((System.ComponentModel.ISupportInitialize)dgvProducts).BeginInit();
        SuspendLayout();
        rootLayout.ColumnCount = 12;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.BackColor = Color.FromArgb(248, 250, 252);
        rootLayout.Padding = new Padding(10);
        rootLayout.RowCount = 4;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));
        rootLayout.Controls.Add(btnSelectAll, 0, 0);
        rootLayout.Controls.Add(btnRefresh, 1, 0);
        rootLayout.Controls.Add(btnAdd, 2, 0);
        rootLayout.Controls.Add(btnCopy, 3, 0);
        rootLayout.Controls.Add(btnEdit, 4, 0);
        rootLayout.Controls.Add(btnDelete, 5, 0);
        rootLayout.Controls.Add(btnExportShopee, 6, 0);
        rootLayout.Controls.Add(btnOpenShopeeUpload, 7, 0);
        rootLayout.Controls.Add(btnExportLazada, 8, 0);
        rootLayout.Controls.Add(btnExportTikTok, 9, 0);
        rootLayout.Controls.Add(btnOpenTikTokUpload, 10, 0);
        rootLayout.Controls.Add(btnUploadLazadaImages, 11, 0);
        rootLayout.Controls.Add(dgvProducts, 0, 1);
        rootLayout.Controls.Add(lblSummary, 0, 2);
        rootLayout.Controls.Add(txtLog, 0, 3);
        rootLayout.SetColumnSpan(dgvProducts, 12);
        rootLayout.SetColumnSpan(lblSummary, 12);
        rootLayout.SetColumnSpan(txtLog, 12);
        Controls.Add(rootLayout);

        btnSelectAll.Text = "Select All";
        btnSelectAll.Dock = DockStyle.Left;
        btnSelectAll.Width = 120;
        btnSelectAll.Click += btnSelectAll_Click;
        btnAdd.Text = "Add";
        btnAdd.Dock = DockStyle.Fill;
        btnAdd.Click += btnAdd_Click;
        btnCopy.Text = "Copy";
        btnCopy.Dock = DockStyle.Fill;
        btnCopy.Click += btnCopy_Click;
        btnEdit.Text = "Edit";
        btnEdit.Dock = DockStyle.Fill;
        btnEdit.Click += btnEdit_Click;
        btnDelete.Text = "Delete";
        btnDelete.Dock = DockStyle.Fill;
        btnDelete.Click += btnDelete_Click;
        btnRefresh.Text = "Refresh";
        btnRefresh.Dock = DockStyle.Fill;
        btnRefresh.Click += btnRefresh_Click;
        btnExportShopee.Text = "Export Shopee";
        btnExportShopee.Dock = DockStyle.Fill;
        btnExportShopee.Click += btnExportShopee_Click;
        btnOpenShopeeUpload.Text = "Open Shopee File";
        btnOpenShopeeUpload.Dock = DockStyle.Fill;
        btnOpenShopeeUpload.Click += btnOpenShopeeUpload_Click;
        btnExportLazada.Text = "Export Lazada";
        btnExportLazada.Dock = DockStyle.Fill;
        btnExportLazada.Click += btnExportLazada_Click;
        btnExportTikTok.Text = "Export TikTok";
        btnExportTikTok.Dock = DockStyle.Fill;
        btnExportTikTok.Click += btnExportTikTok_Click;
        btnOpenTikTokUpload.Text = "Open TikTok File";
        btnOpenTikTokUpload.Dock = DockStyle.Fill;
        btnOpenTikTokUpload.Click += btnOpenTikTokUpload_Click;
        btnUploadLazadaImages.Text = "Upload Image for Lazada";
        btnUploadLazadaImages.Dock = DockStyle.Fill;
        btnUploadLazadaImages.Click += btnUploadLazadaImages_Click;

        dgvProducts.BackgroundColor = Color.White;
        dgvProducts.BorderStyle = BorderStyle.None;
        dgvProducts.AllowUserToAddRows = false;
        dgvProducts.AllowUserToDeleteRows = false;
        dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvProducts.Dock = DockStyle.Fill;
        dgvProducts.MultiSelect = false;
        dgvProducts.ReadOnly = false;
        dgvProducts.RowHeadersVisible = false;
        dgvProducts.RowTemplate.Height = 72;
        dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvProducts.CellClick += dgvProducts_CellClick;
        dgvProducts.CellValueChanged += dgvProducts_CellValueChanged;
        dgvProducts.CellDoubleClick += dgvProducts_CellDoubleClick;
        dgvProducts.CurrentCellDirtyStateChanged += dgvProducts_CurrentCellDirtyStateChanged;
        dgvProducts.DataError += dgvProducts_DataError;
        dgvProducts.ColumnHeaderMouseClick += dgvProducts_ColumnHeaderMouseClick;
        dgvProducts.Columns.Add(new DataGridViewCheckBoxColumn { Name = "SelectColumn", DataPropertyName = "Selected", HeaderText = "Select All", Width = 70 });
        dgvProducts.Columns.Add(new DataGridViewImageColumn
        {
            Name = "Thumbnail",
            DataPropertyName = "Thumbnail",
            HeaderText = "Image",
            ReadOnly = true,
            Width = 90,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            ImageLayout = DataGridViewImageCellLayout.Zoom
        });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductCode", HeaderText = "Product Code", ReadOnly = true });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product Name", ReadOnly = true });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "Category", ReadOnly = true });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Brand", HeaderText = "Brand", ReadOnly = true });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "Price", ReadOnly = true });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Stock", HeaderText = "Stock", ReadOnly = true });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SKU", HeaderText = "SKU", ReadOnly = true });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UpdatedAt", HeaderText = "Updated", ReadOnly = true });

        lblSummary.AutoSize = true;
        lblSummary.Text = "Products: 0 total | Selected: 0 | Mappings: 0";
        txtLog.Dock = DockStyle.Fill;
        txtLog.Multiline = true;
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1610, 820);
        MinimumSize = new Size(1200, 720);
        Name = "ProductWorkspaceForm";
        Text = "Product Workspace";
        ((System.ComponentModel.ISupportInitialize)dgvProducts).EndInit();
        ResumeLayout(false);
    }
}
