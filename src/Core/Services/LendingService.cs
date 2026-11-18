using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class LendingService(IBookStore store)
{
    private readonly IBookStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public event EventHandler<LendingChangedEventArgs>? Changed;

    private void OnChanged(BookCopy copy, LendingChangeKind kind,
                           string? readerId = null, DateOnly? dueDate = null) =>
        Changed?.Invoke(this, new LendingChangedEventArgs(copy.Id, copy.Isbn, kind, readerId, dueDate));

    public BookCopy AddBook(string isbn)
    {
        string id = "C-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        BookCopy copy = BookCopy.Create(id, isbn);
        _store.Add(copy);
        OnChanged(copy, LendingChangeKind.BookAdded);
        return copy;
    }

    public void IssueCopy(string copyId, string readerId, int days = 14)
    {
        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));
        if (days <= 0)
            throw new ArgumentOutOfRangeException(nameof(days), days, "Строк видачі має бути більшим за нуль");

        BookCopy copy = GetExisting(copyId);
        copy.Issue();         
        _store.Update(copy);

        DateOnly dueDate = DateOnly.FromDateTime(DateTime.Today).AddDays(days);
        OnChanged(copy, LendingChangeKind.CopyIssued, readerId.Trim(), dueDate);
    }

    public void ReturnCopy(string copyId)
    {
        BookCopy copy = GetExisting(copyId);
        copy.Return();         
        _store.Update(copy);
        OnChanged(copy, LendingChangeKind.CopyReturned);
    }

    public bool WriteOffCopy(string copyId)
    {
        BookCopy? copy = _store.GetById(copyId);
        if (copy is null || !_store.Remove(copyId))
            return false;

        OnChanged(copy, LendingChangeKind.CopyWrittenOff);
        return true;
    }

    // Асинхронний імпорт каталогу: імпортер приходить параметром (ручний DI),
    // токен скасування передається далі в LoadAsync і перевіряється в циклі.
    public async Task<int> ImportAsync(ICatalogImporter importer, string path, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(importer);

        IReadOnlyList<BookCopy> items = await importer.LoadAsync(path, ct);

        int added = 0;
        foreach (BookCopy item in items)
        {
            ct.ThrowIfCancellationRequested();

            if (_store.GetById(item.Id) is not null)
                continue;                                   // повторний імпорт не дублює записи

            _store.Add(item);
            OnChanged(item, LendingChangeKind.BookAdded);   // подія для кожного доданого примірника
            added++;
        }

        return added;
    }

    public IReadOnlyList<BookCopy> All() => _store.List();

    public BookCopy? Find(string copyId) => _store.GetById(copyId);


    public IReadOnlyList<BookCopy> FindAll(Func<BookCopy, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    private BookCopy GetExisting(string copyId) =>
        _store.GetById(copyId)
        ?? throw new DomainException($"Примірника з id={copyId} немає у сховищі.");
}