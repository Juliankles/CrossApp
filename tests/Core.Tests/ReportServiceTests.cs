using Core.Domain;
using Core.Reports;
using Xunit;

namespace Core.Tests;

public class ReportServiceTests
{
    [Theory] // 6) Звіт прострочених на кількох датах: 10.01 — жодної, 20.01 — L-1, 30.01 — L-1 і L-2
    [InlineData(10, 0)]
    [InlineData(20, 1)]
    [InlineData(30, 2)]
    public void Overdue_CountsOnlyOpenLoansPastDueDate(int dayOfJanuary, int expected)
    {
        var today = new DateOnly(2026, 1, dayOfJanuary);

        IReadOnlyList<OverdueLine> result = ReportService.Overdue(TestData.Loans(), today);

        Assert.Equal(expected, result.Count);
    }

    [Fact] // 7) Детермінований топ-N: у R-1 і R-2 однаково по 2 видачі, порядок визначає ThenBy
    public void TopReaders_IsDeterministic() =>
        Assert.Equal(new[] { "R-1", "R-2" },
                     ReportService.TopReaders(TestData.Loans(), 2).Select(r => r.ReaderId));

    [Fact] // 8) Агрегати на порожньому джерелі: нулі, а не виняток
    public void CopiesPerEdition_OnEmptySource_ReturnsZeros() =>
        Assert.Equal((0, 0, 0d, 0), ReportService.CopiesPerEdition(Array.Empty<BookCopy>()));
}
