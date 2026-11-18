using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    // 1) Топ-N читачів за кількістю видач.
    public static IReadOnlyList<ReaderLine> TopReaders(IEnumerable<Loan> loans, int n) =>
        loans.GroupBy(l => l.ReaderId, StringComparer.OrdinalIgnoreCase)
             .Select(g => new ReaderLine(g.Key, g.Count(), g.Count(l => !l.IsClosed)))
             .OrderByDescending(r => r.Loans)
             .ThenBy(r => r.ReaderId, StringComparer.Ordinal)
             .Take(n)
             .ToList();

    // 2) Запит прострочених видач. Навмисно БЕЗ ToList: повертає відкладений запит,
    //    який виконується лише під час перебирання (Count, ToList, foreach). Використовується в демо.
    public static IEnumerable<Loan> OverdueQuery(IEnumerable<Loan> loans, DateOnly today) =>
        loans.Where(l => l.IsOverdue(today));

    // 3) Звіт про прострочені видачі: Where → OrderBy за датою повернення → ThenBy за id → Select у DTO.
    public static IReadOnlyList<OverdueLine> Overdue(IEnumerable<Loan> loans, DateOnly today) =>
        OverdueQuery(loans, today)
            .OrderBy(l => l.DueDate)
            .ThenBy(l => l.Id, StringComparer.Ordinal)
            .Select(l => new OverdueLine(l.Id, l.CopyId, l.ReaderId, l.DueDate,
                                         today.DayNumber - l.DueDate.DayNumber))
            .ToList();

    // 4) Групування видань за жанром: у кожній групі рахуємо кількість видань, найменший і найбільший рік.
    public static IReadOnlyList<GenreSummary> ByGenre(IEnumerable<Book> books) =>
        books.GroupBy(b => b.Genre, StringComparer.OrdinalIgnoreCase)
             .Select(g => new GenreSummary(g.Key, g.Count(), g.Min(b => b.Year), g.Max(b => b.Year)))
             .OrderByDescending(s => s.Editions)
             .ThenBy(s => s.Genre, StringComparer.Ordinal)
             .ToList();

    // 5) Агрегати: скільки примірників припадає на одне видання.
    //    Порожнє джерело перевіряється заздалегідь, бо Average і Max на порожній колекції кидають виняток.
    public static (int Editions, int Copies, double Average, int Max) CopiesPerEdition(IEnumerable<BookCopy> copies)
    {
        List<int> perEdition = copies
            .GroupBy(c => c.Isbn, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Count())
            .ToList();

        return perEdition.Count == 0
            ? (0, 0, 0d, 0)
            : (perEdition.Count, perEdition.Sum(), perEdition.Average(), perEdition.Max());
    }
}
