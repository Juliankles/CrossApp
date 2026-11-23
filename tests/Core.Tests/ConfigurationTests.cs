using Core.Configuration;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class ConfigurationTests
{
    [Fact] // 12) Строк видачі з конфігурації (Library:LoanDays) потрапляє в дату повернення
    public void IssueCopy_UsesLoanDaysFromConfiguration()
    {
        // Arrange
        var service = new LendingService(new InMemoryBookStore(), loanDays: 30);
        var events = new List<LendingChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);
        BookCopy copy = service.AddBook("978-966-03-1234-5");

        // Act
        service.IssueCopy(copy.Id, "R-001");

        // Assert
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today).AddDays(30), events[^1].DueDate);
    }

    [Fact] // 13) Значення AppOptions за замовчуванням, якщо в конфігурації нічого не задано
    public void AppOptions_HasSafeDefaults()
    {
        var options = new AppOptions();

        Assert.Equal("data", options.Storage.DataPath);
        Assert.Equal("books.json", options.Storage.FileName);
        Assert.Equal(14, options.Library.LoanDays);
    }
}
