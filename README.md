# CrossApp

Наскрізний навчальний проєкт з крос-платформного програмування.

## Предметна область
Бібліотека.

Сутності:
- **Book** — видання (назва, автор, рік)
- **BookCopy** — примірник конкретного видання (інвентарний номер, стан)
- **Reader** — читач (ім'я, контакти)
- **Loan** — видача примірника читачу (дата видачі, дата повернення)

Призначення: облік видач примірників книг читачам і повернень.

## Запуск

```
dotnet build
dotnet run --project src/Cli
```


## Середовище

.NET SDK 8.0.424 / 9.0.311 / 10.0.401, Windows x64

## Лабораторна 2: Core + Cli, multi-targeting, публікація

### Оновлена структура solution

CrossApp/
├── .gitignore
├── CrossApp.sln
├── README.md
└── src/
├── Core/
│ ├── Core.csproj
│ └── EnvironmentInfo.cs
└── Cli/
├── Cli.csproj (містить ProjectReference на Core)
└── Program.cs


### Команди

```
dotnet build
dotnet run --project src/Cli
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -o publish-self-contained
dotnet publish src/Cli -c Release -r win-x64 --self-contained false -o publish-framework-dependent
.\publish-self-contained\Cli.exe
.\publish-framework-dependent\Cli.exe
```


### Порівняння режимів публікації

| RID | Режим | Розмір publish | Кількість файлів | Потрібен runtime |
|---|---|---|---|---|
| win-x64 | self-contained | ~76,9 МБ | 194 | ні |
| win-x64 | framework-dependent | ~0,19 МБ | 7 | так (.NET 10) |

**Self-contained** публікація містить код застосунку, залежності NuGet і копію .NET runtime (у тому числі `coreclr.dll`, `System.Private.CoreLib.dll` та інші системні бібліотеки) — тому працює на машині без встановленого .NET, але має значно більший розмір і прив'язана до конкретної RID.

**Framework-dependent** публікація містить лише код (`Cli.dll`, `Core.dll`) і конфігураційні файли, без runtime — тому має малий розмір (у нашому випадку приблизно в 400 разів менший), але вимагає встановленого сумісного .NET runtime (.NET 10) на машині користувача.

### Multi-targeting

Проєкт Core налаштовано на компіляцію під кілька TFM одночасно через `<TargetFrameworks>net8.0;net10.0</TargetFrameworks>` у файлі `Core.csproj`. Проєкт Cli залишається з єдиним `<TargetFramework>net10.0</TargetFramework>`, оскільки саме бібліотека (Core) призначена для повторного використання в різних проєктах з різними версіями .NET, тоді як Cli — конкретний застосунок з однією цільовою платформою.

### Домовленість про структуру Core (на весь семестр)
- `Core/Dto/` — record-типи формату даних (тиждень 3)
- `Core/Domain/` — сутності з поведінкою та інваріантами (тиждень 4)
- `Core/Storage/` — реалізації сховищ (тиждень 5)


## Лабораторна 3: Records, pattern matching, імпорт CSV

### Формат файлу даних

Файл: `data/sample.csv`
Роздільник: `;` (крапка з комою)
Кодування: UTF-8
Перший рядок — заголовок (id;isbn;title;year)

### Record-типи

BookDto(string Id, string Isbn, string Title, int Year, string? Author = null) — Author є nullable, оскільки в джерельних даних автор не завжди вказаний; решта полів обов'язкові.

ReaderDto(string Id, string FullName, string? Contact = null) — другий доменний тип (читач бібліотеки), підготовлений як DTO для майбутнього використання (сховище читачів, лабораторна 5); Contact є nullable, оскільки не завжди відомий одразу при реєстрації.

ImportResult<T>(IReadOnlyList<T> Items, IReadOnlyList<string> Errors) — узагальнений тип, що повертає одразу і успішно розібрані дані, і перелік помилок з номерами рядків, не перериваючи імпорт на першому дефекті.

### Чому record, а не class

BookDto і ImportResult представляють дані (незмінний "знімок" прочитаного рядка чи результату імпорту), а не сутність з власною поведінкою і життєвим циклом — тому їм природно підходить record з рівністю за значенням, а не class.

### Команди

```
dotnet run --project src/Cli
dotnet run --project src/Cli -- data/sample.csv
dotnet run --project src/Cli -- data/no-such-file.csv
```


## Лабораторна 4: Доменна модель — сутності, інваріанти, інкапсуляція

### Обрані сутності

BookCopy — примірник книги (властивості: Id, Isbn, IsIssued).
Loan — видача примірника читачу (властивості: Id, CopyId, ReaderId, IssuedOn, ReturnedOn).

### Інваріанти

| Правило | Тип винятку | Метод |
|---|---|---|
| Ідентифікатор примірника не порожній | ArgumentException | BookCopy.Create |
| ISBN не порожній | ArgumentException | BookCopy.Create |
| Не можна повторно видати вже виданий примірник | InvalidOperationException | BookCopy.Issue |
| Не можна повернути невиданий примірник | InvalidOperationException | BookCopy.Return |
| Ідентифікатори видачі, примірника, читача не порожні | ArgumentException | Loan.Open |
| Не можна закрити вже закриту видачу | InvalidOperationException | Loan.Close |
| Дата повернення не раніше дати видачі | ArgumentOutOfRangeException | Loan.Close |

### Чому DTO і сутність — різні типи

BookDto/BookCopyDto переносять дані (з файлу чи для збереження) і не гарантують коректності — файл може містити будь-що. BookCopy і Loan — доменні сутності, які самі захищають свої правила: створити чи змінити їх в обхід перевірок неможливо. Зв'язок між ними — методи ToDto()/FromDto(), причому FromDto проходить ті самі перевірки, що й Create.