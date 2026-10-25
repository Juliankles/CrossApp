using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class LendingService(IBookStore store)
{
    private readonly IBookStore _store = store ?? throw new ArgumentNullException(nameof(store));

    // Додати новий примірник видання з указаним ISBN
    public BookCopy AddBook(string isbn)
    {
        string id = "C-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        BookCopy copy = BookCopy.Create(id, isbn); // інваріанти перевіряє сама сутність
        _store.Add(copy);
        return copy;
    }

    // Видати примірник читачу
    public void IssueCopy(string copyId)
    {
        BookCopy copy = GetExisting(copyId);
        copy.Issue();
        _store.Update(copy);
    }

    // Повернути примірник на полицю
    public void ReturnCopy(string copyId)
    {
        BookCopy copy = GetExisting(copyId);
        copy.Return();
        _store.Update(copy);
    }

    // Списати примірник (прибрати зі сховища)
    public bool WriteOffCopy(string copyId) => _store.Remove(copyId);

    public IReadOnlyList<BookCopy> All() => _store.List();

    public BookCopy? Find(string copyId) => _store.GetById(copyId);

    private BookCopy GetExisting(string copyId) =>
        _store.GetById(copyId)
        ?? throw new InvalidOperationException($"Примірника з id={copyId} немає у сховищі.");
}