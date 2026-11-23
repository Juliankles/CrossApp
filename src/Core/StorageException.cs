namespace Core;

// Технічна помилка сховища (пошкоджений файл даних тощо).
// Не маскується під DomainException; початковий виняток зберігається в InnerException.
public sealed class StorageException : Exception
{
    public StorageException(string message, Exception inner) : base(message, inner) { }
}
