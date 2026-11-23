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

Файл: `data/sample.csv` (з лаби 9 — `samples/sample.csv`)
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


## Лабораторна 5: Сервісний шар — інтерфейс, два сховища, ручний DI

### Структура
- `Core/Abstractions/IBookStore.cs` — контракт сховища (List, GetById, Add, Update, Remove)
- `Core/Storage/InMemoryBookStore.cs` — сховище в пам'яті (Dictionary + початкові дані)
- `Core/Storage/FileBookStore.cs` — файлове сховище (JSON, кеш, запис після кожної зміни)
- `Core/Services/LendingService.cs` — бізнес-операції: AddBook, IssueCopy, ReturnCopy, WriteOffCopy
- `Core/SampleData.cs` — 16 примірників для демонстрації
- `Cli/Program.cs` — composition root: вибір реалізації сховища

### Схема залежностей
Cli → LendingService → IBookStore → InMemoryBookStore | FileBookStore

### Запуск

```
dotnet run --project src/Cli            — сховище в пам'яті, дані зникають після виходу
dotnet run --project src/Cli -- --file  — файлове сховище, дані зберігаються між запусками
```

Файл даних: `data/books.json` поруч зі зібраним застосунком (bin/Debug/net10.0/data/books.json).
Формат: JSON-список BookCopyDto (Id, Isbn, IsIssued); видання і примірники зберігаються в одному списку, видання визначається полем Isbn.

DI-контейнер (Microsoft.Extensions.DependencyInjection) не використовується: залежність передається через конструктор вручну в Program.cs.



## Лабораторна 6: Доменні події, делегати Action/Func, лямбди

### Структура
- `Core/Events/LendingChangeKind.cs` — вид події: BookAdded, CopyIssued, CopyReturned, CopyWrittenOff
- `Core/Events/LendingChangedEventArgs.cs` — дані події: CopyId, Isbn, Kind, ReaderId, DueDate, At
- `Core/Services/LendingService.cs` — подія `Changed`, метод `OnChanged`, пошук `FindAll(Func<BookCopy, bool>)`
- `Cli/Program.cs` — підписка, відписка, запис у консоль і в `logs/app.log`

### Як працює
Сервіс піднімає подію `Changed` лише після успішної зміни стану. Якщо інваріант кидає виняток, події немає.
Core не пише ні в консоль, ні у файл: це роблять підписники в Cli.

Підписники:
1. локальний метод `OnLendingChanged` — друкує рядок у консоль і дописує його в `logs/app.log`;
2. лямбда `(_, _) => changes++` — лічильник подій (замикання над змінною `changes`).

Після `service.Changed -= OnLendingChanged` операції виконуються, але рядків у консолі й лозі немає; лічильник продовжує рахувати.

### Запуск

```
dotnet run --project src/Cli
dotnet run --project src/Cli -- --file
```

Лог: `logs/app.log` поруч зі зібраним застосунком (bin/Debug/net10.0/logs/app.log), каталог створюється автоматично.



## Лабораторна 7: Звіти через LINQ

### Структура
- `Core/Domain/Book.cs` — видання (ISBN, назва, автор, рік, жанр)
- `Core/Domain/Loan.cs` — додано `DueDate` (дата видачі + 14 днів) і `IsOverdue(today)`
- `Core/SampleData.cs` — 12 видань (6 жанрів), 20 примірників, 18 видач (3 прострочені)
- `Core/Reports/ReaderLine.cs`, `OverdueLine.cs`, `GenreSummary.cs` — record-DTO звітів
- `Core/Reports/ReportService.cs` — усі LINQ-запити
- `Cli/Program.cs` — підкоманди; лише виклики ReportService і вивід, жодного Where/GroupBy

### Звіти
| Команда | Метод | Питання | Оператори |
|---|---|---|---|
| report-top | TopReaders | хто з читачів бере найбільше книжок (топ-5) | GroupBy, Select, OrderByDescending, ThenBy, Take |
| report-overdue | Overdue | які видачі прострочені | Where, OrderBy, ThenBy, Select |
| report-genres | ByGenre | скільки видань кожного жанру і з яких років | GroupBy, Count, Min, Max, OrderByDescending |
| report-sum | CopiesPerEdition | скільки в середньому примірників на одне видання | GroupBy, Sum, Average, Max |

Додаткові команди: `report-lazy` — демо відкладеного виконання; `report-empty` — усі звіти на порожніх даних; `events` — сценарій подій з лаби 6.

Видання і видачі беруться з SampleData (сховище для них з'явиться разом із базою даних), примірники — зі сховища через `service.All()`, тому `report-sum` працює і з `--file`.

### Запуск

```
dotnet run --project src/Cli -- report-top
dotnet run --project src/Cli -- report-overdue
dotnet run --project src/Cli -- report-genres
dotnet run --project src/Cli -- report-sum
dotnet run --project src/Cli -- report-lazy
dotnet run --project src/Cli -- --file report-sum
```

### Відкладене виконання
Метод `ReportService.OverdueQuery` повертає `IEnumerable<Loan>` без `ToList`, тому в момент виклику запит лише описується, а джерело не перебирається.
Запит виконується тоді, коли викликається завершальний оператор: `Count()`, `ToList()` або `foreach`, і щоразу заново.
У демо `report-lazy` перший `Count()` дав 3, а після додавання в джерело простроченої видачі L-999 другий `Count()` дав 4, бо запит виконався вже на зміненому списку.
Після `ToList()` результат зафіксовано: коли джерело очистили, запит дає 0, а у знімку лишилось 4 записи.



## Лабораторна 8: async/await, скасування, доменні винятки, unit-тести

### Структура
- `Core/DomainException.cs` — виняток для порушення бізнес-правил («примірник уже видано», «не перебуває у видачі», «видача вже закрита», «примірника немає», «id уже існує»)
- `Core/Abstractions/ICatalogImporter.cs` — асинхронний контракт імпорту каталогу
- `Core/Storage/JsonCatalogImporter.cs` — асинхронне читання JSON (`FileStream(useAsync: true)` + `JsonSerializer.DeserializeAsync`), перевірка скасування в циклі, `DomainException` з номером запису для поганих даних
- `Core/Services/LendingService.cs` — `ImportAsync(importer, path, ct)`: повторний імпорт не дублює записи, подія `BookAdded` на кожен доданий примірник
- `Cli/Program.cs` — top-level `await`, Ctrl+C через `Console.CancelKeyPress`, тайм-аут через `CancelAfter`, окремі `catch` для скасування, доменної та технічних помилок
- `samples/catalog.json` (17 примірників), `samples/catalog-bad.json` (третій запис без ISBN); до лаби 9 лежали в `data/`
- `tests/Core.Tests` — xUnit-тести; посилається лише на Core

### Види помилок
| Тип | Коли | Приклад |
|---|---|---|
| DomainException | порушено бізнес-правило або погані дані у файлі | повторна видача, повернення невиданого примірника, «Запис 3: ISBN не може бути порожнім» |
| ArgumentException | некоректний аргумент методу | порожній ідентифікатор читача в `IssueCopy` |
| IOException / JsonException | технічна помилка | немає файлу, файл не є JSON |
| OperationCanceledException | очікуване скасування, не помилка | Ctrl+C, тайм-аут |

### Імпорт
Файли-зразки копіюються поруч із програмою (`bin/.../samples`), тому команди працюють з будь-якої папки:

```
dotnet run --project src/Cli -- import
dotnet run --project src/Cli -- --file import
dotnet run --project src/Cli -- import-bad
dotnet run --project src/Cli -- import-missing
```

З `--file` імпорт іде у файлове сховище: перший запуск додає 17 записів, повторний — 0 (записи з такими id уже є).

### Як перевірити скасування

```
dotnet run --project src/Cli -- import-slow
```

Імпортер чекає 0,5 с на кожен запис, а тайм-аут становить 2 с, тому імпорт не встигає завершитися і виводиться «Імпорт скасовано.». Під час `import-slow` можна також натиснути Ctrl+C — результат той самий, без стектрейсу.

### Тести

```
dotnet test
```

Тести використовують `InMemoryBookStore` і власний набір `TestData` (не SampleData); єдиний тест із диском створює тимчасовий файл у `Path.GetTempPath()` і видаляє його у `finally`.

| Тест | Що перевіряє | Очікуваний результат |
|---|---|---|
| ReturnCopy_WhenNotIssued_ThrowsDomainException_AndKeepsState | повернення невиданого примірника | DomainException, примірник лишається на полиці |
| IssueCopy_WhenAlreadyIssued_ThrowsDomainException | повторна видача | DomainException «уже видано» |
| IssueCopy_WithEmptyReader_ThrowsArgumentException_AndKeepsState (2 випадки) | порожній читач | ArgumentException, стан не змінився |
| AddBook_RaisesChangedExactlyOnce | подія при додаванні | рівно одна подія BookAdded |
| IssueCopy_WhenCopyMissing_ThrowsDomainException_AndRaisesNoEvent | невдала операція | DomainException, подій немає |
| Overdue_CountsOnlyOpenLoansPastDueDate (3 випадки) | звіт прострочених на різні дати | 0, 1, 2 |
| TopReaders_IsDeterministic | топ-N при нічиїй | R-1, R-2 |
| CopiesPerEdition_OnEmptySource_ReturnsZeros | агрегати на порожніх даних | нулі без винятку |
| ImportAsync_WhenAlreadyCancelled_Throws | скасований токен | OperationCanceledException |
| LoadAsync_WithInvalidRecord_ThrowsDomainException_WithRecordNumber | зіпсований файл | DomainException «Запис 2», InnerException — ArgumentException |
| ImportAsync_IsIdempotent_AndRaisesEventPerAddedCopy | повторний імпорт | 2, потім 0; дві події |

### Команди для повернення до 8 лаби

```
git worktree add ..\CrossApp-lab08 lab08
cd ..\CrossApp-lab08
```

Прибрати папку після перегляду:

```
cd C:\dev\CrossApp
git worktree remove ..\CrossApp-lab08
```



## Лабораторна 9: Шляхи, JSON, конфігурація, Secrets

### Структура
- `Core/Configuration/AppOptions.cs` — налаштування: `EnvironmentName`, `Logging`, `Storage`, `Library`; `StorageOptions.ResolveFullPath` будує шлях через `Path.Combine`
- `Core/Storage/FileBookStore.cs` — шлях приходить із конфігурації; JSON у camelCase, кирилиця без `\uXXXX`; атомарний запис (`.tmp` + `File.Move`); пошкоджений файл — `StorageException`
- `Core/Storage/CatalogJsonFile.cs`, `Core/Dto/EditionDto.cs` — експорт та імпорт каталогу видань (isbn, title, author, year, copies)
- `Core/Storage/AtomicFile.cs`, `Core/Storage/JsonFiles.cs` — атомарний запис і спільні налаштування JSON
- `Cli/appsettings.json` (у git, без секретів), `appsettings.Development.json`, `appsettings.Production.json`
- `samples/` — файли-зразки для імпорту (раніше `data/`); `data/` тепер створюється під час роботи і є в `.gitignore`

### Порядок джерел конфігурації
Кожне наступне джерело перекриває попереднє:

```
appsettings.json → appsettings.{Environment}.json → user-secrets → змінні CROSSAPP_* → аргументи командного рядка
```

Середовище обирається змінною `CROSSAPP_ENVIRONMENT` (за замовчуванням `Development`).

### Профілі
| Профіль | Рівень логу | Де лежать дані |
|---|---|---|
| Development | Debug (додатково друкуються джерела конфігурації) | `bin/Debug/net10.0/data/` поруч із програмою |
| Production | Warning | `%LOCALAPPDATA%\CrossApp\data\` (Windows), `~/.local/share/CrossApp/data/` (Linux) |

### Таблиця ключів
| Ключ | За замовчуванням | Змінна середовища | Опис |
|---|---|---|---|
| EnvironmentName | Production / Development | — (профіль: `CROSSAPP_ENVIRONMENT`) | назва профілю |
| Logging:MinimumLevel | Warning (Development: Debug) | `CROSSAPP_Logging__MinimumLevel` | детальність виводу |
| Storage:BaseFolder | App (Production: LocalAppData) | `CROSSAPP_Storage__BaseFolder` | базова папка для відносного DataPath |
| Storage:DataPath | data | `CROSSAPP_Storage__DataPath` або `CROSSAPP_DATA_PATH` | каталог даних |
| Storage:FileName | books.json | `CROSSAPP_Storage__FileName` | сховище примірників (`--file`) |
| Storage:ExportFileName | catalog-export.json | `CROSSAPP_Storage__ExportFileName` | файл експорту каталогу |
| Library:LoanDays | 14 | `CROSSAPP_Library__LoanDays` | строк видачі в днях |
| Integration:ApiKey | — (лише user-secrets / env) | `CROSSAPP_Integration__ApiKey` | секрет; друкується лише факт наявності |

### Експорт та імпорт каталогу

```
dotnet run --project src/Cli -- export
dotnet run --project src/Cli -- import-editions
```

### Перевизначення змінними середовища
cmd:

```
set CROSSAPP_DATA_PATH=C:\Temp\crossapp-data
dotnet run --project src/Cli -- export
set CROSSAPP_DATA_PATH=
```

PowerShell:

```
$env:CROSSAPP_DATA_PATH = "C:\Temp\crossapp-data"
dotnet run --project src/Cli -- export
Remove-Item Env:CROSSAPP_DATA_PATH
```

bash:

```
CROSSAPP_DATA_PATH=/tmp/crossapp-data dotnet run --project src/Cli -- export
```

Інші ключі так само, з подвійним підкресленням замість двокрапки: `CROSSAPP_Library__LoanDays=30`, `CROSSAPP_ENVIRONMENT=Production`.

### Секрет (user-secrets)

```
dotnet user-secrets init --project src/Cli
dotnet user-secrets set "Integration:ApiKey" "<ваш-ключ>" --project src/Cli
dotnet user-secrets list --project src/Cli
```

У репозиторії зберігається лише `UserSecretsId` у `Cli.csproj`. Саме значення лежить поза репозиторієм:
- Windows: `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`
- Linux / macOS: `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

