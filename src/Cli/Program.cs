using System.Runtime.InteropServices;

Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
Console.WriteLine("Студентка: Лесняк Юлiана, група ФЕI-34");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"ОС (OSDescription) : {RuntimeInformation.OSDescription}");
Console.WriteLine($"ОС (Environment) : {Environment.OSVersion}");
Console.WriteLine($"Архiтектура процесу : {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"Версiя .NET (CLR) : {Environment.Version}");
Console.WriteLine($"Runtime : {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Каталог застосунку : {AppContext.BaseDirectory}");
Console.WriteLine($"Поточний каталог : {Environment.CurrentDirectory}");
Console.WriteLine(new string('-', 52));
Console.WriteLine("Предметна область: Бiблiотека (Book, BookCopy, Reader, Loan)");