using Core.Domain;

namespace Core;

public static class SampleData
{
    // Видання: 12 записів, 6 різних жанрів (щоб GroupBy показав кілька груп)
    public static IReadOnlyList<Book> Books() => new List<Book>
    {
        Book.Create("978-966-03-1234-5", "Кобзар", "Т. Шевченко", 1840, "Поезія"),
        Book.Create("978-966-03-2345-6", "Тіні забутих предків", "М. Коцюбинський", 1912, "Повість"),
        Book.Create("978-966-03-3456-7", "Лісова пісня", "Леся Українка", 1911, "Драма"),
        Book.Create("978-966-03-4567-8", "Захар Беркут", "І. Франко", 1883, "Повість"),
        Book.Create("978-966-03-5678-9", "Intermezzo", "М. Коцюбинський", 1908, "Новела"),
        Book.Create("978-966-03-6789-0", "Кайдашева сім'я", "І. Нечуй-Левицький", 1879, "Повість"),
        Book.Create("978-966-03-7890-1", "Хіба ревуть воли, як ясла повні?", "Панас Мирний", 1880, "Роман"),
        Book.Create("978-966-03-8901-2", "Енеїда", "І. Котляревський", 1798, "Поема"),
        Book.Create("978-966-03-9012-3", "Земля", "О. Кобилянська", 1902, "Роман"),
        Book.Create("978-966-03-0123-4", "Сад Гетсиманський", "І. Багряний", 1950, "Роман"),
        Book.Create("978-966-03-1111-1", "Наталка Полтавка", "І. Котляревський", 1819, "Драма"),
        Book.Create("978-966-03-2222-2", "Камінний хрест", "В. Стефаник", 1900, "Новела"),
    };

    // Примірники: 20 записів, 6 із них видано (C-002, C-005, C-008, C-012, C-017, C-019)
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
        Issued(BookCopy.Create("C-017", "978-966-03-1111-1")),
        BookCopy.Create("C-018", "978-966-03-1111-1"),
        Issued(BookCopy.Create("C-019", "978-966-03-2222-2")),
        BookCopy.Create("C-020", "978-966-03-1234-5"),
    };

    // Видачі: 18 записів — 12 закритих і 6 відкритих (з них 3 прострочені).
    // Дати рахуються від сьогоднішнього дня, тому результат звітів однаковий у будь-який день.
    public static IReadOnlyList<Loan> Loans(DateOnly today) => new List<Loan>
    {
        Closed("L-001", "C-001", "R-001", today, issuedDaysAgo: 120, returnedDaysAgo: 110),
        Closed("L-002", "C-004", "R-002", today, issuedDaysAgo: 120, returnedDaysAgo: 105),
        Closed("L-003", "C-006", "R-001", today, issuedDaysAgo: 100, returnedDaysAgo: 90),
        Closed("L-004", "C-010", "R-003", today, issuedDaysAgo: 95, returnedDaysAgo: 80),
        Closed("L-005", "C-011", "R-002", today, issuedDaysAgo: 90, returnedDaysAgo: 85),
        Closed("L-006", "C-014", "R-004", today, issuedDaysAgo: 85, returnedDaysAgo: 70),
        Closed("L-007", "C-003", "R-001", today, issuedDaysAgo: 80, returnedDaysAgo: 75),
        Closed("L-008", "C-009", "R-005", today, issuedDaysAgo: 75, returnedDaysAgo: 60),
        Closed("L-009", "C-013", "R-003", today, issuedDaysAgo: 70, returnedDaysAgo: 58),
        Closed("L-010", "C-015", "R-006", today, issuedDaysAgo: 65, returnedDaysAgo: 50),
        Closed("L-011", "C-016", "R-002", today, issuedDaysAgo: 60, returnedDaysAgo: 49),
        Closed("L-012", "C-007", "R-007", today, issuedDaysAgo: 55, returnedDaysAgo: 45),

        Loan.Open("L-013", "C-002", "R-001", today.AddDays(-30)),   // прострочена на 16 днів
        Loan.Open("L-014", "C-005", "R-003", today.AddDays(-20)),   // прострочена на 6 днів
        Loan.Open("L-015", "C-008", "R-002", today.AddDays(-5)),
        Loan.Open("L-016", "C-012", "R-001", today.AddDays(-45)),   // прострочена на 31 день
        Loan.Open("L-017", "C-017", "R-005", today.AddDays(-10)),
        Loan.Open("L-018", "C-019", "R-004", today.AddDays(-3)),
    };

    private static BookCopy Issued(BookCopy copy)
    {
        copy.Issue();
        return copy;
    }

    // Створює видачу, яку вже закрили (книжку повернули)
    private static Loan Closed(string id, string copyId, string readerId,
                               DateOnly today, int issuedDaysAgo, int returnedDaysAgo)
    {
        Loan loan = Loan.Open(id, copyId, readerId, today.AddDays(-issuedDaysAgo));
        loan.Close(today.AddDays(-returnedDaysAgo));
        return loan;
    }
}
