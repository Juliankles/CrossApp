namespace Core.Configuration;

// Налаштування застосунку. Заповнюються з appsettings*.json, user-secrets і змінних середовища
// (у Cli через config.Get<AppOptions>()). Core нічого не знає про те, звідки прийшли значення.
public sealed class AppOptions
{
    public string EnvironmentName { get; init; } = "Development";
    public LoggingOptions Logging { get; init; } = new();
    public StorageOptions Storage { get; init; } = new();
    public LibraryOptions Library { get; init; } = new();
}

public sealed class LoggingOptions
{
    // Debug — детальний вивід (Development), Warning — лише важливе (Production)
    public string MinimumLevel { get; init; } = "Information";
}

public sealed class LibraryOptions
{
    // Ключ варіанту «Бібліотека»: Library:LoanDays — строк видачі в днях
    public int LoanDays { get; init; } = 14;
}

public sealed class StorageOptions
{
    // App — поруч із застосунком (AppContext.BaseDirectory);
    // LocalAppData — системна папка локальних даних користувача (профіль Production)
    public string BaseFolder { get; init; } = "App";

    // Відносний (від базової папки) або абсолютний шлях до каталогу даних
    public string DataPath { get; init; } = "data";

    // Файл сховища примірників (режим --file)
    public string FileName { get; init; } = "books.json";

    // Файл експорту/імпорту каталогу видань
    public string ExportFileName { get; init; } = "catalog-export.json";

    // Базова папка, від якої рахується відносний DataPath
    public string ResolveBaseDirectory(string appBaseDir) =>
        BaseFolder.Equals("LocalAppData", StringComparison.OrdinalIgnoreCase)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CrossApp")
            : appBaseDir;

    // Повний шлях до файлу в каталозі даних; каталог створюється, якщо його немає
    public string ResolveFullPath(string appBaseDir, string fileName)
    {
        string root = Path.IsPathRooted(DataPath)
            ? DataPath
            : Path.Combine(ResolveBaseDirectory(appBaseDir), DataPath);

        Directory.CreateDirectory(root);
        return Path.GetFullPath(Path.Combine(root, fileName));
    }
}
