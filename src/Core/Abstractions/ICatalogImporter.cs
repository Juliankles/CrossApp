using Core.Domain;

namespace Core.Abstractions;

// Асинхронний контракт імпорту каталогу примірників із зовнішнього джерела
public interface ICatalogImporter
{
    Task<IReadOnlyList<BookCopy>> LoadAsync(string path, CancellationToken ct = default);
}
