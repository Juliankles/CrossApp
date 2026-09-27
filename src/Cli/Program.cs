using Core;
using Core.Dto;
using Core.Import;
using Core.Domain;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.OutputEncoding = System.Text.Encoding.UTF8;
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

Console.OutputEncoding = System.Text.Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();
Console.WriteLine("CrossApp – інформація про середовище");
Console.WriteLine("Студентка: Лесняк Юліана, група ФЕІ-34");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"ОС              : {report.OsDescription}");
Console.WriteLine($"Runtime         : {report.FrameworkDescription}");
Console.WriteLine($"RID             : {report.DetectedRid}");
Console.WriteLine(new string('-', 52));

ImportResult<BookDto> result = BookCsvImporter.Load(path);

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (BookDto b in result.Items.Take(5))
    Console.WriteLine($"  {b.Id,-6} {b.Isbn,-20} {b.Title,-26} {b.Year}");

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
        Console.WriteLine($"  ! {e}");
}

Console.WriteLine();
Console.WriteLine("=== Сценарій 1: успіх ===");

BookCopy copy = BookCopy.Create("C-001", "978-966-03-1234-5");
Console.WriteLine(copy);

Loan loan = Loan.Open("L-001", copy.Id, "R-001", new DateOnly(2026, 9, 1));
copy.Issue();
Console.WriteLine(copy);
Console.WriteLine(loan);

loan.Close(new DateOnly(2026, 9, 15));
copy.Return();
Console.WriteLine(copy);
Console.WriteLine(loan);

Console.WriteLine();
Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");

BookCopy copy2 = BookCopy.Create("C-002", "978-966-03-9999-9");
copy2.Issue();

TryDo("повторна видача вже виданого примірника", () => copy2.Issue());
TryDo("порожній ISBN", () => BookCopy.Create("C-003", " "));
TryDo("повернення раніше дати видачі",
    () => Loan.Open("L-002", copy2.Id, "R-002", new DateOnly(2026, 9, 10))
              .Close(new DateOnly(2026, 9, 5)));

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}