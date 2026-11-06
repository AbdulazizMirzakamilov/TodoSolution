# TodoSolution

Учебная .NET-библиотека управления списком задач (`Todo.Core`) с автоматизированным CI/CD конвейером через GitHub Actions и публикацией NuGet-пакета в GitHub Packages.

Разработано в рамках практической работы: **«Проектирование и публикация .NET-библиотеки с CI/CD»**.

---

## Возможности

- **Модель задачи (`TodoItem`)**:
  - Уникальный идентификатор `Guid Id`.
  - Заголовок задачи `Title` с валидацией входных данных.
  - Статус выполнения `IsDone`.
  - Методы управления: `MarkDone()`, `MarkUndone()`, `Rename(newTitle)`.
  - Поддержка сериализации в JSON с сохранением состояния приватных свойств.
- **Список задач (`TodoList`)**:
  - Добавление задачи `Add(title)`.
  - Удаление задачи по ID `Remove(id)`.
  - Поиск по подстроке `Find(substring)` (без учёта регистра).
  - Получение общего количества задач `Count`.
  - Сохранение в JSON-файл: `Save(path)` с форматированным выводом.
  - Загрузка из JSON-файла: `Load(path)`.
- **Unit-тестирование**:
  - Полное покрытие логики тестами на базе `xUnit`.
- **CI/CD**:
  - Автоматическая сборка, тестирование и упаковка (`dotnet pack`) в workflow `ci.yml`.
  - Автоматическая публикация NuGet-пакета в GitHub Packages в workflow `publish-package.yml`.

---

## Структура решения

```text
TodoSolution/
├── .github/
│   └── workflows/
│       ├── ci.yml                 # Непрерывная интеграция (сборка, тесты, упаковка)
│       └── publish-package.yml    # Публикация пакета в GitHub Packages
├── Todo.Core/
│   ├── Todo.Core.csproj           # Проект библиотеки (.NET 10)
│   ├── TodoItem.cs                # Сущность задачи
│   └── TodoList.cs                # Менеджер списка задач
├── Todo.Core.Tests/
│   ├── Todo.Core.Tests.csproj     # Проект xUnit тестов
│   └── TodoListTests.cs           # Набор модульных тестов
├── TodoSolution.slnx              # Файл решения
└── README.md
```

---

## Локальная сборка и запуск тестов

Требования: [.NET 10 SDK](https://dotnet.microsoft.com/).

```bash
# Восстановление зависимостей
dotnet restore

# Сборка проекта
dotnet build -c Release

# Запуск тестов
dotnet test -c Release --verbosity normal

# Создание NuGet-пакета
dotnet pack Todo.Core/Todo.Core.csproj -c Release -o ./artifacts
```

---

## Подключение пакета через NuGet (GitHub Packages)

### 1. Добавление источника пакетов в Visual Studio
1. Откройте **Tools** (Сервис) → **Options** (Параметры) → **NuGet Package Manager** → **Package Sources** (Источники пакетов).
2. Нажмите **+** (Добавить новый источник):
   - **Name**: `GitHubPackages`
   - **Source**: `https://nuget.pkg.github.com/AbdulazizMirzakamilov/index.json`
3. Нажмите **OK**.

### 2. Авторизация в GitHub Packages
При установке пакета через диспетчер пакетов Visual Studio или CLI потребуется авторизация:
- **Username**: ваш логин GitHub (`AbdulazizMirzakamilov`)
- **Password**: GitHub Personal Access Token (PAT) с правами `read:packages`.

Для авторизации через консоль:
```bash
dotnet nuget add source "https://nuget.pkg.github.com/AbdulazizMirzakamilov/index.json" \
  --name "GitHubPackages" \
  --username "AbdulazizMirzakamilov" \
  --password "<YOUR_GITHUB_PAT>" \
  --store-password-in-clear-text
```

### 3. Установка пакета в проект

Через .NET CLI:
```bash
dotnet add package Todo.Core
```

Или через `PackageReference` в `.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="Todo.Core" Version="0.2.0" />
</ItemGroup>
```

---

## Пример использования библиотеки в коде C#

```csharp
using System;
using Todo.Core;

// Создание списка задач
var todoList = new TodoList();

// Добавление задач
var task1 = todoList.Add("Купить молоко");
var task2 = todoList.Add("Сдать практическую работу");

// Отметка о выполнении
task2.MarkDone();

// Поиск задач
var results = todoList.Find("молоко");
foreach (var item in results)
{
    Console.WriteLine($"[{(item.IsDone ? "X" : " ")}] {item.Title}");
}

// Сохранение списка в JSON
todoList.Save("todos.json");

// Загрузка списка из JSON в новый экземпляр
var loadedList = new TodoList();
loadedList.Load("todos.json");
Console.WriteLine($"Загружено задач: {loadedList.Count}");
```
