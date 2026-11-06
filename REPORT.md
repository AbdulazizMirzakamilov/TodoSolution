# Отчет по практической работе №3

## 1. Номер и название практического занятия
**Практическое занятие №3:** «Проектирование и публикация .NET-библиотеки с CI/CD».

## 2. Цели практического занятия
- Освоение практического опыта разработки программного проекта с автоматизированным CI/CD конвейером.
- Изучение принципов организации структуры многопроектного решения в экосистеме .NET.
- Применение системы контроля версий Git и платформы GitHub (ветвление, коммиты, Pull Request).
- Разработка модульных тестов с использованием фреймворка xUnit.
- Настройка процессов непрерывной интеграции (CI) и непрерывного развертывания (CD) с помощью GitHub Actions.
- Освоение механизма пакетирования NuGet и публикации библиотек в реестр GitHub Packages.

---

## 3. Ход выполнения работы

### 3.1. Создание репозитория и стратегия ветвления
На GitHub создан публичный репозиторий:  
**URL:** [https://github.com/AbdulazizMirzakamilov/TodoSolution](https://github.com/AbdulazizMirzakamilov/TodoSolution)

Разработка велась по методологии GitHub Flow с созданием отдельных тематических веток и слиянием через Pull Request:
1. `Initial commit` — создание решения `TodoSolution.slnx` и файла исключений `.gitignore`.
2. **PR #1 (`feat/todo-core`):** создание библиотеки классов `Todo.Core`, реализация сущности `TodoItem` и менеджера задач `TodoList`.
3. **PR #2 (`feat/todo-core-tests`):** создание тестового проекта `Todo.Core.Tests` на базе фреймворка xUnit, написание базовых тестов.
4. **PR #3 (`feat/ci-cd`):** конфигурирование метаданных NuGet-пакета версии 0.1.0, создание workflows `ci.yml` и `publish-package.yml`.
5. **PR #4 (`feat/json-persistence`):** выполнение самостоятельного задания по реализации сохранения/загрузки задач в формате JSON, добавление соответствующих тестов, поднятие версии пакета до 0.2.0, оформление документации `README.md`.

---

### 3.2. Структура проекта

```text
TodoSolution/
├── .github/
│   └── workflows/
│       ├── ci.yml                 # Сборка, тестирование и упаковка в артефакт
│       └── publish-package.yml    # Публикация NuGet-пакета в GitHub Packages
├── Todo.Core/
│   ├── Todo.Core.csproj           # Проект библиотеки (.NET 10, настройки упаковки NuGet)
│   ├── TodoItem.cs                # Модель задачи с валидацией и JSON-аннотациями
│   └── TodoList.cs                # Коллекция задач с операциями CRUD и Save/Load
├── Todo.Core.Tests/
│   ├── Todo.Core.Tests.csproj     # Проект тестов (xUnit, Microsoft.NET.Test.Sdk)
│   └── TodoListTests.cs           # Модульные тесты функциональности списка и JSON
├── TodoSolution.slnx              # Файл решения Visual Studio / .NET CLI
├── .gitignore                     # Файл игнорирования временных файлов сборки
├── README.md                      # Инструкция по сборке, тестированию и установке пакета
└── REPORT.md                      # Текстовая версия отчета
```

---

### 3.3. Код библиотеки и тестов

#### Файл `Todo.Core/TodoItem.cs`
```csharp
using System;
using System.Text.Json.Serialization;

namespace Todo.Core
{
    public class TodoItem
    {
        [JsonInclude]
        public Guid Id { get; private set; } = Guid.NewGuid();

        [JsonInclude]
        public string Title { get; private set; }

        [JsonInclude]
        public bool IsDone { get; private set; }

        public TodoItem(string title)
        {
            Title = title?.Trim() ?? throw new ArgumentNullException(nameof(title));
        }

        [JsonConstructor]
        public TodoItem(Guid id, string title, bool isDone)
        {
            Id = id;
            Title = title?.Trim() ?? throw new ArgumentNullException(nameof(title));
            IsDone = isDone;
        }

        public void MarkDone() => IsDone = true;
        public void MarkUndone() => IsDone = false;

        public void Rename(string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException("Title required", nameof(newTitle));
            Title = newTitle.Trim();
        }
    }
}
```

#### Файл `Todo.Core/TodoList.cs`
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Todo.Core
{
    public class TodoList
    {
        private readonly List<TodoItem> _items = new();
        public IReadOnlyList<TodoItem> Items => _items.AsReadOnly();

        public TodoItem Add(string title)
        {
            var item = new TodoItem(title);
            _items.Add(item);
            return item;
        }

        public bool Remove(Guid id) => _items.RemoveAll(i => i.Id == id) > 0;

        public IEnumerable<TodoItem> Find(string substring) =>
            _items.Where(i => i.Title.Contains(substring ?? string.Empty, StringComparison.OrdinalIgnoreCase));

        public int Count => _items.Count;

        public void Save(string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_items, options);
            File.WriteAllText(path, json);
        }

        public void Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Файл не найден", path);

            var json = File.ReadAllText(path);
            var items = JsonSerializer.Deserialize<List<TodoItem>>(json);
            _items.Clear();
            if (items != null)
            {
                _items.AddRange(items);
            }
        }
    }
}
```

#### Файл `Todo.Core.Tests/TodoListTests.cs`
```csharp
using System;
using System.IO;
using System.Linq;
using Xunit;
using Todo.Core;

namespace Todo.Core.Tests
{
    public class TodoListTests
    {
        [Fact]
        public void Add_IncreasesCount()
        {
            var list = new TodoList();
            list.Add(" task ");
            Assert.Equal(1, list.Count);
            Assert.Equal("task", list.Items.First().Title);
        }

        [Fact]
        public void Remove_ById_Works()
        {
            var list = new TodoList();
            var item = list.Add("a");
            Assert.True(list.Remove(item.Id));
            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void Find_ReturnsMatches()
        {
            var list = new TodoList();
            list.Add("Buy milk");
            list.Add("Read book");
            var found = list.Find("buy").ToList();
            Assert.Single(found);
            Assert.Equal("Buy milk", found[0].Title);
        }

        [Fact]
        public void SaveAndLoad_PreservesItemsCorrectly()
        {
            var list = new TodoList();
            var item1 = list.Add("Первая задача");
            var item2 = list.Add("Вторая задача");
            item2.MarkDone();

            var tempPath = Path.Combine(Path.GetTempPath(), $"todo_test_{Guid.NewGuid()}.json");
            try
            {
                list.Save(tempPath);

                var loadedList = new TodoList();
                loadedList.Load(tempPath);

                Assert.Equal(2, loadedList.Count);

                var loaded1 = loadedList.Items.First(i => i.Id == item1.Id);
                Assert.Equal("Первая задача", loaded1.Title);
                Assert.False(loaded1.IsDone);

                var loaded2 = loadedList.Items.First(i => i.Id == item2.Id);
                Assert.Equal("Вторая задача", loaded2.Title);
                Assert.True(loaded2.IsDone);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        [Fact]
        public void Load_NonExistentFile_ThrowsFileNotFoundException()
        {
            var list = new TodoList();
            Assert.Throws<FileNotFoundException>(() => list.Load("non_existent_file.json"));
        }
    }
}
```

---

### 3.4. Конфигурация GitHub Actions Workflows

#### 1. `.github/workflows/ci.yml` (Непрерывная интеграция)
```yaml
name: CI / Build & Test

on:
  push:
    branches: [ main, master ]
  pull_request:
    branches: [ main, master ]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj', '**/packages.lock.json') }}
          restore-keys: |
            ${{ runner.os }}-nuget-

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore -c Release

      - name: Test
        run: dotnet test --no-build -c Release --verbosity normal

      - name: Pack
        run: dotnet pack Todo.Core/Todo.Core.csproj -c Release -o ./artifacts
```

#### 2. `.github/workflows/publish-package.yml` (Публикация пакета)
```yaml
name: Publish package to GitHub Packages

on:
  workflow_run:
    workflows: ["CI / Build & Test"]
    types:
      - completed

jobs:
  publish:
    if: ${{ github.event.workflow_run.conclusion == 'success' }}
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore

      - name: Build (Release)
        run: dotnet build -c Release --no-restore

      - name: Pack
        run: dotnet pack Todo.Core/Todo.Core.csproj -c Release -o ./artifacts

      - name: List artifacts
        run: ls -la ./artifacts || true

      - name: Push package to GitHub Packages (NuGet)
        run: dotnet nuget push ./artifacts/*.nupkg --source "https://nuget.pkg.github.com/${{ github.repository_owner }}/index.json" --api-key ${{ secrets.GITHUB_TOKEN }} --skip-duplicate
```

---

### 3.5. Ссылка на опубликованный пакет в GitHub Packages
- **URL реестра пакетов:** `https://nuget.pkg.github.com/AbdulazizMirzakamilov/index.json`
- **Имя пакета:** `Todo.Core`
- **Версия пакета:** `0.2.0`
- **Страница репозитория:** [https://github.com/AbdulazizMirzakamilov/TodoSolution](https://github.com/AbdulazizMirzakamilov/TodoSolution)

---

## 4. Краткий вывод по проделанной работе
В ходе выполнения практической работы были достигнуты все поставленные цели:
1. Создана архитектурно правильная структура .NET-решения, разделенная на библиотеку (`Todo.Core`) и модульные тесты (`Todo.Core.Tests`).
2. Применены современные стандарты разработки на C# (.NET 10, nullable-аннотации, сериализация через `System.Text.Json`).
3. Реализовано самостоятельное задание по сохранению и загрузке задач из JSON с поддержкой сохранения приватных свойств.
4. Написано 5 модульных тестов на xUnit, успешно подтверждающих корректность работы бизнес-логики.
5. Настроен полный цикл автоматизации в GitHub Actions: автоматическое восстановление зависимостей с кешированием, сборка в Release-режиме, прогон тестов, создание `.nupkg` и публикация в GitHub Packages.
