using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();
Console.WriteLine("CrossApp – інформація про середовище");
Console.WriteLine("Студентка: Лесняк Юліана, група ФЕІ-34");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"ОС              : {report.OsDescription}");
Console.WriteLine($"Runtime         : {report.FrameworkDescription}");
Console.WriteLine($"RID             : {report.DetectedRid}");
Console.WriteLine(new string('-', 52));

// ===== Composition root: єдине місце, де створюються конкретні класи =====
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "books.json");

IBookStore store = useFile
    ? new FileBookStore(dataPath)
    : new InMemoryBookStore(SampleData.Copies());

var service = new LendingService(store);


Console.WriteLine($"Сховище: {store.GetType().Name}");
if (useFile)
    Console.WriteLine($"Файл: {dataPath}");

Console.WriteLine();
Console.WriteLine("=== Сценарій: додати, змінити, знайти ===");

BookCopy created = service.AddBook("978-966-03-1234-5");
Console.WriteLine($"Додано:            {created}");

service.IssueCopy(created.Id);
Console.WriteLine($"Після видачі:      {service.Find(created.Id)}");

BookCopy second = service.AddBook("978-966-03-3456-7");
service.IssueCopy(second.Id);
service.ReturnCopy(second.Id);
Console.WriteLine($"Видано й повернуто: {service.Find(second.Id)}");

Console.WriteLine();
Console.WriteLine($"Усі примірники у сховищі ({service.All().Count}):");
foreach (BookCopy c in service.All())
    Console.WriteLine($"  {c}");

Console.WriteLine();
Console.WriteLine("=== Сценарій відмови ===");
TryDo("видача неіснуючого примірника", () => service.IssueCopy("C-404"));
TryDo("повернення неіснуючого примірника", () => service.ReturnCopy("C-404"));
TryDo("додавання примірника з уже наявним id",
    () => store.Add(BookCopy.Create(created.Id, "978-966-03-1234-5")));

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: помилки не виникло");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"  {title}: відмова — {ex.Message}");
    }
}