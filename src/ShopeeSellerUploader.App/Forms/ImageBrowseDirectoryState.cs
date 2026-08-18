namespace ShopeeSellerUploader.App.Forms;

internal static class ImageBrowseDirectoryState
{
    private static string? s_lastBrowsedImageDirectory;
    private static readonly string CacheFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ShopeeSellerUploader",
        "last-image-directory.txt");

    public static void ApplyDefaultDirectory(OpenFileDialog dialog)
    {
        EnsureLoaded();

        if (!string.IsNullOrWhiteSpace(s_lastBrowsedImageDirectory) &&
            Directory.Exists(s_lastBrowsedImageDirectory))
        {
            dialog.InitialDirectory = s_lastBrowsedImageDirectory;
            return;
        }

        var pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        if (Directory.Exists(pictures))
        {
            dialog.InitialDirectory = pictures;
        }
    }

    public static void Remember(string? selectedFilePath)
    {
        var directory = Path.GetDirectoryName(selectedFilePath ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
        {
            s_lastBrowsedImageDirectory = directory;
            Persist(directory);
        }
    }

    private static void EnsureLoaded()
    {
        if (!string.IsNullOrWhiteSpace(s_lastBrowsedImageDirectory))
        {
            return;
        }

        try
        {
            if (!File.Exists(CacheFilePath))
            {
                return;
            }

            var cachedDirectory = File.ReadAllText(CacheFilePath).Trim();
            if (!string.IsNullOrWhiteSpace(cachedDirectory) && Directory.Exists(cachedDirectory))
            {
                s_lastBrowsedImageDirectory = cachedDirectory;
            }
        }
        catch
        {
            // Ignore cache read errors and fall back to Pictures.
        }
    }

    private static void Persist(string directory)
    {
        try
        {
            var cacheDirectory = Path.GetDirectoryName(CacheFilePath);
            if (!string.IsNullOrWhiteSpace(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
            }

            File.WriteAllText(CacheFilePath, directory);
        }
        catch
        {
            // Ignore cache write errors and keep the in-memory value.
        }
    }
}
