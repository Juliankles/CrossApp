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