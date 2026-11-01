using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class LendingService(IBookStore store)
{
    private readonly IBookStore _store = store ?? throw new ArgumentNullException(nameof(store));

    // Подія: сервіс повідомляє, що сталося, а як на це реагувати, вирішує зовнішній код
    public event EventHandler<LendingChangedEventArgs>? Changed;

    // Єдине місце, де піднімається подія
    private void OnChanged(BookCopy copy, LendingChangeKind kind,
                           string? readerId = null, DateOnly? dueDate = null) =>
        Changed?.Invoke(this, new LendingChangedEventArgs(copy.Id, copy.Isbn, kind, readerId, dueDate));

    // Додати новий примірник видання з указаним ISBN
    public BookCopy AddBook(string isbn)
    {
        string id = "C-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        BookCopy copy = BookCopy.Create(id, isbn);
        _store.Add(copy);
        OnChanged(copy, LendingChangeKind.BookAdded); // після зміни, не до
        return copy;
    }

    // Видати примірник читачу на вказану кількість днів
    public void IssueCopy(string copyId, string readerId, int days = 14)
    {
        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));
        if (days <= 0)
            throw new ArgumentOutOfRangeException(nameof(days), days, "Строк видачі має бути більшим за нуль");

        BookCopy copy = GetExisting(copyId);
        copy.Issue();          // якщо примірник уже виданий, тут буде виняток, і події не буде
        _store.Update(copy);

        DateOnly dueDate = DateOnly.FromDateTime(DateTime.Today).AddDays(days);
        OnChanged(copy, LendingChangeKind.CopyIssued, readerId.Trim(), dueDate);
    }

    // Повернути примірник на полицю
    public void ReturnCopy(string copyId)
    {
        BookCopy copy = GetExisting(copyId);
        copy.Return();         // якщо примірник не виданий, тут буде виняток, і події не буде
        _store.Update(copy);
        OnChanged(copy, LendingChangeKind.CopyReturned);
    }

    // Списати примірник
    public bool WriteOffCopy(string copyId)
    {
        BookCopy? copy = _store.GetById(copyId);
        if (copy is null || !_store.Remove(copyId))
            return false;

        OnChanged(copy, LendingChangeKind.CopyWrittenOff);
        return true;
    }

    public IReadOnlyList<BookCopy> All() => _store.List();

    public BookCopy? Find(string copyId) => _store.GetById(copyId);

    // Пошук: критерій задає той, хто викликає (Func повертає bool)
    public IReadOnlyList<BookCopy> FindAll(Func<BookCopy, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    private BookCopy GetExisting(string copyId) =>
        _store.GetById(copyId)
        ?? throw new InvalidOperationException($"Примірника з id={copyId} немає у сховищі.");
}