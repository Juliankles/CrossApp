using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

// Імпорт каталогу примірників із JSON-файлу (список BookCopyDto).
// delayPerRecord — штучна затримка на кожен запис лише для демонстрації скасування (команда import-slow).
public sealed class JsonCatalogImporter(TimeSpan? delayPerRecord = null) : ICatalogImporter
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<BookCopy>> LoadAsync(string path, CancellationToken ct = default)
    {
        // Якщо скасування вже запитано, файл навіть не відкриваємо
        ct.ThrowIfCancellationRequested();

        // Справжнє асинхронне читання: useAsync: true, без Task.Run
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                                                FileShare.Read, 4096, useAsync: true);

        List<BookCopyDto?> dtos =
            await JsonSerializer.DeserializeAsync<List<BookCopyDto?>>(stream, Options, ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку примірників.");

        var result = new List<BookCopy>(dtos.Count);
        for (int i = 0; i < dtos.Count; i++)
        {
            // Перевірка скасування на кожному записі
            ct.ThrowIfCancellationRequested();

            if (delayPerRecord is { } delay)
                await Task.Delay(delay, ct);

            BookCopyDto dto = dtos[i] ?? throw new DomainException($"Запис {i + 1}: порожній запис.");

            try
            {
                result.Add(BookCopy.FromDto(dto));
            }
            catch (ArgumentException ex)
            {
                // Погані дані у файлі — доменна помилка з номером запису; початковий виняток зберігаємо як InnerException
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }
        }

        return result;
    }
}
