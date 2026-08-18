namespace ShopeeSellerUploader.App.Forms;

partial class ProductEditForm
{
    private System.ComponentModel.IContainer components = null!;
    private Panel rootPanel = null!;
    private TableLayoutPanel contentLayout = null!;
    private TextBox txtProductCode = null!;
    private TextBox txtProductName = null!;
    private TextBox txtDescription = null!;
    private ComboBox cboCategory = null!;
    private NumericUpDown numPrice = null!;
    private NumericUpDown numStock = null!;
    private NumericUpDown numWeight = null!;
    private NumericUpDown numLength = null!;
    private NumericUpDown numWidth = null!;
    private NumericUpDown numHeight = null!;
    private TextBox txtSku = null!;
    private TextBox txtImage1 = null!;
    private TextBox txtImage2 = null!;
    private TextBox txtImage3 = null!;
    private TextBox txtImage4 = null!;
    private TextBox txtShopeeImage1Url = null!;
    private TextBox txtShopeeImage2Url = null!;
    private TextBox txtShopeeImage3Url = null!;
    private TextBox txtShopeeImage4Url = null!;
    private TextBox txtLazadaImage1Url = null!;
    private TextBox txtLazadaImage2Url = null!;
    private TextBox txtLazadaImage3Url = null!;
    private TextBox txtLazadaImage4Url = null!;
    private TextBox txtVariationName = null!;
    private TextBox txtVariationOption = null!;
    private NumericUpDown numVariationPrice = null!;
    private NumericUpDown numVariationStock = null!;
    private ComboBox cboBrand = null!;
    private ComboBox cboBabyMaterial = null!;
    private ComboBox cboCountryOfOrigin = null!;
    private ComboBox cboWarrantyType = null!;
    private TextBox txtWarrantyPeriod = null!;
    private ComboBox cboColorFamily = null!;
    private ComboBox cboDangerousGoods = null!;
    private ComboBox cboDeliveryStandard = null!;
    private TextBox txtAiHint = null!;
    private TextBox txtAiNotes = null!;
    private DataGridView dgvVariationOptions = null!;
    private Button btnViewImage1 = null!;
    private Button btnViewImage2 = null!;
    private Button btnViewImage3 = null!;
    private Button btnViewImage4 = null!;
    private Button btnUploadImage1 = null!;
    private Button btnUploadImage2 = null!;
    private Button btnUploadImage3 = null!;
    private Button btnUploadImage4 = null!;
    private Button btnAddVariantRow = null!;
    private Button btnRemoveVariantRow = null!;
    private Button btnUploadVariantImage = null!;
    private Button btnUpdateAllVariantPrice = null!;
    private Button btnUpdateAllVariantStock = null!;
    private Button btnAiFill = null!;
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
        rootPanel = new Panel();
        contentLayout = new TableLayoutPanel();
        txtProductCode = new TextBox();
        txtProductName = new TextBox();
        txtDescription = new TextBox();
        cboCategory = new ComboBox();
        numPrice = new NumericUpDown();
        numStock = new NumericUpDown();
        numWeight = new NumericUpDown();
        numLength = new NumericUpDown();
        numWidth = new NumericUpDown();
        numHeight = new NumericUpDown();
        txtSku = new TextBox();
        txtImage1 = new TextBox();
        txtImage2 = new TextBox();
        txtImage3 = new TextBox();
        txtImage4 = new TextBox();
        txtShopeeImage1Url = new TextBox();
        txtShopeeImage2Url = new TextBox();
        txtShopeeImage3Url = new TextBox();
        txtShopeeImage4Url = new TextBox();
        txtLazadaImage1Url = new TextBox();
        txtLazadaImage2Url = new TextBox();
        txtLazadaImage3Url = new TextBox();
        txtLazadaImage4Url = new TextBox();
        txtVariationName = new TextBox();
        txtVariationOption = new TextBox();
        numVariationPrice = new NumericUpDown();
        numVariationStock = new NumericUpDown();
        cboBrand = new ComboBox();
        cboBabyMaterial = new ComboBox();
        cboCountryOfOrigin = new ComboBox();
        cboWarrantyType = new ComboBox();
        txtWarrantyPeriod = new TextBox();
        cboColorFamily = new ComboBox();
        cboDangerousGoods = new ComboBox();
        cboDeliveryStandard = new ComboBox();
        txtAiHint = new TextBox();
        txtAiNotes = new TextBox();
        dgvVariationOptions = new DataGridView();
        btnViewImage1 = new Button();
        btnViewImage2 = new Button();
        btnViewImage3 = new Button();
        btnViewImage4 = new Button();
        btnUploadImage1 = new Button();
        btnUploadImage2 = new Button();
        btnUploadImage3 = new Button();
        btnUploadImage4 = new Button();
        btnAddVariantRow = new Button();
        btnRemoveVariantRow = new Button();
        btnUploadVariantImage = new Button();
        btnUpdateAllVariantPrice = new Button();
        btnUpdateAllVariantStock = new Button();
        btnAiFill = new Button();
        btnSave = new Button();
        btnCancel = new Button();
        ((System.ComponentModel.ISupportInitialize)numPrice).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numStock).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numWeight).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numLength).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numWidth).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numHeight).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numVariationPrice).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numVariationStock).BeginInit();
        SuspendLayout();

        BackColor = Color.FromArgb(243, 246, 251);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1460, 980);
        Font = new Font("Segoe UI", 9.5F);
        MinimumSize = new Size(1180, 840);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Product Editor";
        WindowState = FormWindowState.Maximized;

        rootPanel.Dock = DockStyle.Fill;
        rootPanel.AutoScroll = true;
        rootPanel.Padding = new Padding(12, 10, 12, 10);
        rootPanel.BackColor = Color.FromArgb(243, 246, 251);

        contentLayout.ColumnCount = 1;
        contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        contentLayout.RowCount = 7;
        contentLayout.RowStyles.Add(new RowStyle());
        contentLayout.RowStyles.Add(new RowStyle());
        contentLayout.RowStyles.Add(new RowStyle());
        contentLayout.RowStyles.Add(new RowStyle());
        contentLayout.RowStyles.Add(new RowStyle());
        contentLayout.RowStyles.Add(new RowStyle());
        contentLayout.RowStyles.Add(new RowStyle());
        contentLayout.Dock = DockStyle.Top;
        contentLayout.AutoSize = true;
        contentLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        contentLayout.Margin = new Padding(0);
        contentLayout.Padding = new Padding(0);

        var headerCard = CreateHeaderCard();
        var aiSection = CreateSectionCard("AI Assistant", "Create draft content and save API settings.");
        var coreSection = CreateSectionCard("Core Details", "Main product identity and category mapping.");
        var pricingSection = CreateSectionCard("Pricing && Logistics", "Price, stock, and shipping dimensions.");
        var imageSection = CreateSectionCard("Images", "Local image files, Shopee image URLs, and Lazada public image URLs.");
        var variationSection = CreateSectionCard("Variations && Attributes", "Variation, brand, warranty, and attribute data.");
        var noteSection = CreateSectionCard("AI Notes", "Assistant notes and validation hints.");

        contentLayout.Controls.Add(headerCard, 0, 0);
        contentLayout.Controls.Add(aiSection, 0, 1);
        contentLayout.Controls.Add(coreSection, 0, 2);
        contentLayout.Controls.Add(pricingSection, 0, 3);
        contentLayout.Controls.Add(imageSection, 0, 4);
        contentLayout.Controls.Add(variationSection, 0, 5);
        contentLayout.Controls.Add(noteSection, 0, 6);

        BuildAiSection(GetSectionContent(aiSection));
        BuildCoreSection(GetSectionContent(coreSection));
        BuildPricingSection(GetSectionContent(pricingSection));
        BuildImageSection(GetSectionContent(imageSection));
        BuildVariationSection(GetSectionContent(variationSection));
        BuildNotesSection(GetSectionContent(noteSection));

        rootPanel.Controls.Add(contentLayout);
        Controls.Add(rootPanel);

        ConfigureTextBox(txtAiHint, "Optional hint");
        ConfigureTextBox(txtProductCode);
        ConfigureTextBox(txtProductName);
        ConfigureTextBox(txtSku);
        ConfigureTextBox(txtImage1);
        ConfigureTextBox(txtImage2);
        ConfigureTextBox(txtImage3);
        ConfigureTextBox(txtImage4);
        ConfigureTextBox(txtVariationName);
        ConfigureTextBox(txtWarrantyPeriod);

        txtDescription.Multiline = true;
        txtDescription.ScrollBars = ScrollBars.Vertical;
        ConfigureTextBox(txtDescription);

        txtVariationOption.Multiline = true;
        txtVariationOption.ScrollBars = ScrollBars.Vertical;
        ConfigureTextBox(txtVariationOption, "Red | 199 | 10 | https://...");

        txtAiNotes.Multiline = true;
        txtAiNotes.ReadOnly = true;
        txtAiNotes.ScrollBars = ScrollBars.Vertical;
        ConfigureTextBox(txtAiNotes);

        cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        ConfigureComboBox(cboCategory);

        cboBrand.DropDownStyle = ComboBoxStyle.DropDown;
        ConfigureComboBox(cboBrand);

        cboBabyMaterial.DropDownStyle = ComboBoxStyle.DropDown;
        ConfigureComboBox(cboBabyMaterial);

        cboCountryOfOrigin.DropDownStyle = ComboBoxStyle.DropDown;
        ConfigureComboBox(cboCountryOfOrigin);

        cboWarrantyType.DropDownStyle = ComboBoxStyle.DropDown;
        ConfigureComboBox(cboWarrantyType);

        cboColorFamily.DropDownStyle = ComboBoxStyle.DropDown;
        ConfigureComboBox(cboColorFamily);

        cboDangerousGoods.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDangerousGoods.Items.AddRange(["No", "Yes", "Battery"]);
        ConfigureComboBox(cboDangerousGoods);

        cboDeliveryStandard.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDeliveryStandard.Items.AddRange(["Yes", "No"]);
        ConfigureComboBox(cboDeliveryStandard);

        btnViewImage1.Text = "View";
        btnViewImage1.Click += btnViewImage1_Click;
        btnViewImage2.Text = "View";
        btnViewImage2.Click += btnViewImage2_Click;
        btnViewImage3.Text = "View";
        btnViewImage3.Click += btnViewImage3_Click;
        btnViewImage4.Text = "View";
        btnViewImage4.Click += btnViewImage4_Click;

        btnUploadImage1.Text = "↑";
        btnUploadImage1.Click += btnUploadImage1_Click;
        btnUploadImage2.Text = "↑";
        btnUploadImage2.Click += btnUploadImage2_Click;
        btnUploadImage3.Text = "↑";
        btnUploadImage3.Click += btnUploadImage3_Click;
        btnUploadImage4.Text = "↑";
        btnUploadImage4.Click += btnUploadImage4_Click;
        ConfigureIconButton(btnUploadImage1);
        ConfigureIconButton(btnUploadImage2);
        ConfigureIconButton(btnUploadImage3);
        ConfigureIconButton(btnUploadImage4);
        ConfigureGhostButton(btnViewImage1);
        ConfigureGhostButton(btnViewImage2);
        ConfigureGhostButton(btnViewImage3);
        ConfigureGhostButton(btnViewImage4);
        btnUploadImage1.Text = "Upload";
        btnUploadImage2.Text = "Upload";
        btnUploadImage3.Text = "Upload";
        btnUploadImage4.Text = "Upload";

        btnAddVariantRow.Text = "Add Row";
        btnAddVariantRow.Click += btnAddVariantRow_Click;
        ConfigureSecondaryButton(btnAddVariantRow, 100);

        btnRemoveVariantRow.Text = "Remove Row";
        btnRemoveVariantRow.Click += btnRemoveVariantRow_Click;
        ConfigureSecondaryButton(btnRemoveVariantRow, 120);

        btnUploadVariantImage.Text = "Upload Variant Image";
        btnUploadVariantImage.Click += btnUploadVariantImage_Click;
        ConfigurePrimaryButton(btnUploadVariantImage, 160);

        btnUpdateAllVariantPrice.Text = "Update All Price";
        btnUpdateAllVariantPrice.Click += btnUpdateAllVariantPrice_Click;
        ConfigureSecondaryButton(btnUpdateAllVariantPrice, 140);

        btnUpdateAllVariantStock.Text = "Update All Stock";
        btnUpdateAllVariantStock.Click += btnUpdateAllVariantStock_Click;
        ConfigureSecondaryButton(btnUpdateAllVariantStock, 140);

        btnAiFill.Text = "AI Fill";
        btnAiFill.Click += btnAiFill_Click;
        ConfigurePrimaryButton(btnAiFill, 110);

        btnSave.Text = "Save";
        btnSave.Click += btnSave_Click;
        ConfigurePrimaryButton(btnSave, 110);

        btnCancel.Text = "Cancel";
        btnCancel.Click += btnCancel_Click;
        ConfigureSecondaryButton(btnCancel, 110);

        ConfigureNumeric(numPrice, 9999999, 2);
        numPrice.Minimum = 0.01M;
        numPrice.Value = 1M;
        ConfigureNumeric(numWeight, 9999, 3);
        ConfigureNumeric(numLength, 9999, 3);
        ConfigureNumeric(numWidth, 9999, 3);
        ConfigureNumeric(numHeight, 9999, 3);
        ConfigureNumeric(numVariationPrice, 9999999, 2);
        numStock.Maximum = 999999;
        numVariationStock.Maximum = 999999;
        ConfigureNumericStyle(numStock);
        ConfigureNumericStyle(numVariationStock);

        ConfigureVariationGrid(dgvVariationOptions);

        ((System.ComponentModel.ISupportInitialize)numPrice).EndInit();
        ((System.ComponentModel.ISupportInitialize)numStock).EndInit();
        ((System.ComponentModel.ISupportInitialize)numWeight).EndInit();
        ((System.ComponentModel.ISupportInitialize)numLength).EndInit();
        ((System.ComponentModel.ISupportInitialize)numWidth).EndInit();
        ((System.ComponentModel.ISupportInitialize)numHeight).EndInit();
        ((System.ComponentModel.ISupportInitialize)numVariationPrice).EndInit();
        ((System.ComponentModel.ISupportInitialize)numVariationStock).EndInit();
        ResumeLayout(false);
    }

    private void BuildAiSection(TableLayoutPanel section)
    {
        section.Controls.Add(CreateTwoColumnRow(
            CreateStandardInputBlock("AI Hint", txtAiHint),
            CreateButtonBlock("AI Actions", btnAiFill, btnSave, btnCancel)), 0, 0);
    }

    private void BuildCoreSection(TableLayoutPanel section)
    {
        section.Controls.Add(CreateTwoColumnRow(
            CreateStandardInputBlock("Product Code", txtProductCode),
            CreateStandardInputBlock("SKU", txtSku)), 0, 0);
        section.Controls.Add(CreateTwoColumnRow(
            CreateStandardInputBlock("Category", cboCategory),
            CreateStandardInputBlock("Brand", cboBrand)), 0, 1);
        section.Controls.Add(CreateTwoColumnRow(
            CreateStandardInputBlock("Product Name", txtProductName),
            CreateStandardInputBlock("Baby Material", cboBabyMaterial)), 0, 2);
        section.Controls.Add(CreateTwoColumnRow(
            CreateStandardInputBlock("Country Of Origin", cboCountryOfOrigin),
            CreateStandardInputBlock("Color Family", cboColorFamily)), 0, 3);
        section.Controls.Add(CreateTwoColumnRow(
            CreateStandardInputBlock("Warranty Type", cboWarrantyType),
            CreateSpacer()), 0, 4);
        section.Controls.Add(CreateTwoColumnRow(
            CreateStandardInputBlock("Warranty Period", txtWarrantyPeriod),
            CreateSpacer()), 0, 5);
        section.Controls.Add(CreateFullWidthRow(
            CreateStandardInputBlock("Description", txtDescription, 88)), 0, 6);
    }

    private void BuildPricingSection(TableLayoutPanel section)
    {
        section.Controls.Add(CreateTwoColumnRow(
            CreateStandardInputBlock("Price", numPrice),
            CreateSpacer()), 0, 0);
        section.Controls.Add(CreateTwoColumnRow(
            CreateStandardInputBlock("Weight", numWeight),
            CreateSpacer()), 0, 1);
        section.Controls.Add(CreateTwoColumnRow(
            CreateStandardInputBlock("Length", numLength),
            CreateStandardInputBlock("Width", numWidth)), 0, 2);
        section.Controls.Add(CreateTwoColumnRow(
            CreateStandardInputBlock("Height", numHeight),
            CreateSpacer()), 0, 3);
    }

    private void BuildImageSection(TableLayoutPanel section)
    {
        section.Controls.Add(CreateTwoColumnRow(
            CreateImageBlock("Image 1", txtImage1, btnViewImage1, btnUploadImage1),
            CreateImageBlock("Image 2", txtImage2, btnViewImage2, btnUploadImage2)), 0, 0);
        section.Controls.Add(CreateTwoColumnRow(
            CreateImageBlock("Image 3", txtImage3, btnViewImage3, btnUploadImage3),
            CreateImageBlock("Image 4", txtImage4, btnViewImage4, btnUploadImage4)), 0, 1);
        section.Controls.Add(CreateFullWidthRow(
            CreateSectionSubtitleLabel("Each Upload button sends only that image to ImageKit and reuses the same URL for both Shopee and Lazada.")), 0, 2);
    }

    private void BuildVariationSection(TableLayoutPanel section)
    {
        section.Controls.Add(CreateTwoColumnRow(
            CreateStandardInputBlock("Variation Name", txtVariationName),
            CreateSpacer()), 0, 0);
        section.Controls.Add(CreateTwoColumnRow(
            CreateBulkUpdateBlock("Bulk Price", numVariationPrice, btnUpdateAllVariantPrice),
            CreateBulkUpdateBlock("Bulk Stock", numVariationStock, btnUpdateAllVariantStock)), 0, 1);
        section.Controls.Add(CreateFullWidthRow(
            CreateVariantGridBlock("Variant Properties", dgvVariationOptions, btnAddVariantRow, btnRemoveVariantRow, btnUploadVariantImage)), 0, 2);
        section.Controls.Add(CreateTwoColumnRow(
            CreateStackedFieldBlock("Dangerous Goods", cboDangerousGoods, 58, 220),
            CreateStackedFieldBlock("Delivery Standard", cboDeliveryStandard, 58, 220)), 0, 3);
    }

    private void BuildNotesSection(TableLayoutPanel section)
    {
        section.Controls.Add(CreateFullWidthRow(
            CreateStandardInputBlock("AI Notes", txtAiNotes, 92)), 0, 0);
    }

    private Control CreateHeaderCard()
    {
        var card = CreateCardPanel();
        card.Padding = new Padding(16, 12, 16, 10);

        var layout = CreateVerticalLayout();
        layout.Controls.Add(CreateTitleLabel("Product Editor"), 0, 0);

        card.Controls.Add(layout);
        return card;
    }

    private Panel CreateSectionCard(string title, string subtitle)
    {
        var card = CreateCardPanel();
        card.Padding = new Padding(18, 14, 18, 16);

        var layout = CreateVerticalLayout();
        layout.Controls.Add(CreateSectionTitleLabel(title), 0, 0);
        layout.Controls.Add(CreateSectionSubtitleLabel(subtitle), 0, 1);

        var content = CreateSectionContentTable();
        layout.Controls.Add(content, 0, 2);

        card.Controls.Add(layout);
        card.Tag = content;
        return card;
    }

    private static TableLayoutPanel GetSectionContent(Control section)
    {
        if (section.Tag is TableLayoutPanel table)
        {
            return table;
        }

        throw new InvalidOperationException("Section content table is missing.");
    }

    private static Panel CreateCardPanel()
    {
        return new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 12)
        };
    }

    private static TableLayoutPanel CreateVerticalLayout()
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 1,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        return layout;
    }

    private static TableLayoutPanel CreateSectionContentTable()
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 0,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 12, 0, 0),
            Padding = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        return layout;
    }

    private static TableLayoutPanel CreateTwoColumnRow(Control left, Control right)
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 1,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        left.Margin = new Padding(0, 0, 10, 0);
        right.Margin = new Padding(10, 0, 0, 0);
        left.Dock = DockStyle.Fill;
        right.Dock = DockStyle.Fill;

        layout.Controls.Add(left, 0, 0);
        layout.Controls.Add(right, 1, 0);
        return layout;
    }

    private static TableLayoutPanel CreateFullWidthRow(Control control)
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 1,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        control.Dock = DockStyle.Fill;
        layout.Controls.Add(control, 0, 0);
        return layout;
    }

    private static Panel CreateStandardInputBlock(string labelText, Control control, int height = 38)
    {
        var block = new Panel
        {
            Height = height,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        var label = CreateFieldLabel(labelText);
        label.Location = new Point(0, 8);
        label.Width = 140;

        var inputFrame = WrapControlIfNeeded(control);
        block.Controls.Add(label);
        block.Controls.Add(inputFrame);

        void ApplyLayout()
        {
            const int left = 146;
            inputFrame.Location = new Point(left, 4);
            inputFrame.Size = new Size(Math.Max(120, block.ClientSize.Width - left), control is TextBox { Multiline: true } ? Math.Max(28, height - 8) : 28);
            inputFrame.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        }

        block.Resize += (_, _) => ApplyLayout();
        ApplyLayout();
        return block;
    }

    private static Panel CreateStackedFieldBlock(string labelText, Control control, int height = 58, int controlWidth = 220)
    {
        var block = new Panel
        {
            Height = height,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        var label = CreateFieldLabel(labelText);
        label.Location = new Point(0, 0);
        label.Width = Math.Max(220, block.Width);
        label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        var fieldControl = WrapControlIfNeeded(control);
        fieldControl.Location = new Point(0, 24);
        fieldControl.Size = new Size(controlWidth, 30);
        fieldControl.Anchor = AnchorStyles.Top | AnchorStyles.Left;

        block.Controls.Add(label);
        block.Controls.Add(fieldControl);
        return block;
    }

    private static Control WrapControlIfNeeded(Control control)
    {
        if (control is TextBox textBox)
        {
            return CreateTextBoxFrame(textBox);
        }

        if (control is NumericUpDown numericUpDown)
        {
            return CreateNumericFrame(numericUpDown);
        }

        if (control is not ComboBox comboBox)
        {
            return control;
        }

        var frame = new Panel
        {
            BackColor = Color.White,
            Padding = new Padding(1),
            Margin = new Padding(0)
        };
        frame.Paint += (_, args) =>
        {
            ControlPaint.DrawBorder(args.Graphics, frame.ClientRectangle,
                Color.Black, 1, ButtonBorderStyle.Solid,
                Color.Black, 1, ButtonBorderStyle.Solid,
                Color.Black, 1, ButtonBorderStyle.Solid,
                Color.Black, 1, ButtonBorderStyle.Solid);
        };

        comboBox.Dock = DockStyle.Fill;
        comboBox.Margin = new Padding(0);
        frame.Controls.Add(comboBox);
        return frame;
    }

    private static Panel CreateImageBlock(string labelText, TextBox textBox, params Button[] actionButtons)
    {
        var block = new Panel
        {
            Height = 38,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        var label = CreateFieldLabel(labelText);
        label.Location = new Point(0, 8);
        label.Width = 140;

        var textBoxFrame = CreateTextBoxFrame(textBox);
        block.Controls.Add(label);
        block.Controls.Add(textBoxFrame);
        foreach (var button in actionButtons)
        {
            block.Controls.Add(button);
        }

        void ApplyLayout()
        {
            const int left = 146;
            const int gap = 8;
            var right = Math.Max(left + 220, block.ClientSize.Width);
            var currentRight = right;

            for (var index = actionButtons.Length - 1; index >= 0; index--)
            {
                var button = actionButtons[index];
                var buttonWidth = Math.Max(button.Width, 90);
                var buttonHeight = Math.Max(button.Height, 30);
                currentRight -= buttonWidth;
                button.Location = new Point(currentRight, 1);
                button.Size = new Size(buttonWidth, buttonHeight);
                button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                currentRight -= gap;
            }

            textBoxFrame.Location = new Point(left, 4);
            textBoxFrame.Size = new Size(Math.Max(120, currentRight - left), 28);
            textBoxFrame.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        }

        block.Resize += (_, _) => ApplyLayout();
        ApplyLayout();
        return block;
    }

    private static Panel CreateTextBoxFrame(TextBox textBox)
    {
        var frame = new Panel
        {
            BackColor = Color.White,
            Padding = textBox.Multiline ? new Padding(4) : new Padding(1),
            Margin = new Padding(0)
        };
        frame.Paint += (_, args) =>
        {
            ControlPaint.DrawBorder(args.Graphics, frame.ClientRectangle,
                Color.Black, 1, ButtonBorderStyle.Solid,
                Color.Black, 1, ButtonBorderStyle.Solid,
                Color.Black, 1, ButtonBorderStyle.Solid,
                Color.Black, 1, ButtonBorderStyle.Solid);
        };

        textBox.BorderStyle = BorderStyle.None;
        textBox.BackColor = Color.White;
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0);
        frame.Controls.Add(textBox);
        return frame;
    }

    private static Panel CreateNumericFrame(NumericUpDown numericUpDown)
    {
        var frame = new Panel
        {
            BackColor = Color.White,
            Padding = new Padding(1),
            Margin = new Padding(0)
        };
        frame.Paint += (_, args) =>
        {
            ControlPaint.DrawBorder(args.Graphics, frame.ClientRectangle,
                Color.Black, 1, ButtonBorderStyle.Solid,
                Color.Black, 1, ButtonBorderStyle.Solid,
                Color.Black, 1, ButtonBorderStyle.Solid,
                Color.Black, 1, ButtonBorderStyle.Solid);
        };

        numericUpDown.BorderStyle = BorderStyle.None;
        numericUpDown.BackColor = Color.White;
        numericUpDown.Dock = DockStyle.Fill;
        numericUpDown.Margin = new Padding(0);
        frame.Controls.Add(numericUpDown);
        return frame;
    }

    private static Panel CreateButtonBlock(string labelText, Control inputControl, params Button[] buttons)
    {
        var block = new Panel
        {
            Height = 38,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        var label = CreateFieldLabel(labelText);
        label.Location = new Point(0, 8);
        label.Width = 100;

        var panel = new FlowLayoutPanel
        {
            Location = new Point(106, 0),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0)
        };

        var inputFrame = WrapControlIfNeeded(inputControl);
        inputFrame.Size = new Size(120, 28);
        inputFrame.Margin = new Padding(0, 2, 10, 0);
        panel.Controls.Add(inputFrame);

        foreach (var button in buttons)
        {
            panel.Controls.Add(button);
        }

        block.Controls.Add(label);
        block.Controls.Add(panel);
        return block;
    }

    private static Panel CreateBulkUpdateBlock(string labelText, NumericUpDown inputControl, Button actionButton)
    {
        var block = new Panel
        {
            Height = 40,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        var label = CreateFieldLabel(labelText);
        label.Location = new Point(0, 10);
        label.Size = new Size(84, 20);
        label.TextAlign = ContentAlignment.MiddleLeft;

        var inputFrame = CreateNumericFrame(inputControl);
        inputFrame.Location = new Point(88, 6);
        inputFrame.Size = new Size(124, 28);
        inputFrame.Anchor = AnchorStyles.Top | AnchorStyles.Left;

        actionButton.Margin = new Padding(0);
        actionButton.Location = new Point(224, 4);
        actionButton.Size = new Size(140, 32);
        actionButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;

        block.Controls.Add(label);
        block.Controls.Add(inputFrame);
        block.Controls.Add(actionButton);
        return block;
    }

    private static Panel CreateVariantGridBlock(string labelText, DataGridView grid, Button addButton, Button removeButton, Button uploadButton)
    {
        var block = new Panel
        {
            Height = 252,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        var label = CreateFieldLabel(labelText);
        label.Location = new Point(0, 8);
        label.Width = 140;

        var buttonPanel = new FlowLayoutPanel
        {
            Location = new Point(146, 0),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0)
        };
        buttonPanel.Controls.Add(addButton);
        buttonPanel.Controls.Add(removeButton);
        buttonPanel.Controls.Add(uploadButton);

        grid.Location = new Point(146, 40);
        grid.Size = new Size(1000, 204);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        block.Controls.Add(label);
        block.Controls.Add(buttonPanel);
        block.Controls.Add(grid);
        return block;
    }

    private static Control CreateSpacer()
    {
        return new Panel
        {
            Height = 34,
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
    }

    private static Label CreateTitleLabel(string text)
    {
        return new Label
        {
            AutoSize = true,
            Text = text,
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            Margin = new Padding(0)
        };
    }

    private static Label CreateSubtitleLabel(string text)
    {
        return new Label
        {
            AutoSize = true,
            Text = text,
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(99, 115, 129),
            Margin = new Padding(0, 6, 0, 0)
        };
    }

    private static Label CreateSectionTitleLabel(string text)
    {
        return new Label
        {
            AutoSize = true,
            Text = text,
            Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            Margin = new Padding(0)
        };
    }

    private static Label CreateSectionSubtitleLabel(string text)
    {
        return new Label
        {
            AutoSize = true,
            Text = text,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(99, 115, 129),
            Margin = new Padding(0, 4, 0, 0)
        };
    }

    private static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            AutoSize = false,
            Height = 20,
            Text = text,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(55, 65, 81),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static void ConfigureTextBox(TextBox textBox, string placeholder = "")
    {
        textBox.BorderStyle = BorderStyle.None;
        textBox.BackColor = Color.White;
        textBox.ForeColor = Color.FromArgb(17, 24, 39);
        textBox.Font = new Font("Segoe UI", 9.5F);
        textBox.Margin = new Padding(0);
        if (!string.IsNullOrWhiteSpace(placeholder))
        {
            textBox.PlaceholderText = placeholder;
        }
    }

    private static void ConfigureComboBox(ComboBox comboBox)
    {
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.BackColor = Color.White;
        comboBox.ForeColor = Color.FromArgb(17, 24, 39);
        comboBox.Font = new Font("Segoe UI", 9.5F);
        comboBox.IntegralHeight = false;
        comboBox.DropDownHeight = 220;
        comboBox.Margin = new Padding(0);
    }

    private static void ConfigureNumericStyle(NumericUpDown control)
    {
        control.BorderStyle = BorderStyle.None;
        control.BackColor = Color.White;
        control.ForeColor = Color.FromArgb(17, 24, 39);
        control.Font = new Font("Segoe UI", 9.5F);
        control.ThousandsSeparator = true;
        control.Margin = new Padding(0);
    }

    private static void ConfigurePrimaryButton(Button button, int width)
    {
        button.Width = width;
        button.Height = 32;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Color.FromArgb(37, 99, 235);
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        button.Margin = new Padding(0, 0, 10, 0);
    }

    private static void ConfigureIconButton(Button button)
    {
        button.Width = 90;
        button.Height = 32;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Color.FromArgb(15, 118, 110);
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        button.Margin = new Padding(0, 0, 10, 0);
        button.TextAlign = ContentAlignment.MiddleCenter;
    }

    private static void ConfigureGhostButton(Button button)
    {
        button.Width = 72;
        button.Height = 32;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(191, 219, 254);
        button.BackColor = Color.FromArgb(239, 246, 255);
        button.ForeColor = Color.FromArgb(30, 64, 175);
        button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        button.Margin = new Padding(0, 0, 10, 0);
        button.TextAlign = ContentAlignment.MiddleCenter;
    }

    private static void ConfigureSecondaryButton(Button button, int width)
    {
        button.Width = width;
        button.Height = 32;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
        button.BackColor = Color.White;
        button.ForeColor = Color.FromArgb(31, 41, 55);
        button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        button.Margin = new Padding(0, 0, 10, 0);
    }

    private static void ConfigureNumeric(NumericUpDown control, decimal maximum, int decimals)
    {
        control.Maximum = maximum;
        control.DecimalPlaces = decimals;
        control.Increment = decimals == 0 ? 1 : 0.1M;
        ConfigureNumericStyle(control);
    }

    private static void ConfigureVariationGrid(DataGridView grid)
    {
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = true;
        grid.AllowUserToResizeRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.EnableHeadersVisualStyles = false;
        grid.GridColor = Color.FromArgb(229, 231, 235);
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.EditMode = DataGridViewEditMode.EditOnEnter;
        grid.Font = new Font("Segoe UI", 9F);
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        grid.RowTemplate.Height = 30;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(243, 244, 246);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(31, 41, 55);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(17, 24, 39);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 24, 39);
        grid.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        grid.ScrollBars = ScrollBars.Both;
        grid.Columns.Clear();
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Option",
            HeaderText = "Option",
            Width = 180,
            MinimumWidth = 140,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Price",
            HeaderText = "Price",
            Width = 110,
            MinimumWidth = 90,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Stock",
            HeaderText = "Stock",
            Width = 90,
            MinimumWidth = 80,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Image",
            HeaderText = "Image / URL",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 520,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        grid.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "UploadVariantImage",
            HeaderText = "Upload",
            Text = "Upload",
            UseColumnTextForButtonValue = true,
            Width = 110,
            MinimumWidth = 96
        });
    }
}
