using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class LendingServiceTests
{
    private const string Isbn = "978-966-03-1234-5";

    [Fact] // 1) Варіант «Бібліотека»: повернення невиданого примірника — доменна помилка, стан не змінюється
    public void ReturnCopy_WhenNotIssued_ThrowsDomainException_AndKeepsState()
    {
        // Arrange
        var store = new InMemoryBookStore();
        var service = new LendingService(store);
        BookCopy copy = service.AddBook(Isbn);

        // Act + Assert
        Assert.Throws<DomainException>(() => service.ReturnCopy(copy.Id));
        Assert.False(store.GetById(copy.Id)!.IsIssued);
    }

    [Fact] // 2) Бізнес-правило «примірник уже видано»
    public void IssueCopy_WhenAlreadyIssued_ThrowsDomainException()
    {
        // Arrange
        var service = new LendingService(new InMemoryBookStore());
        BookCopy copy = service.AddBook(Isbn);
        service.IssueCopy(copy.Id, "R-001");

        // Act
        DomainException ex = Assert.Throws<DomainException>(() => service.IssueCopy(copy.Id, "R-002"));

        // Assert
        Assert.Contains("уже видано", ex.Message);
    }

    [Theory] // 3) Некоректний аргумент — ArgumentException (а не DomainException), стан не змінюється
    [InlineData("")]
    [InlineData("   ")]
    public void IssueCopy_WithEmptyReader_ThrowsArgumentException_AndKeepsState(string readerId)
    {
        // Arrange
        var store = new InMemoryBookStore();
        var service = new LendingService(store);
        BookCopy copy = service.AddBook(Isbn);

        // Act + Assert
        Assert.Throws<ArgumentException>(() => service.IssueCopy(copy.Id, readerId));
        Assert.False(store.GetById(copy.Id)!.IsIssued);
    }

    [Fact] // 4) Подія з лаби 6: піднімається рівно один раз і з правильними даними
    public void AddBook_RaisesChangedExactlyOnce()
    {
        // Arrange
        var service = new LendingService(new InMemoryBookStore());
        var events = new List<LendingChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        // Act
        BookCopy copy = service.AddBook(Isbn);

        // Assert
        LendingChangedEventArgs raised = Assert.Single(events);
        Assert.Equal(LendingChangeKind.BookAdded, raised.Kind);
        Assert.Equal(copy.Id, raised.CopyId);
    }

    [Fact] // 5) Невдала операція не піднімає подію
    public void IssueCopy_WhenCopyMissing_ThrowsDomainException_AndRaisesNoEvent()
    {
        // Arrange
        var service = new LendingService(new InMemoryBookStore());
        var events = new List<LendingChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        // Act + Assert
        Assert.Throws<DomainException>(() => service.IssueCopy("C-404", "R-001"));
        Assert.Empty(events);
    }
}
