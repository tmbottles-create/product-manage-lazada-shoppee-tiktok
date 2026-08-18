using ShopeeSellerUploader.App.Services;
using ShopeeSellerUploader.Core.Models;
using System.ComponentModel;
using System.Diagnostics;

namespace ShopeeSellerUploader.App.Forms;

public sealed class WebPriceCheckForm : Form
{
    private readonly IReadOnlyList<ProductItem> _products;
    private readonly MarketplaceWebPriceService _service;
    private readonly Action<string> _appendLog;
    private readonly BindingList<MarketplaceWebPriceResult> _rows = [];
    private readonly DataGridView _grid;
    private readonly Label _statusLabel;
    private readonly Button _refreshButton;
    private readonly Button _openButton;

    public WebPriceCheckForm(
        IReadOnlyList<ProductItem> products,
        MarketplaceWebPriceService service,
        Action<string> appendLog)
    {
        _products = products;
        _service = service;
        _appendLog = appendLog;

        Text = "Web Price Check";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(980, 560);
        ClientSize = new Size(1280, 720);
        BackColor = Color.FromArgb(248, 250, 252);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16),
            BackColor = BackColor
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty
        };

        _refreshButton = new Button
        {
            Text = "Refresh",
            AutoSize = true,
            Padding = new Padding(14, 6, 14, 6),
            BackColor = Color.White
        };
        _refreshButton.Click += async (_, _) => await LoadResultsAsync();

        _openButton = new Button
        {
            Text = "Open Selected Link",
            AutoSize = true,
            Padding = new Padding(14, 6, 14, 6),
            BackColor = Color.White
        };
        _openButton.Click += (_, _) => OpenSelectedLink();

        toolbar.Controls.Add(_refreshButton);
        toolbar.Controls.Add(_openButton);

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            DataSource = _rows
        };
        _grid.CellContentClick += Grid_CellContentClick;
        _grid.CellDoubleClick += (_, _) => OpenSelectedLink();
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MarketplaceWebPriceResult.ProductCode), HeaderText = "Product Code", Width = 120, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MarketplaceWebPriceResult.Marketplace), HeaderText = "Marketplace", Width = 110, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MarketplaceWebPriceResult.Title), HeaderText = "Listing", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MarketplaceWebPriceResult.PriceText), HeaderText = "Price", Width = 110, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MarketplaceWebPriceResult.Status), HeaderText = "Status", Width = 120, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewLinkColumn { DataPropertyName = nameof(MarketplaceWebPriceResult.Link), HeaderText = "Link", Width = 320, ReadOnly = true, TrackVisitedState = false, UseColumnTextForLinkValue = false });

        _statusLabel = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            Text = "Preparing web price check..."
        };

        root.Controls.Add(toolbar, 0, 0);
        root.Controls.Add(_grid, 0, 1);
        root.Controls.Add(_statusLabel, 0, 2);
        Controls.Add(root);
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await LoadResultsAsync();
    }

    private async Task LoadResultsAsync()
    {
        _refreshButton.Enabled = false;
        _openButton.Enabled = false;
        _statusLabel.Text = $"Checking {_products.Count} product(s) on Shopee, Lazada, and TikTok...";
        _rows.Clear();

        try
        {
            foreach (var product in _products)
            {
                var results = await _service.SearchAsync(product);
                foreach (var result in results)
                {
                    _rows.Add(result);
                }

                _appendLog($"Web price check | {product.ProductCode} | {results.Count} row(s)");
            }

            _statusLabel.Text = _rows.Count == 0
                ? "No web price results were found."
                : $"Loaded {_rows.Count} row(s) for {_products.Count} product(s). Double-click a row to open the listing.";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Failed to load web price results.";
            MessageBox.Show(this, ex.Message, "Web Price Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _refreshButton.Enabled = true;
            _openButton.Enabled = _rows.Count > 0;
        }
    }

    private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        if (_grid.Columns[e.ColumnIndex] is DataGridViewLinkColumn)
        {
            OpenSelectedLink();
        }
    }

    private void OpenSelectedLink()
    {
        if (_grid.CurrentRow?.DataBoundItem is not MarketplaceWebPriceResult result ||
            string.IsNullOrWhiteSpace(result.Link))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = result.Link,
            UseShellExecute = true
        });
    }
}
