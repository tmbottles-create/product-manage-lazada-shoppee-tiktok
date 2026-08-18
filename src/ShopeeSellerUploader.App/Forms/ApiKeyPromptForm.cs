namespace ShopeeSellerUploader.App.Forms;

public partial class ApiKeyPromptForm : Form
{
    public string ApiKey => txtApiKey.Text.Trim();
    public bool SaveToMachine => chkSaveToMachine.Checked;
    public bool ClearExistingKey => chkClearExistingKey.Checked;

    public ApiKeyPromptForm()
    {
        InitializeComponent();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (!ClearExistingKey && string.IsNullOrWhiteSpace(ApiKey))
        {
            MessageBox.Show(this, "Please enter an API key or choose to clear the saved key.", "API Key", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void chkClearExistingKey_CheckedChanged(object sender, EventArgs e)
    {
        txtApiKey.Enabled = !chkClearExistingKey.Checked;
        chkSaveToMachine.Enabled = !chkClearExistingKey.Checked;
    }
}
