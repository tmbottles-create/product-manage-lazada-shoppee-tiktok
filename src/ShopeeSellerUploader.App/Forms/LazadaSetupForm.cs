using System.Diagnostics;
using ShopeeSellerUploader.Contracts.Configuration;
using ShopeeSellerUploader.Core.Models;
using ShopeeSellerUploader.Infrastructure.Services;

namespace ShopeeSellerUploader.App.Forms;

public sealed class LazadaSetupForm : Form
{
    private readonly LazadaAuthorizationService _authorizationService;
    private readonly TextBox _txtAppKey;
    private readonly TextBox _txtAppSecret;
    private readonly TextBox _txtCallbackUrl;
    private readonly TextBox _txtAuthorizeUrl;
    private readonly TextBox _txtAuthBaseUrl;
    private readonly TextBox _txtApiBaseUrl;
    private readonly TextBox _txtAuthorizationCode;
    private readonly TextBox _txtAccessToken;
    private readonly TextBox _txtRefreshToken;
    private readonly Label _lblStatus;
    private readonly Button _btnExchangeCode;

    public LazadaSetupForm(
        LazadaOptions options,
        OneDriveTokenSnapshot? tokenSnapshot,
        LazadaAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;

        Text = "Lazada Setup";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(900, 760);
        MinimumSize = new Size(900, 760);
        Font = new Font("Segoe UI", 9.5F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(16),
            BackColor = Color.White
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = new Label
        {
            AutoSize = true,
            Text = "Configure Lazada app credentials and exchange the seller authorization code for tokens.",
            Font = new Font("Segoe UI", 11F, FontStyle.Regular),
            ForeColor = Color.FromArgb(31, 41, 55),
            Margin = new Padding(0, 0, 0, 12)
        };

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            ColumnCount = 2,
            RowCount = 10,
            BackColor = Color.White
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        _txtAppKey = CreateTextBox(options.AppKey);
        _txtAppSecret = CreateTextBox(options.AppSecret, usePassword: true);
        _txtCallbackUrl = CreateTextBox(options.CallbackUrl);
        _txtAuthorizeUrl = CreateTextBox(options.AuthorizeUrl);
        _txtAuthBaseUrl = CreateTextBox(options.AuthBaseUrl);
        _txtApiBaseUrl = CreateTextBox(options.ApiBaseUrl);
        _txtAuthorizationCode = CreateTextBox(string.Empty);
        _txtAccessToken = CreateTextBox(tokenSnapshot?.AccessToken ?? string.Empty, multiline: true, usePassword: true, height: 90);
        _txtRefreshToken = CreateTextBox(tokenSnapshot?.RefreshToken ?? string.Empty, multiline: true, usePassword: true, height: 90);

        AddRow(fields, 0, "App Key", _txtAppKey);
        AddRow(fields, 1, "App Secret", _txtAppSecret);
        AddRow(fields, 2, "Callback URL", _txtCallbackUrl);
        AddRow(fields, 3, "Authorize URL", _txtAuthorizeUrl);
        AddRow(fields, 4, "Auth Base URL", _txtAuthBaseUrl);
        AddRow(fields, 5, "API Base URL", _txtApiBaseUrl);
        AddRow(fields, 6, "Auth Code", _txtAuthorizationCode);
        AddRow(fields, 7, "Access Token", _txtAccessToken);
        AddRow(fields, 8, "Refresh Token", _txtRefreshToken);

        var authActions = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 8)
        };

        var btnOpenAuthorize = CreateActionButton("Open Authorize Page");
        btnOpenAuthorize.Click += (_, _) => OpenAuthorizeUrl();
        var btnCopyAuthorize = CreateActionButton("Copy Authorize URL");
        btnCopyAuthorize.Click += (_, _) => CopyAuthorizeUrl();
        _btnExchangeCode = CreateActionButton("Exchange Code For Tokens");
        _btnExchangeCode.Click += async (_, _) => await ExchangeAuthorizationCodeAsync();

        authActions.Controls.Add(btnOpenAuthorize);
        authActions.Controls.Add(btnCopyAuthorize);
        authActions.Controls.Add(_btnExchangeCode);
        AddRow(fields, 9, "Actions", authActions);

        _lblStatus = new Label
        {
            AutoSize = true,
            ForeColor = Color.FromArgb(75, 85, 99),
            Text = "Fill App Key, App Secret, and Callback URL first. Then authorize the seller account and paste the code here.",
            MaximumSize = new Size(820, 0),
            Margin = new Padding(0, 8, 0, 0)
        };

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };

        var btnSave = new Button
        {
            Text = "Save",
            Width = 110,
            Height = 36,
            DialogResult = DialogResult.OK
        };
        btnSave.Click += btnSave_Click;

        var btnCancel = new Button
        {
            Text = "Cancel",
            Width = 110,
            Height = 36,
            DialogResult = DialogResult.Cancel
        };

        buttons.Controls.Add(btnCancel);
        buttons.Controls.Add(btnSave);

        AcceptButton = btnSave;
        CancelButton = btnCancel;

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(fields, 0, 1);
        root.Controls.Add(_lblStatus, 0, 2);
        root.Controls.Add(buttons, 0, 3);
        Controls.Add(root);
    }

    public string AppKey => _txtAppKey.Text.Trim();
    public string AppSecret => _txtAppSecret.Text.Trim();
    public string CallbackUrl => _txtCallbackUrl.Text.Trim();
    public string AuthorizeUrl => _txtAuthorizeUrl.Text.Trim();
    public string AuthBaseUrl => _txtAuthBaseUrl.Text.Trim();
    public string ApiBaseUrl => _txtApiBaseUrl.Text.Trim();
    public string AccessToken => _txtAccessToken.Text.Trim();
    public string RefreshToken => _txtRefreshToken.Text.Trim();

    private static TextBox CreateTextBox(string value, bool multiline = false, bool usePassword = false, int height = 34)
    {
        return new TextBox
        {
            Text = value,
            Dock = DockStyle.Top,
            Multiline = multiline,
            UseSystemPasswordChar = usePassword,
            Height = height,
            ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None
        };
    }

    private static Button CreateActionButton(string text)
    {
        return new Button
        {
            Text = text,
            AutoSize = true,
            Height = 34,
            Padding = new Padding(10, 0, 10, 0)
        };
    }

    private static void AddRow(TableLayoutPanel table, int rowIndex, string labelText, Control control)
    {
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var label = new Label
        {
            Text = labelText,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true,
            Margin = new Padding(0, 8, 12, 0)
        };

        control.Margin = new Padding(0, 4, 0, 4);
        table.Controls.Add(label, 0, rowIndex);
        table.Controls.Add(control, 1, rowIndex);
    }

    private LazadaOptions BuildOptions()
    {
        return new LazadaOptions
        {
            AppKey = AppKey,
            AppSecret = AppSecret,
            CallbackUrl = CallbackUrl,
            AuthorizeUrl = AuthorizeUrl,
            AuthBaseUrl = AuthBaseUrl,
            ApiBaseUrl = ApiBaseUrl
        };
    }

    private void OpenAuthorizeUrl()
    {
        try
        {
            var url = _authorizationService.BuildAuthorizationUrl(BuildOptions(), "codex-lazada-setup");
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
            _lblStatus.Text = "Opened the Lazada authorization page. After seller login and approval, paste the returned code here.";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = ex.Message;
            MessageBox.Show(this, ex.Message, "Lazada Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CopyAuthorizeUrl()
    {
        try
        {
            var url = _authorizationService.BuildAuthorizationUrl(BuildOptions(), "codex-lazada-setup");
            Clipboard.SetText(url);
            _lblStatus.Text = "Authorization URL copied to clipboard.";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = ex.Message;
            MessageBox.Show(this, ex.Message, "Lazada Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ExchangeAuthorizationCodeAsync()
    {
        try
        {
            _btnExchangeCode.Enabled = false;
            _lblStatus.Text = "Exchanging Lazada authorization code for tokens...";

            var snapshot = await _authorizationService.CreateTokenAsync(BuildOptions(), _txtAuthorizationCode.Text.Trim());
            _txtAccessToken.Text = snapshot.AccessToken;
            _txtRefreshToken.Text = snapshot.RefreshToken;
            _lblStatus.Text = "Token exchange completed. Review the tokens and click Save.";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = ex.Message;
            MessageBox.Show(this, ex.Message, "Lazada Authorization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnExchangeCode.Enabled = true;
        }
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AppKey))
        {
            MessageBox.Show(this, "App Key is required.", "Lazada Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.None;
            return;
        }

        if (string.IsNullOrWhiteSpace(AppSecret))
        {
            MessageBox.Show(this, "App Secret is required.", "Lazada Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.None;
            return;
        }
    }
}
