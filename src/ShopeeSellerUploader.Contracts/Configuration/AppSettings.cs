namespace ShopeeSellerUploader.Contracts.Configuration;

public sealed class AppSettings
{
    public const string SectionName = "AppSettings";
    public required BrowserOptions Browser { get; init; }
    public required OpenAiOptions OpenAi { get; init; }
    public required LazadaOptions Lazada { get; init; }
    public required ImageKitOptions ImageKit { get; init; }
    public required AutomationOptions Automation { get; init; }
    public required StorageOptions Storage { get; init; }
    public required SelectorOptions Selectors { get; init; }
    public required ProductCatalogOptions ProductCatalog { get; init; }
}

public sealed class ImageKitOptions
{
    public string UploadApiUrl { get; set; } = "https://upload.imagekit.io/api/v1/files/upload";
    public string UrlEndpoint { get; set; } = "https://ik.imagekit.io/yzx2xk3aq";
    public string UploadFolderPath { get; set; } = "/path/to";
    public string PrivateKey { get; set; } = string.Empty;
    public string PrivateKeyEnvironmentVariable { get; set; } = "IMAGEKIT_PRIVATE_KEY";
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxUploadSizeMb { get; set; } = 25;
    public bool UseUniqueFileName { get; set; }
}

public sealed class LazadaOptions
{
    public string AuthorizeUrl { get; set; } = "https://auth.lazada.com/oauth/authorize";
    public string AuthBaseUrl { get; set; } = "https://auth.lazada.com/rest";
    public string ApiBaseUrl { get; set; } = "https://api.lazada.co.th/rest";
    public string AppKey { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public string AccessTokenEnvironmentVariable { get; set; } = "LAZADA_ACCESS_TOKEN";
    public string RefreshTokenEnvironmentVariable { get; set; } = "LAZADA_REFRESH_TOKEN";
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxUploadSizeMb { get; set; } = 3;
}

public sealed class OpenAiOptions
{
    public bool Enabled { get; init; } = true;
    public string BaseUrl { get; init; } = "https://api.openai.com/v1/responses";
    public string Model { get; init; } = "gpt-5.6";
    public string ApiKeyEnvironmentVariable { get; init; } = "OPENAI_API_KEY";
    public int TimeoutSeconds { get; init; } = 120;
    public int MaxImagesPerRequest { get; init; } = 3;
}

public sealed class BrowserOptions
{
    public string SellerCenterUrl { get; init; } = "https://seller.shopee.co.th/";
    public string Channel { get; init; } = "chrome";
    public string FallbackChannel { get; init; } = "msedge";
    public string ExecutablePath { get; init; } = string.Empty;
    public bool Headless { get; init; }
    public int DefaultTimeoutMs { get; init; } = 30000;
}

public sealed class AutomationOptions
{
    public int MaxRetryCount { get; init; } = 3;
    public int DelayBetweenProductsMs { get; init; } = 1500;
    public bool RequireManualPublishConfirmation { get; init; } = true;
    public bool SaveAsDraftOnly { get; init; } = true;
}

public sealed class StorageOptions
{
    public string WorkingDirectory { get; init; } = "Data";
    public string SessionFileName { get; init; } = "session.dat";
    public string CheckpointFileName { get; init; } = "checkpoint.json";
    public string ResultSnapshotFileName { get; init; } = "result-snapshot.json";
    public string TemplateFileName { get; init; } = "ShopeeProductTemplate.xlsx";
    public string LogFileName { get; init; } = "logs\\shopee-seller-uploader-.log";
    public string OpenAiApiKeyFileName { get; init; } = "openai-api-key.bin";
    public string LazadaTokenFileName { get; init; } = "lazada-token.bin";
}

public sealed class ProductCatalogOptions
{
    public string DatabaseFileName { get; set; } = "product-catalog.db";
    public string ExportDirectoryName { get; set; } = "exports";
    public string TemplateRootDirectory { get; set; } = @"D:\shoppee-lazada-templete";
    public LazadaImageMode LazadaImageMode { get; set; } = LazadaImageMode.PublicImageUrl;
}

public enum LazadaImageMode
{
    PublicImageUrl = 0,
    LocalFilePath = 1
}

public sealed class SelectorOptions
{
    public required CommonSelectorOptions Common { get; init; }
    public required LoginSelectorOptions Login { get; init; }
    public required ProductFormSelectorOptions ProductForm { get; init; }
}

public sealed class CommonSelectorOptions
{
    public string LoadingMask { get; init; } = "[data-testid='loading']";
    public string ToastMessage { get; init; } = ".shopee-toast";
    public string PublishSuccessIndicator { get; init; } = "text=สำเร็จ";
    public string CaptchaIndicator { get; init; } = "iframe[title*='captcha'], text=/captcha/i";
    public string OtpIndicator { get; init; } = "text=/otp|verification code|ยืนยันตัวตน/i";
}

public sealed class LoginSelectorOptions
{
    public string UserAvatar { get; init; } = "[data-testid='account-avatar']";
    public string LoginForm { get; init; } = "input[name='loginKey'], input[type='password']";
}

public sealed class ProductFormSelectorOptions
{
    public string AddProductButton { get; init; } = "text=เพิ่มสินค้าใหม่, text=Add New Product";
    public string ProductNameInput { get; init; } = "input[placeholder*='ชื่อสินค้า'], input[placeholder*='product name']";
    public string DescriptionEditor { get; init; } = "[contenteditable='true']";
    public string CategoryPicker { get; init; } = "text=หมวดหมู่, text=Category";
    public string CategorySearchInput { get; init; } = "input[placeholder*='ค้นหา'], input[placeholder*='search']";
    public string CategoryConfirmButton { get; init; } = "button:has-text('ยืนยัน'), button:has-text('Confirm')";
    public string PriceInput { get; init; } = "input[placeholder*='ราคา'], input[inputmode='decimal']";
    public string StockInput { get; init; } = "input[placeholder*='สต๊อก'], input[placeholder*='Stock']";
    public string WeightInput { get; init; } = "input[placeholder*='น้ำหนัก'], input[placeholder*='Weight']";
    public string LengthInput { get; init; } = "input[placeholder*='Length'], input[placeholder*='ยาว']";
    public string WidthInput { get; init; } = "input[placeholder*='Width'], input[placeholder*='กว้าง']";
    public string HeightInput { get; init; } = "input[placeholder*='Height'], input[placeholder*='สูง']";
    public string SkuInput { get; init; } = "input[placeholder*='SKU']";
    public string ImageUploadInput { get; init; } = "input[type='file']";
    public string AddVariationButton { get; init; } = "text=เปิดใช้งานสินค้าแยก, text=Enable Variations";
    public string VariationNameInput { get; init; } = "input[placeholder*='เช่น สี'], input[placeholder*='Variation name']";
    public string VariationOptionInput { get; init; } = "input[placeholder*='เช่น สีแดง'], input[placeholder*='Variation option']";
    public string VariationPriceInput { get; init; } = "input[placeholder*='ราคาสินค้าแยก'], input[placeholder*='Variation price']";
    public string VariationStockInput { get; init; } = "input[placeholder*='สต๊อกสินค้าแยก'], input[placeholder*='Variation stock']";
    public string SaveDraftButton { get; init; } = "button:has-text('บันทึกเป็นฉบับร่าง'), button:has-text('Save as draft')";
    public string PublishButton { get; init; } = "button:has-text('เผยแพร่สินค้า'), button:has-text('Publish')";
    public string AttributeSection { get; init; } = "text=คุณสมบัติ, text=Attributes";
}
