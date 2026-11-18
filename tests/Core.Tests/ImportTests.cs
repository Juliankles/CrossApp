using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class ImportTests
{
    [Fact] // 9) Скасування: токен скасовано заздалегідь — імпорт навіть не відкриває файл
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var service = new LendingService(new InMemoryBookStore());

        // Act + Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), "any.json", cts.Token));
    }

    [Fact] // 10) Зіпсований файл: єдиний тест із диском — тимчасовий файл, видаляється у finally
    public async Task LoadAsync_WithInvalidRecord_ThrowsDomainException_WithRecordNumber()
    {
        // Arrange
        string path = Path.Combine(Path.GetTempPath(), $"crossapp-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(path,
                """
                [
                  { "Id": "C-1", "Isbn": "978-966-03-1234-5", "IsIssued": false },
                  { "Id": "C-2", "Isbn": "", "IsIssued": false }
                ]
                """);

            // Act
            DomainException ex = await Assert.ThrowsAsync<DomainException>(
                () => new JsonCatalogImporter().LoadAsync(path));

            // Assert
            Assert.Contains("Запис 2", ex.Message);
            Assert.IsType<ArgumentException>(ex.InnerException);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact] // 11) Повторний імпорт не дублює записи; подія — на кожен доданий примірник. Диск не потрібен
    public async Task ImportAsync_IsIdempotent_AndRaisesEventPerAddedCopy()
    {
        // Arrange
        var store = new InMemoryBookStore();
        var service = new LendingService(store);
        var events = new List<LendingChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);
        var importer = new FakeImporter(
            BookCopy.Create("C-1", "978-966-03-1234-5"),
            BookCopy.Create("C-2", "978-966-03-2345-6"));

        // Act
        int first = await service.ImportAsync(importer, "ignored.json");
        int second = await service.ImportAsync(importer, "ignored.json");

        // Assert
        Assert.Equal(2, first);
        Assert.Equal(0, second);
        Assert.Equal(2, store.List().Count);
        Assert.Equal(2, events.Count);
    }

    // Підставний імпортер: повертає готовий список і не читає жодного файлу
    private sealed class FakeImporter(params BookCopy[] items) : ICatalogImporter
    {
        public Task<IReadOnlyList<BookCopy>> LoadAsync(string path, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<BookCopy>>(items);
    }
}
