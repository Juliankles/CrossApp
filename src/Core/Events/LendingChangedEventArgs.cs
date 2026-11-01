namespace Core.Events;

public sealed class LendingChangedEventArgs(
    string copyId,
    string isbn,
    LendingChangeKind kind,
    string? readerId = null,
    DateOnly? dueDate = null) : EventArgs
{
    public string CopyId { get; } = copyId;
    public string Isbn { get; } = isbn;
    public LendingChangeKind Kind { get; } = kind;
    public string? ReaderId { get; } = readerId;
    public DateOnly? DueDate { get; } = dueDate;
    public DateTimeOffset At { get; } = DateTimeOffset.Now;
}