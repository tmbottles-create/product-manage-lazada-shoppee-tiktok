namespace ShopeeSellerUploader.App.Forms;

partial class MdiMainForm
{
    private System.ComponentModel.IContainer components = null!;
    private MenuStrip menuMain = null!;
    private ToolStripMenuItem menuProduct = null!;
    private ToolStripMenuItem menuProductList = null!;
    private ToolStripMenuItem menuProductAdd = null!;
    private ToolStripMenuItem menuProductCopy = null!;
    private ToolStripMenuItem menuProductRefresh = null!;
    private ToolStripMenuItem menuMaster = null!;
    private ToolStripMenuItem menuMasterCategory = null!;
    private ToolStripMenuItem menuMasterLazadaCategory = null!;
    private ToolStripMenuItem menuMasterShopeeCategory = null!;
    private ToolStripMenuItem menuMasterTikTokCategory = null!;
    private ToolStripMenuItem menuExport = null!;
    private ToolStripMenuItem menuExportShopee = null!;
    private ToolStripMenuItem menuExportLazada = null!;
    private ToolStripMenuItem menuExportTikTok = null!;
    private ToolStripMenuItem menuConfig = null!;
    private ToolStripMenuItem menuConfigApiKey = null!;
    private ToolStripMenuItem menuConfigImageKitSetup = null!;
    private ToolStripMenuItem menuConfigDatabase = null!;
    private ToolStripMenuItem menuConfigTemplates = null!;
    private ToolStripMenuItem menuConfigExports = null!;
    private ToolStripMenuItem menuConfigLazadaImageMode = null!;
    private ToolStripMenuItem menuConfigCheckPrices = null!;
    private ToolStripMenuItem menuWindow = null!;
    private ToolStripMenuItem menuWindowCascade = null!;
    private ToolStripMenuItem menuWindowTileHorizontal = null!;
    private ToolStripMenuItem menuWindowTileVertical = null!;
    private Panel headerPanel = null!;
    private Label lblHeaderTitle = null!;
    private Label lblHeaderSubtitle = null!;
    private StatusStrip statusMain = null!;
    private ToolStripStatusLabel toolStripStatusLabel = null!;

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
        components = new System.ComponentModel.Container();
        menuMain = new MenuStrip();
        menuProduct = new ToolStripMenuItem();
        menuProductList = new ToolStripMenuItem();
        menuProductAdd = new ToolStripMenuItem();
        menuProductCopy = new ToolStripMenuItem();
        menuProductRefresh = new ToolStripMenuItem();
        menuMaster = new ToolStripMenuItem();
        menuMasterCategory = new ToolStripMenuItem();
        menuMasterLazadaCategory = new ToolStripMenuItem();
        menuMasterShopeeCategory = new ToolStripMenuItem();
        menuMasterTikTokCategory = new ToolStripMenuItem();
        menuExport = new ToolStripMenuItem();
        menuExportShopee = new ToolStripMenuItem();
        menuExportLazada = new ToolStripMenuItem();
        menuExportTikTok = new ToolStripMenuItem();
        menuConfig = new ToolStripMenuItem();
        menuConfigApiKey = new ToolStripMenuItem();
        menuConfigImageKitSetup = new ToolStripMenuItem();
        menuConfigDatabase = new ToolStripMenuItem();
        menuConfigTemplates = new ToolStripMenuItem();
        menuConfigExports = new ToolStripMenuItem();
        menuConfigLazadaImageMode = new ToolStripMenuItem();
        menuConfigCheckPrices = new ToolStripMenuItem();
        menuWindow = new ToolStripMenuItem();
        menuWindowCascade = new ToolStripMenuItem();
        menuWindowTileHorizontal = new ToolStripMenuItem();
        menuWindowTileVertical = new ToolStripMenuItem();
        headerPanel = new Panel();
        lblHeaderTitle = new Label();
        lblHeaderSubtitle = new Label();
        statusMain = new StatusStrip();
        toolStripStatusLabel = new ToolStripStatusLabel();
        menuMain.SuspendLayout();
        headerPanel.SuspendLayout();
        statusMain.SuspendLayout();
        SuspendLayout();
        // 
        // menuMain
        // 
        menuMain.BackColor = Color.White;
        menuMain.Font = new Font("Segoe UI", 10F);
        menuMain.ImageScalingSize = new Size(20, 20);
        menuMain.Items.AddRange(new ToolStripItem[] { menuProduct, menuMaster, menuExport, menuConfig, menuWindow });
        menuMain.Location = new Point(0, 0);
        menuMain.Name = "menuMain";
        menuMain.Padding = new Padding(10, 8, 10, 8);
        menuMain.Size = new Size(1600, 39);
        menuMain.TabIndex = 0;
        menuMain.Text = "menuStrip1";
        // 
        // menuProduct
        // 
        menuProduct.DropDownItems.AddRange(new ToolStripItem[] { menuProductList, menuProductAdd, menuProductCopy, menuProductRefresh });
        menuProduct.Name = "menuProduct";
        menuProduct.Size = new Size(71, 23);
        menuProduct.Text = "Product";
        // 
        // menuProductList
        // 
        menuProductList.Name = "menuProductList";
        menuProductList.Size = new Size(157, 24);
        menuProductList.Text = "Product List";
        menuProductList.Click += menuProductList_Click;
        // 
        // menuProductAdd
        // 
        menuProductAdd.Name = "menuProductAdd";
        menuProductAdd.Size = new Size(157, 24);
        menuProductAdd.Text = "Add Product";
        menuProductAdd.Click += menuProductAdd_Click;
        // 
        // menuProductCopy
        // 
        menuProductCopy.Name = "menuProductCopy";
        menuProductCopy.Size = new Size(157, 24);
        menuProductCopy.Text = "Copy Product";
        menuProductCopy.Click += menuProductCopy_Click;
        // 
        // menuProductRefresh
        // 
        menuProductRefresh.Name = "menuProductRefresh";
        menuProductRefresh.Size = new Size(157, 24);
        menuProductRefresh.Text = "Refresh";
        menuProductRefresh.Click += menuProductRefresh_Click;
        // 
        // menuMaster
        // 
        menuMaster.DropDownItems.AddRange(new ToolStripItem[] { menuMasterCategory, menuMasterLazadaCategory, menuMasterShopeeCategory, menuMasterTikTokCategory });
        menuMaster.Name = "menuMaster";
        menuMaster.Size = new Size(65, 23);
        menuMaster.Text = "Master";
        // 
        // menuMasterCategory
        // 
        menuMasterCategory.Name = "menuMasterCategory";
        menuMasterCategory.Size = new Size(191, 24);
        menuMasterCategory.Text = "Category Mapping";
        menuMasterCategory.Click += menuMasterCategory_Click;
        // 
        // menuMasterLazadaCategory
        // 
        menuMasterLazadaCategory.Name = "menuMasterLazadaCategory";
        menuMasterLazadaCategory.Size = new Size(191, 24);
        menuMasterLazadaCategory.Text = "Lazada Cat Master";
        menuMasterLazadaCategory.Click += menuMasterLazadaCategory_Click;
        // 
        // menuMasterShopeeCategory
        // 
        menuMasterShopeeCategory.Name = "menuMasterShopeeCategory";
        menuMasterShopeeCategory.Size = new Size(191, 24);
        menuMasterShopeeCategory.Text = "Shopee Cat Master";
        menuMasterShopeeCategory.Click += menuMasterShopeeCategory_Click;
        // 
        // menuMasterTikTokCategory
        // 
        menuMasterTikTokCategory.Name = "menuMasterTikTokCategory";
        menuMasterTikTokCategory.Size = new Size(191, 24);
        menuMasterTikTokCategory.Text = "TikTok Cat Master";
        menuMasterTikTokCategory.Click += menuMasterTikTokCategory_Click;
        // 
        // menuExport
        // 
        menuExport.DropDownItems.AddRange(new ToolStripItem[] { menuExportShopee, menuExportLazada, menuExportTikTok });
        menuExport.Name = "menuExport";
        menuExport.Size = new Size(62, 23);
        menuExport.Text = "Export";
        // 
        // menuExportShopee
        // 
        menuExportShopee.Name = "menuExportShopee";
        menuExportShopee.Size = new Size(168, 24);
        menuExportShopee.Text = "Export Shopee";
        menuExportShopee.Click += menuExportShopee_Click;
        // 
        // menuExportLazada
        // 
        menuExportLazada.Name = "menuExportLazada";
        menuExportLazada.Size = new Size(168, 24);
        menuExportLazada.Text = "Export Lazada";
        menuExportLazada.Click += menuExportLazada_Click;
        // 
        // menuExportTikTok
        // 
        menuExportTikTok.Name = "menuExportTikTok";
        menuExportTikTok.Size = new Size(168, 24);
        menuExportTikTok.Text = "Export TikTok";
        menuExportTikTok.Click += menuExportTikTok_Click;
        // 
        // menuConfig
        // 
        menuConfig.DropDownItems.AddRange(new ToolStripItem[] { menuConfigApiKey, menuConfigImageKitSetup, menuConfigDatabase, menuConfigTemplates, menuConfigExports, menuConfigLazadaImageMode, menuConfigCheckPrices });
        menuConfig.Name = "menuConfig";
        menuConfig.Size = new Size(61, 23);
        menuConfig.Text = "Config";
        // 
        // menuConfigApiKey
        // 
        menuConfigApiKey.Name = "menuConfigApiKey";
        menuConfigApiKey.Size = new Size(171, 24);
        menuConfigApiKey.Text = "OpenAI API Key";
        menuConfigApiKey.Click += menuConfigApiKey_Click;
        // 
        // menuConfigImageKitSetup
        // 
        menuConfigImageKitSetup.Name = "menuConfigImageKitSetup";
        menuConfigImageKitSetup.Size = new Size(198, 24);
        menuConfigImageKitSetup.Text = "ImageKit Setup";
        menuConfigImageKitSetup.Click += menuConfigImageKitSetup_Click;
        // 
        // menuConfigDatabase
        // 
        menuConfigDatabase.Name = "menuConfigDatabase";
        menuConfigDatabase.Size = new Size(198, 24);
        menuConfigDatabase.Text = "Database Path";
        menuConfigDatabase.Click += menuConfigDatabase_Click;
        // 
        // menuConfigTemplates
        // 
        menuConfigTemplates.Name = "menuConfigTemplates";
        menuConfigTemplates.Size = new Size(198, 24);
        menuConfigTemplates.Text = "Template Path";
        menuConfigTemplates.Click += menuConfigTemplates_Click;
        // 
        // menuConfigExports
        // 
        menuConfigExports.Name = "menuConfigExports";
        menuConfigExports.Size = new Size(198, 24);
        menuConfigExports.Text = "Export Path";
        menuConfigExports.Click += menuConfigExports_Click;
        // 
        // menuConfigLazadaImageMode
        // 
        menuConfigLazadaImageMode.Name = "menuConfigLazadaImageMode";
        menuConfigLazadaImageMode.Size = new Size(201, 24);
        menuConfigLazadaImageMode.Text = "Lazada Image Mode";
        menuConfigLazadaImageMode.Click += menuConfigLazadaImageMode_Click;
        // 
        // menuConfigCheckPrices
        // 
        menuConfigCheckPrices.Name = "menuConfigCheckPrices";
        menuConfigCheckPrices.Size = new Size(201, 24);
        menuConfigCheckPrices.Text = "Check Web Prices";
        menuConfigCheckPrices.Click += menuConfigCheckPrices_Click;
        // 
        // menuWindow
        // 
        menuWindow.DropDownItems.AddRange(new ToolStripItem[] { menuWindowCascade, menuWindowTileHorizontal, menuWindowTileVertical });
        menuWindow.Name = "menuWindow";
        menuWindow.Size = new Size(75, 23);
        menuWindow.Text = "Window";
        // 
        // menuWindowCascade
        // 
        menuWindowCascade.Name = "menuWindowCascade";
        menuWindowCascade.Size = new Size(184, 24);
        menuWindowCascade.Text = "Cascade";
        menuWindowCascade.Click += menuWindowCascade_Click;
        // 
        // menuWindowTileHorizontal
        // 
        menuWindowTileHorizontal.Name = "menuWindowTileHorizontal";
        menuWindowTileHorizontal.Size = new Size(184, 24);
        menuWindowTileHorizontal.Text = "Tile Horizontal";
        menuWindowTileHorizontal.Click += menuWindowTileHorizontal_Click;
        // 
        // menuWindowTileVertical
        // 
        menuWindowTileVertical.Name = "menuWindowTileVertical";
        menuWindowTileVertical.Size = new Size(184, 24);
        menuWindowTileVertical.Text = "Tile Vertical";
        menuWindowTileVertical.Click += menuWindowTileVertical_Click;
        // 
        // headerPanel
        // 
        headerPanel.BackColor = Color.FromArgb(18, 67, 160);
        headerPanel.Controls.Add(lblHeaderSubtitle);
        headerPanel.Controls.Add(lblHeaderTitle);
        headerPanel.Dock = DockStyle.Top;
        headerPanel.Location = new Point(0, 39);
        headerPanel.Name = "headerPanel";
        headerPanel.Padding = new Padding(24, 18, 24, 18);
        headerPanel.Size = new Size(1600, 96);
        headerPanel.TabIndex = 1;
        // 
        // lblHeaderTitle
        // 
        lblHeaderTitle.AutoSize = true;
        lblHeaderTitle.Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold);
        lblHeaderTitle.ForeColor = Color.White;
        lblHeaderTitle.Location = new Point(24, 14);
        lblHeaderTitle.Name = "lblHeaderTitle";
        lblHeaderTitle.Size = new Size(385, 37);
        lblHeaderTitle.TabIndex = 0;
        lblHeaderTitle.Text = "Shopee Lazada Product Studio";
        // 
        // lblHeaderSubtitle
        // 
        lblHeaderSubtitle.AutoSize = true;
        lblHeaderSubtitle.Font = new Font("Segoe UI", 10F);
        lblHeaderSubtitle.ForeColor = Color.FromArgb(220, 232, 255);
        lblHeaderSubtitle.Location = new Point(26, 56);
        lblHeaderSubtitle.Name = "lblHeaderSubtitle";
        lblHeaderSubtitle.Size = new Size(426, 19);
        lblHeaderSubtitle.TabIndex = 1;
        lblHeaderSubtitle.Text = "Manage products, category mapping, and marketplace export in one place.";
        // 
        // statusMain
        // 
        statusMain.ImageScalingSize = new Size(20, 20);
        statusMain.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel });
        statusMain.Location = new Point(0, 878);
        statusMain.Name = "statusMain";
        statusMain.Padding = new Padding(1, 0, 18, 0);
        statusMain.Size = new Size(1600, 22);
        statusMain.TabIndex = 2;
        statusMain.Text = "statusStrip1";
        // 
        // toolStripStatusLabel
        // 
        toolStripStatusLabel.Name = "toolStripStatusLabel";
        toolStripStatusLabel.Size = new Size(144, 17);
        toolStripStatusLabel.Text = "Ready to manage products.";
        // 
        // MdiMainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(241, 245, 249);
        ClientSize = new Size(1600, 900);
        Controls.Add(statusMain);
        Controls.Add(headerPanel);
        Controls.Add(menuMain);
        IsMdiContainer = true;
        MainMenuStrip = menuMain;
        MinimumSize = new Size(1280, 780);
        Name = "MdiMainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Shopee Lazada Product Manager";
        WindowState = FormWindowState.Maximized;
        menuMain.ResumeLayout(false);
        menuMain.PerformLayout();
        headerPanel.ResumeLayout(false);
        headerPanel.PerformLayout();
        statusMain.ResumeLayout(false);
        statusMain.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
