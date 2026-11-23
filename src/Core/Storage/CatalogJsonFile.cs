using System.Text.Json;
using Core.Dto;

namespace Core.Storage;

// Експорт та імпорт усього каталогу видань у JSON-файл
public static class CatalogJsonFile
{
    public static void Write(string path, IEnumerable<EditionDto> editions) =>
        AtomicFile.WriteAllText(path, JsonSerializer.Serialize(editions.ToList(), JsonFiles.Options));

    public static IReadOnlyList<EditionDto> Read(string path)
    {
        List<EditionDto?> dtos;
        try
        {
            dtos = JsonSerializer.Deserialize<List<EditionDto?>>(File.ReadAllText(path), JsonFiles.Options) ?? [];
        }
        catch (JsonException ex)
        {
            throw new StorageException($"Файл {Path.GetFileName(path)} пошкоджено: {ex.Message}", ex);
        }

        // Кожен запис перевіряється, перш ніж потрапити в результат
        var result = new List<EditionDto>(dtos.Count);
        for (int i = 0; i < dtos.Count; i++)
        {
            EditionDto dto = dtos[i] ?? throw new DomainException($"Запис {i + 1}: порожній запис.");

            if (string.IsNullOrWhiteSpace(dto.Isbn))
                throw new DomainException($"Запис {i + 1}: ISBN не може бути порожнім.");
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new DomainException($"Запис {i + 1}: назва видання обов'язкова.");
            if (dto.Year < 1450 || dto.Year > DateTime.Today.Year)
                throw new DomainException($"Запис {i + 1}: рік {dto.Year} поза допустимими межами.");
            if (dto.Copies < 0)
                throw new DomainException($"Запис {i + 1}: кількість примірників не може бути від'ємною.");

            result.Add(dto);
        }

        return result;
    }
}
