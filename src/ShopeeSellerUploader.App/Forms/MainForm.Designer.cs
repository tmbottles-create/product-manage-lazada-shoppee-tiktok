namespace ShopeeSellerUploader.App.Forms;

partial class ProductWorkspaceForm
{
    private System.ComponentModel.IContainer components = null!;
    private TableLayoutPanel rootLayout = null!;
    private TableLayoutPanel topBarLayout = null!;
    private Panel pnlSearchCard = null!;
    private Label lblSearchTitle = null!;
    private Label lblCategorySearch = null!;
    private ComboBox cboCategorySearch = null!;
    private Label lblNameSearch = null!;
    private TextBox txtNameSearch = null!;
    private Button btnSearch = null!;
    private Button btnClearSearch = null!;
    private FlowLayoutPanel pnlActionButtons = null!;
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
        topBarLayout = new TableLayoutPanel();
        pnlSearchCard = new Panel();
        lblSearchTitle = new Label();
        lblCategorySearch = new Label();
        cboCategorySearch = new ComboBox();
        lblNameSearch = new Label();
        txtNameSearch = new TextBox();
        btnSearch = new Button();
        btnClearSearch = new Button();
        pnlActionButtons = new FlowLayoutPanel();
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
        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.BackColor = Color.FromArgb(248, 250, 252);
        rootLayout.Padding = new Padding(14, 12, 14, 12);
        rootLayout.RowCount = 5;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 132F));
        rootLayout.Controls.Add(topBarLayout, 0, 0);
        rootLayout.Controls.Add(pnlActionButtons, 0, 1);
        rootLayout.Controls.Add(dgvProducts, 0, 2);
        rootLayout.Controls.Add(lblSummary, 0, 3);
        rootLayout.Controls.Add(txtLog, 0, 4);
        Controls.Add(rootLayout);

        topBarLayout.ColumnCount = 2;
        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230F));
        topBarLayout.Dock = DockStyle.Fill;
        topBarLayout.Margin = new Padding(0, 0, 0, 12);
        topBarLayout.Controls.Add(pnlSearchCard, 0, 0);

        pnlSearchCard.Dock = DockStyle.Fill;
        pnlSearchCard.BackColor = Color.White;
        pnlSearchCard.Padding = new Padding(16, 12, 16, 12);
        pnlSearchCard.Margin = new Padding(0, 0, 12, 0);
        pnlSearchCard.BorderStyle = BorderStyle.FixedSingle;
        pnlSearchCard.Controls.Add(lblSearchTitle);
        pnlSearchCard.Controls.Add(lblCategorySearch);
        pnlSearchCard.Controls.Add(cboCategorySearch);
        pnlSearchCard.Controls.Add(lblNameSearch);
        pnlSearchCard.Controls.Add(txtNameSearch);
        pnlSearchCard.Controls.Add(btnSearch);
        pnlSearchCard.Controls.Add(btnClearSearch);

        lblSearchTitle.AutoSize = true;
        lblSearchTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        lblSearchTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblSearchTitle.Location = new Point(16, 12);
        lblSearchTitle.Text = "Search Products";

        lblCategorySearch.AutoSize = true;
        lblCategorySearch.ForeColor = Color.FromArgb(71, 85, 105);
        lblCategorySearch.Location = new Point(16, 46);
        lblCategorySearch.Text = "Category";

        cboCategorySearch.Location = new Point(16, 64);
        cboCategorySearch.Size = new Size(260, 23);
        cboCategorySearch.DropDownStyle = ComboBoxStyle.DropDownList;

        lblNameSearch.AutoSize = true;
        lblNameSearch.ForeColor = Color.FromArgb(71, 85, 105);
        lblNameSearch.Location = new Point(292, 46);
        lblNameSearch.Text = "Product Name";

        txtNameSearch.Location = new Point(292, 64);
        txtNameSearch.Size = new Size(320, 23);
        txtNameSearch.PlaceholderText = "Filter by product name";

        btnSearch.Text = "Search";
        btnSearch.Location = new Point(628, 61);
        btnSearch.Size = new Size(94, 30);
        btnSearch.Click += btnSearch_Click;

        btnClearSearch.Text = "Clear";
        btnClearSearch.Location = new Point(730, 61);
        btnClearSearch.Size = new Size(94, 30);
        btnClearSearch.Click += btnClearSearch_Click;

        pnlActionButtons.Dock = DockStyle.Fill;
        pnlActionButtons.WrapContents = false;
        pnlActionButtons.AutoScroll = true;
        pnlActionButtons.Margin = new Padding(0, 2, 0, 10);
        pnlActionButtons.Padding = new Padding(4, 0, 0, 0);
        pnlActionButtons.Controls.Add(btnRefresh);
        pnlActionButtons.Controls.Add(btnAdd);
        pnlActionButtons.Controls.Add(btnCopy);
        pnlActionButtons.Controls.Add(btnEdit);
        pnlActionButtons.Controls.Add(btnDelete);
        pnlActionButtons.Controls.Add(btnExportShopee);
        pnlActionButtons.Controls.Add(btnOpenShopeeUpload);
        pnlActionButtons.Controls.Add(btnExportLazada);
        pnlActionButtons.Controls.Add(btnExportTikTok);
        pnlActionButtons.Controls.Add(btnOpenTikTokUpload);
        pnlActionButtons.Controls.Add(btnUploadLazadaImages);

        btnSelectAll.Text = "Select All";
        btnSelectAll.Width = 116;
        btnSelectAll.Click += btnSelectAll_Click;
        btnAdd.Text = "Add";
        btnAdd.Width = 104;
        btnAdd.Click += btnAdd_Click;
        btnCopy.Text = "Copy";
        btnCopy.Width = 104;
        btnCopy.Click += btnCopy_Click;
        btnEdit.Text = "Edit";
        btnEdit.Width = 104;
        btnEdit.Click += btnEdit_Click;
        btnDelete.Text = "Delete";
        btnDelete.Width = 104;
        btnDelete.Click += btnDelete_Click;
        btnRefresh.Text = "Refresh";
        btnRefresh.Width = 104;
        btnRefresh.Click += btnRefresh_Click;
        btnExportShopee.Text = "Export Shopee";
        btnExportShopee.Width = 132;
        btnExportShopee.Click += btnExportShopee_Click;
        btnOpenShopeeUpload.Text = "Open Shopee File";
        btnOpenShopeeUpload.Width = 136;
        btnOpenShopeeUpload.Click += btnOpenShopeeUpload_Click;
        btnExportLazada.Text = "Export Lazada";
        btnExportLazada.Width = 132;
        btnExportLazada.Click += btnExportLazada_Click;
        btnExportTikTok.Text = "Export TikTok";
        btnExportTikTok.Width = 132;
        btnExportTikTok.Click += btnExportTikTok_Click;
        btnOpenTikTokUpload.Text = "Open TikTok File";
        btnOpenTikTokUpload.Width = 136;
        btnOpenTikTokUpload.Click += btnOpenTikTokUpload_Click;
        btnUploadLazadaImages.Text = "Upload Image for Lazada";
        btnUploadLazadaImages.Width = 168;
        btnUploadLazadaImages.Click += btnUploadLazadaImages_Click;

        dgvProducts.BackgroundColor = Color.White;
        dgvProducts.BorderStyle = BorderStyle.None;
        dgvProducts.AllowUserToAddRows = false;
        dgvProducts.AllowUserToDeleteRows = false;
        dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvProducts.Dock = DockStyle.Fill;
        dgvProducts.Margin = new Padding(0);
        dgvProducts.MultiSelect = false;
        dgvProducts.ReadOnly = false;
        dgvProducts.RowHeadersVisible = false;
        dgvProducts.RowTemplate.Height = 72;
        dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvProducts.EnableHeadersVisualStyles = false;
        dgvProducts.GridColor = Color.FromArgb(226, 232, 240);
        dgvProducts.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        dgvProducts.ColumnHeadersHeight = 36;
        dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
        dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
        dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        dgvProducts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        dgvProducts.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        dgvProducts.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
        dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        dgvProducts.CellClick += dgvProducts_CellClick;
        dgvProducts.CellValueChanged += dgvProducts_CellValueChanged;
        dgvProducts.CellDoubleClick += dgvProducts_CellDoubleClick;
        dgvProducts.CurrentCellDirtyStateChanged += dgvProducts_CurrentCellDirtyStateChanged;
        dgvProducts.DataError += dgvProducts_DataError;
        dgvProducts.ColumnHeaderMouseClick += dgvProducts_ColumnHeaderMouseClick;
        dgvProducts.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "SelectColumn",
            DataPropertyName = "Selected",
            HeaderText = string.Empty,
            Width = 78,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        });
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "NumberColumn",
            DataPropertyName = "RowNumber",
            HeaderText = "No.",
            ReadOnly = true,
            Width = 52,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        });
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
        lblSummary.Dock = DockStyle.Fill;
        lblSummary.Padding = new Padding(0, 4, 0, 0);
        lblSummary.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        lblSummary.ForeColor = Color.FromArgb(51, 65, 85);
        txtLog.Dock = DockStyle.Fill;
        txtLog.Multiline = true;
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.BackColor = Color.FromArgb(248, 250, 252);
        txtLog.BorderStyle = BorderStyle.FixedSingle;

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
