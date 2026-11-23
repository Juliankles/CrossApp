using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileBookStore(string path) : IBookStore
{
    private readonly Dictionary<string, BookCopy> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (!File.Exists(_path)) { _loaded = true; return; }   // файлу ще немає — сховище порожнє

        List<BookCopyDto> dtos;
        try
        {
            dtos = JsonSerializer.Deserialize<List<BookCopyDto>>(File.ReadAllText(_path), JsonFiles.Options) ?? [];
        }
        catch (JsonException ex)
        {
            throw new StorageException($"Файл даних {_path} пошкоджено: {ex.Message}", ex);
        }

        _cache.Clear();
        foreach (BookCopyDto dto in dtos)
        {
            BookCopy copy = BookCopy.FromDto(dto);   // повторно перевіряє інваріанти
            _cache[copy.Id] = copy;
        }

        _loaded = true;
    }

    private void Flush()
    {
        // Атомарний запис через .tmp + File.Move (див. AtomicFile)
        List<BookCopyDto> dtos = _cache.Values.Select(c => c.ToDto()).ToList();
        AtomicFile.WriteAllText(_path, JsonSerializer.Serialize(dtos, JsonFiles.Options));
    }

    public IReadOnlyList<BookCopy> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public BookCopy? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(BookCopy item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        if (_cache.ContainsKey(item.Id))
            throw new DomainException($"Примірник з id={item.Id} уже існує.");

        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(BookCopy item)
    {
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;
        Flush();
        return true;
    }
}