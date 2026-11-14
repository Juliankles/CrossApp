using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Reports;
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

// Composition root
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "books.json");

IBookStore store = useFile
    ? new FileBookStore(dataPath)
    : new InMemoryBookStore(SampleData.Copies());

var service = new LendingService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");
if (useFile)
    Console.WriteLine($"Файл: {dataPath}");

// Лог-файл (лаба 6)
string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

// Підписник 1: локальний метод (консоль + файл)
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

// Підписник 2: лямбда із замиканням над змінною changes
int changes = 0;
service.Changed += (_, _) => changes++;

// Дані для звітів (лаба 7)
DateOnly today = DateOnly.FromDateTime(DateTime.Today);
IReadOnlyList<BookCopy> copies = service.All();          // примірники — зі сховища (пам'ять або файл)
IReadOnlyList<Book> books = SampleData.Books();          // видання
IReadOnlyList<Loan> loans = SampleData.Loans(today);     // видачі

// Підкоманда: перший аргумент, що НЕ починається з "--" (щоб --file не вважався назвою звіту)
string command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";
Console.WriteLine($"Команда: {command}");
Console.WriteLine();

switch (command)
{
    case "report-top":
        Console.WriteLine("=== Топ-5 читачів за кількістю видач ===");
        PrintReaders(ReportService.TopReaders(loans, 5));
        break;

    case "report-overdue":
        Console.WriteLine($"=== Прострочені видачі на {today:dd.MM.yyyy} ===");
        PrintOverdue(ReportService.Overdue(loans, today));
        break;

    case "report-genres":
        Console.WriteLine("=== Видання за жанрами ===");
        PrintGenres(ReportService.ByGenre(books));
        break;

    case "report-sum":
        Console.WriteLine("=== Примірники на одне видання ===");
        PrintTotals(ReportService.CopiesPerEdition(copies));
        break;

    case "report-lazy":
        RunLazyDemo();
        break;

    case "report-empty":
        RunEmptyCheck();
        break;

    case "events":
        RunEventsScenario();
        break;

    default:
        Console.WriteLine("Команди: report-top | report-overdue | report-genres | report-sum | report-lazy | report-empty | events");
        break;
}

return 0;

// Демо відкладеного виконання
void RunLazyDemo()
{
    Console.WriteLine("=== Демо відкладеного виконання ===");

    List<Loan> source = loans.ToList();                                      // локальна копія, яку будемо змінювати
    IEnumerable<Loan> query = ReportService.OverdueQuery(source, today);     // запит побудовано, але ще НЕ виконано

    Console.WriteLine($"Прострочених до змін:     {query.Count()}");          // запит виконується тут уперше

    source.Add(Loan.Open("L-999", "C-020", "R-008", today.AddDays(-40)));    // додали в джерело ще одну прострочену видачу
    Console.WriteLine($"Прострочених після змін:  {query.Count()}");          // запит виконується знову — вже на зміненому джерелі

    List<Loan> snapshot = query.ToList();                                    // результат зафіксовано (матеріалізовано)
    source.Clear();
    Console.WriteLine($"Після очищення джерела:   запит дає {query.Count()}, у знімку лишилось {snapshot.Count}");
}

// Звіти на порожніх даних: мають працювати без винятків
void RunEmptyCheck()
{
    Console.WriteLine("=== Звіти на порожніх даних ===");
    Console.WriteLine("Топ читачів:");
    PrintReaders(ReportService.TopReaders(Array.Empty<Loan>(), 5));
    Console.WriteLine("Прострочені:");
    PrintOverdue(ReportService.Overdue(Array.Empty<Loan>(), today));
    Console.WriteLine("Жанри:");
    PrintGenres(ReportService.ByGenre(Array.Empty<Book>()));
    Console.WriteLine("Примірники на видання:");
    PrintTotals(ReportService.CopiesPerEdition(Array.Empty<BookCopy>()));
}

// Сценарій подій з лаби 6 (команда events)
void RunEventsScenario()
{
    Console.WriteLine($"Лог: {logPath}");
    Console.WriteLine();
    Console.WriteLine("=== Сценарій: події видачі ===");

    BookCopy copy = service.AddBook("978-966-03-1234-5");
    service.IssueCopy(copy.Id, "R-001");
    service.ReturnCopy(copy.Id);
    Console.WriteLine($"Подій отримано: {changes}");

    Console.WriteLine();
    Console.WriteLine("=== Сценарій відмови: виняток є, події немає ===");
    TryDo("видача неіснуючого примірника", () => service.IssueCopy("C-404", "R-001"));
    TryDo("видача без читача", () => service.IssueCopy(copy.Id, ""));
    TryDo("повернення примірника, що вже на полиці", () => service.ReturnCopy(copy.Id));
    Console.WriteLine($"Подій отримано: {changes}");

    Console.WriteLine();
    Console.WriteLine("=== Після відписки (-=) ===");
    service.Changed -= OnLendingChanged;
    service.IssueCopy(copy.Id, "R-002");
    Console.WriteLine($"Стан примірника: {service.Find(copy.Id)}");
    Console.WriteLine($"Подій отримано: {changes}");

    Console.WriteLine();
    Console.WriteLine("=== FindAll(c => c.IsIssued): видані примірники ===");
    IReadOnlyList<BookCopy> issued = service.FindAll(c => c.IsIssued);
    foreach (BookCopy c in issued)
        Console.WriteLine($"  {c}");
    Console.WriteLine($"Усього виданих: {issued.Count}");
}

// Вивід звітів: тут лише форматування, жодних запитів
static void PrintReaders(IReadOnlyList<ReaderLine> lines)
{
    if (lines.Count == 0) { Console.WriteLine("  (немає даних)"); return; }

    Console.WriteLine("  Читач     Видач   Зараз на руках");
    foreach (ReaderLine r in lines)
        Console.WriteLine($"  {r.ReaderId,-8} {r.Loans,6} {r.Open,16}");
}

static void PrintOverdue(IReadOnlyList<OverdueLine> lines)
{
    if (lines.Count == 0) { Console.WriteLine("  (немає даних)"); return; }

    Console.WriteLine("  Видача  Примірник  Читач   Повернути до  Днів прострочки");
    foreach (OverdueLine l in lines)
        Console.WriteLine($"  {l.LoanId,-7} {l.CopyId,-10} {l.ReaderId,-7} {l.DueDate:dd.MM.yyyy} {l.DaysOverdue,16}");
}

static void PrintGenres(IReadOnlyList<GenreSummary> lines)
{
    if (lines.Count == 0) { Console.WriteLine("  (немає даних)"); return; }

    foreach (GenreSummary g in lines)
        Console.WriteLine($"  {g.Genre,-8} видань: {g.Editions,2}   роки: {g.FromYear}–{g.ToYear}");
}

static void PrintTotals((int Editions, int Copies, double Average, int Max) t) =>
    Console.WriteLine($"  видань {t.Editions}, примірників {t.Copies}, у середньому {t.Average:F2} на видання, максимум {t.Max}");

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
