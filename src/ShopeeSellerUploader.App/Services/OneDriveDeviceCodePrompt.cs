using System.Diagnostics;
using ShopeeSellerUploader.Contracts.Interfaces;

namespace ShopeeSellerUploader.App.Services;

public sealed class OneDriveDeviceCodePrompt : IOneDriveDeviceCodePrompt
{
    public Task<bool> ShowAsync(
        string message,
        string verificationUri,
        string userCode,
        CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var thread = new Thread(() =>
        {
            try
            {
                using var form = new DeviceCodePromptForm(message, verificationUri, userCode);
                var result = form.ShowDialog();
                tcs.TrySetResult(result == DialogResult.OK);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();

        cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
        return tcs.Task;
    }

    private sealed class DeviceCodePromptForm : Form
    {
        public DeviceCodePromptForm(string message, string verificationUri, string userCode)
        {
            Text = "OneDrive Sign-In Required";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            ClientSize = new Size(620, 300);
            Font = new Font("Segoe UI", 9.5F);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(16),
                BackColor = Color.White
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var title = new Label
            {
                AutoSize = true,
                Text = "Sign in to OneDrive to upload product images",
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Margin = new Padding(0, 0, 0, 8)
            };

            var instruction = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Text = message.Trim(),
                Dock = DockStyle.Top,
                Height = 76,
                Margin = new Padding(0, 0, 0, 8)
            };

            var verificationPanel = new TableLayoutPanel
            {
                ColumnCount = 3,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            verificationPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            verificationPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            verificationPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));

            var verificationLabel = new Label
            {
                Text = "Open URL",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var verificationText = new TextBox
            {
                Text = verificationUri,
                ReadOnly = true,
                Dock = DockStyle.Fill
            };

            var openButton = new Button
            {
                Text = "Open Page",
                Dock = DockStyle.Fill
            };
            openButton.Click += (_, _) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = verificationUri,
                    UseShellExecute = true
                });
            };

            verificationPanel.Controls.Add(verificationLabel, 0, 0);
            verificationPanel.Controls.Add(verificationText, 1, 0);
            verificationPanel.Controls.Add(openButton, 2, 0);

            var codePanel = new TableLayoutPanel
            {
                ColumnCount = 3,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            codePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            codePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            codePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));

            var codeLabel = new Label
            {
                Text = "Code",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var codeText = new TextBox
            {
                Text = userCode,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 12F, FontStyle.Bold)
            };

            var copyButton = new Button
            {
                Text = "Copy Code",
                Dock = DockStyle.Fill
            };
            copyButton.Click += (_, _) => Clipboard.SetText(userCode);

            codePanel.Controls.Add(codeLabel, 0, 0);
            codePanel.Controls.Add(codeText, 1, 0);
            codePanel.Controls.Add(copyButton, 2, 0);

            var note = new Label
            {
                AutoSize = true,
                Text = "After you finish signing in in the browser, click Continue.",
                ForeColor = Color.FromArgb(75, 85, 99),
                Margin = new Padding(0, 0, 0, 8)
            };

            var actions = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0)
            };

            var continueButton = new Button
            {
                Text = "Continue",
                DialogResult = DialogResult.OK,
                Width = 100,
                Height = 32
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Width = 100,
                Height = 32
            };

            actions.Controls.Add(continueButton);
            actions.Controls.Add(cancelButton);

            AcceptButton = continueButton;
            CancelButton = cancelButton;

            root.Controls.Add(title, 0, 0);
            root.Controls.Add(instruction, 0, 1);
            root.Controls.Add(verificationPanel, 0, 2);
            root.Controls.Add(codePanel, 0, 3);
            root.Controls.Add(note, 0, 4);
            root.Controls.Add(actions, 0, 5);
            Controls.Add(root);
        }
    }
}
