using System.Text.Encodings.Web;
using System.Text.Json;

namespace Core.Storage;

// Спільні налаштування JSON для файлів даних
internal static class JsonFiles
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,                                   // зручно читати людині
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,      // isbn, title, isIssued
        PropertyNameCaseInsensitive = true,                     // старі файли з Isbn/IsIssued теж читаються
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping   // кирилиця як є, а не Ц
    };
}
