using Core.Domain;

namespace Core.Tests;

// Маленький фіксований набір даних лише для тестів (не SampleData з проєкту Core).
// Дати фіксовані, тому результат тестів не залежить від дня запуску.
internal static class TestData
{
    public static readonly DateOnly Start = new(2026, 1, 1);

    // 5 видач: у R-1 і R-2 по 2 видачі (нічия для перевірки ThenBy), у R-3 — одна.
    // Відкриті: L-1 (повернути до 15.01) і L-2 (повернути до 25.01).
    public static IReadOnlyList<Loan> Loans() => new List<Loan>
    {
        Loan.Open("L-1", "C-1", "R-2", Start),
        Loan.Open("L-2", "C-2", "R-1", Start.AddDays(10)),
        Closed("L-3", "C-3", "R-1", Start, Start.AddDays(5)),
        Closed("L-4", "C-4", "R-2", Start, Start.AddDays(7)),
        Closed("L-5", "C-5", "R-3", Start, Start.AddDays(3)),
    };

    private static Loan Closed(string id, string copyId, string readerId, DateOnly issuedOn, DateOnly returnedOn)
    {
        Loan loan = Loan.Open(id, copyId, readerId, issuedOn);
        loan.Close(returnedOn);
        return loan;
    }
}
