using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<BookCopy> Copies() => new List<BookCopy>
    {
        BookCopy.Create("C-001", "978-966-03-1234-5"),
        Issued(BookCopy.Create("C-002", "978-966-03-1234-5")),
        BookCopy.Create("C-003", "978-966-03-1234-5"),
        BookCopy.Create("C-004", "978-966-03-2345-6"),
        Issued(BookCopy.Create("C-005", "978-966-03-2345-6")),
        BookCopy.Create("C-006", "978-966-03-3456-7"),
        BookCopy.Create("C-007", "978-966-03-3456-7"),
        Issued(BookCopy.Create("C-008", "978-966-03-4567-8")),
        BookCopy.Create("C-009", "978-966-03-4567-8"),
        BookCopy.Create("C-010", "978-966-03-5678-9"),
        BookCopy.Create("C-011", "978-966-03-6789-0"),
        Issued(BookCopy.Create("C-012", "978-966-03-7890-1")),
        BookCopy.Create("C-013", "978-966-03-7890-1"),
        BookCopy.Create("C-014", "978-966-03-8901-2"),
        BookCopy.Create("C-015", "978-966-03-9012-3"),
        BookCopy.Create("C-016", "978-966-03-0123-4"),
    };

    private static BookCopy Issued(BookCopy copy)
    {
        copy.Issue();
        return copy;
    }
}