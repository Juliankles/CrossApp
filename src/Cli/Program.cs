using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
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

// ===== Composition root =====
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "books.json");

IBookStore store = useFile
    ? new FileBookStore(dataPath)
    : new InMemoryBookStore(SampleData.Copies());

var service = new LendingService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");
if (useFile)
    Console.WriteLine($"Файл: {dataPath}");

// ===== Лог-файл =====
string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
Console.WriteLine($"Лог: {logPath}");

// ===== Підписник 1: локальний метод (консоль + файл) =====
void OnLendingChanged(object? sender, LendingChangedEventArgs e)
{
    string details = e.Kind switch
    {
        LendingChangeKind.BookAdded      => $"додано примірник видання {e.Isbn}",
        LendingChangeKind.CopyIssued     => $"видано читачу {e.ReaderId} до {e.DueDate:dd.MM.yyyy}",
        LendingChangeKind.CopyReturned   => "повернуто на полицю",
        LendingChangeKind.CopyWrittenOff => "списано",
        _ => e.Kind.ToString()
    };

    string line = $"{e.At:HH:mm:ss} {e.Kind,-14} {e.CopyId} {details}";
    Console.WriteLine(line);

    try
    {
        File.AppendAllText(logPath, line + Environment.NewLine);
    }
    catch (IOException ex)
    {
        Console.WriteLine($"  (не вдалося записати в лог: {ex.Message})");
    }
}

service.Changed += OnLendingChanged;

// ===== Підписник 2: лямбда із замиканням над змінною changes =====
int changes = 0;
service.Changed += (_, _) => changes++;

// ===== Сценарій: успішні операції =====
Console.WriteLine();
Console.WriteLine("=== Сценарій: події видачі ===");

BookCopy copy = service.AddBook("978-966-03-1234-5");
service.IssueCopy(copy.Id, "R-001");
service.ReturnCopy(copy.Id);
Console.WriteLine($"Подій отримано: {changes}");

// ===== Сценарій відмови: виняток є, події немає =====
Console.WriteLine();
Console.WriteLine("=== Сценарій відмови: виняток є, події немає ===");
TryDo("видача неіснуючого примірника", () => service.IssueCopy("C-404", "R-001"));
TryDo("видача без читача", () => service.IssueCopy(copy.Id, ""));
TryDo("повернення примірника, що вже на полиці", () => service.ReturnCopy(copy.Id));
Console.WriteLine($"Подій отримано: {changes}");

// ===== Відписка =====
Console.WriteLine();
Console.WriteLine("=== Після відписки (-=) ===");
service.Changed -= OnLendingChanged;
service.IssueCopy(copy.Id, "R-002");   // тиха операція: дані змінюються, рядка події немає
Console.WriteLine($"Стан примірника: {service.Find(copy.Id)}");
Console.WriteLine($"Подій отримано: {changes}");   // +1 лише від лямбди-лічильника

// ===== Func замість події: FindAll =====
Console.WriteLine();
Console.WriteLine("=== FindAll(c => c.IsIssued): видані примірники ===");
IReadOnlyList<BookCopy> issued = service.FindAll(c => c.IsIssued);
foreach (BookCopy c in issued)
    Console.WriteLine($"  {c}");
Console.WriteLine($"Усього виданих: {issued.Count}");

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: помилки не виникло");
    }
    catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
    {
        Console.WriteLine($"  {title}: помилка — {ex.Message}");
    }
}