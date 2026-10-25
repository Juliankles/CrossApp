namespace Core.Domain;

public sealed class Loan
{
    private DateOnly? _returnedOn;

    public string Id { get; }
    public string CopyId { get; }
    public string ReaderId { get; }
    public DateOnly IssuedOn { get; }
    public DateOnly? ReturnedOn => _returnedOn;
    public bool IsClosed => _returnedOn is not null;

    private Loan(string id, string copyId, string readerId, DateOnly issuedOn)
    {
        Id = id;
        CopyId = copyId;
        ReaderId = readerId;
        IssuedOn = issuedOn;
        _returnedOn = null;
    }

    public static Loan Open(string id, string copyId, string readerId, DateOnly issuedOn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(copyId))
            throw new ArgumentException("Ідентифікатор примірника обов'язковий", nameof(copyId));

        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));

        return new Loan(id.Trim(), copyId.Trim(), readerId.Trim(), issuedOn);
    }

    public void Close(DateOnly returnedOn)
    {
        if (IsClosed)
            throw new InvalidOperationException($"Видача {Id} вже закрита");

        if (returnedOn < IssuedOn)
            throw new ArgumentOutOfRangeException(
                nameof(returnedOn), returnedOn,
                $"Дата повернення не може бути раніше дати видачі ({IssuedOn})");

        _returnedOn = returnedOn;
    }

    public override string ToString() =>
        $"Видача {Id}: примірник {CopyId} читачу {ReaderId}, видано {IssuedOn}" +
        (IsClosed ? $", повернуто {ReturnedOn}" : ", відкрита");
}