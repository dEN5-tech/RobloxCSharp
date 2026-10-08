# 📚 RobloxCSharp: Полная Документация и Справочник API (WIKI)

---

## 🌟 1. Обзор Архитектуры (Architecture Overview)

**RobloxCSharp** — это современный пайплайн разработки игр для платформы **Roblox** на языке **C# (.NET 8)** с автоматической компиляцией в оптимизированный **Roblox Luau**, интеграцией **Rojo 7.7** для синхронизации в реальном времени, строгой типизацией Roblox Engine API и системой **SourceMap** для отладки ошибок в runtime.

```
┌────────────────────────┐
│   C# .NET 8 Codebase   │  (Visual Studio, Rider, VSCode)
│   (csharp/, Entities/) │
└───────────┬────────────┘
            │  dotnet build / compile.bat
            ▼
┌────────────────────────┐       ┌────────────────────────┐
│  CSharp.lua Transpiler │ ◄──── │  RobloxAPI (Bindings)  │ ◄── api-dump.json
└───────────┬────────────┘       └────────────────────────┘
            │
            ├──────────────────────────────────────────┐
            ▼                                          ▼
┌────────────────────────┐                  ┌──────────────────────┐
│ Generated Luau Modules │                  │ SourceMapData.lua    │
│    (src/Generated/)    │                  │  (Line Remapper)     │
└───────────┬────────────┘                  └──────────┬───────────┘
            │                                          │
            ▼                                          ▼
┌──────────────────────────────────────────────────────────────────┐
│                           Rojo 7.7                               │
│              (Live-Sync Server <-> Studio Plugin)                │
└──────────────────────────────────┬───────────────────────────────┘
                                   │
                                   ▼
┌──────────────────────────────────────────────────────────────────┐
│                   Roblox Studio / Game Server                    │
│      ├── ReplicatedStorage: CoreSystem + RobloxCSharp            │
│      ├── ServerScriptService: ServerLoader (C# Server)          │
│      └── StarterPlayerScripts: ClientLoader (C# Client)          │
└──────────────────────────────────────────────────────────────────┘
```

---

## ⚙️ 2. RobloxAPI Generator (Генератор API Биндингов)

### Назначение
Проект `RobloxAPI.Generator` парсит официальный дамп API движка Roblox (`api-dump.json`) и автоматически генерирует строго типизированную сборку `RobloxAPI.cs`, содержащую:
- **920+ классов** (`Instance`, `Part`, `Model`, `Player`, `Humanoid`, `Workspace`, `Lighting` и др.)
- **636+ перечислений (Enums)** (`Font`, `Material`, `HumanoidRigType`, `EasingStyle` и др.)
- **Свойства, методы и события (Events / RBXScriptSignal)**
- **Встроенные структуры типов данных** (`Vector3`, `CFrame`, `Color3`, `UDim`, `UDim2`, `NumberRange`)

### Запуск генератора
```powershell
# Вариант 1: Через пакетный файл (автоматически скачает или обновит)
.\generate_types.bat

# Вариант 2: С флагом принудительной загрузки свежего api-dump.json из сети
.\RobloxAPI\generate.ps1 -Fetch

# Вариант 3: Прямой запуск через dotnet CLI
dotnet run --project RobloxAPI/Generator/RobloxAPI.Generator.csproj -- --fetch RobloxAPI/api-dump.json RobloxAPI/RobloxAPI.cs
```

### Преобразование типов в генераторе

| Roblox DataType / Primitive | C# Type in `RobloxAPI` |
|---|---|
| `string` | `string` |
| `bool` | `bool` |
| `int`, `int64` | `int` |
| `float`, `double` | `double` |
| `Vector3` | `Roblox.Vector3` |
| `CFrame` | `Roblox.CFrame` |
| `Color3` | `Roblox.Color3` |
| `UDim` / `UDim2` | `Roblox.UDim` / `Roblox.UDim2` |
| `NumberRange` | `Roblox.NumberRange` |
| `Instance` и наследники | `Roblox.Instance`, `Roblox.Part`, `Roblox.Player`, ... |
| `RBXScriptSignal` | `Roblox.RBXScriptSignal` (`Connect(Action<object>)`) |
| `Enum.X` | `Roblox.XEnum` / `Roblox.X` |

### Мета-атрибуты CSharpLua
Для трансляции C# вызовов в нативные синтаксические конструкции Luau используются мета-атрибуты:
- `[Template("...")]` — заменяет вызов метода/свойства кастомным Luau выражением (например, `Instance.New<T>()` -> `Instance.new("{0}")`).
- `[Get("...")]` / `[Set("...")]` — кастомизирует доступ к свойствам.
- `[External]` / `[Ignore]` — указывает компилятору исключить класс из сборки, так как он существует в рантайме Roblox.

---

## 🧩 3. CoreSystem Luau Runtime (Среда Исполнения)

Файл `src/CoreSystem/init.lua` предоставляет платформо-независимую реализацию ключевых подсистем .NET для Luau:

### 1. Объектно-Ориентированное Программирование (OOP)
- **Пространства имен**: `System.namespace("RobloxCSharp.Entities", function(namespace) ... end)`
- **Классы**: `namespace.class("Hero", function(this) ... end)`
- **Наследование**: Поддержка цепочки прототипов `__index` через `classMembers.base`.
- **Конструкторы**: `__ctor__` с автоматической передачей аргументов и поддержкой вызова базового конструктора через `System.base(this).__ctor__(this, ...)`.
- **Статические конструкторы**: Блок `static` для инициализации статических полей.

### 2. Коллекции (.NET Collections)
- `System.List<T>`:
  - `Add(item)` — добавление элемента в список.
  - `Remove(item)` — удаление элемента.
  - `Count()` — получение количества элементов.
  - `Get(index)` — доступ по 0-based индексу.
  - `items` — внутренняя таблица элементов для итераций.
- `System.Dictionary<TKey, TValue>`:
  - `Add(key, value)` — сохранение пары ключ-значение.
  - `Remove(key)` — удаление ключа.
  - `ContainsKey(key)` — проверка наличия ключа.
  - `Get(key)` — получение значения.
- `System.Array`: Поддержка фиксированных и динамических массивов.
- `System.each(collection)`: Универсальный итератор для циклов `foreach`.

### 3. Базовые Сервисы и Синтаксический Сахар
- `Game.GetService("ServiceName")` и свойства быстрого доступа: `Game.Players`, `Game.Workspace`, `Game.Lighting`, `Game.ReplicatedStorage`, `Game.ServerScriptService`, `Game.StarterPlayer`.
- `Console.WriteLine(...)` / `Console.Write(...)` — вывод в Output окно Roblox Studio.
- `System.cast(type, obj)`, `System.as(obj, type)`, `System.is(obj, type)` — безопасные приведения типов.

---

## 🔍 4. SourceMap и Перехват Ошибок (Error Remapper)

При сборке проекта скрипт `build.ps1` генерирует файл `src/CoreSystem/SourceMapData.lua`, содержащий точную карту соответствия строк Luau исходным файлам C#.

### Как работает перехватчик:
1. `ServerLoader.server.lua` инициализирует глобальный перехватчик через `ErrorHandler.Install()`.
2. Подключается слушатель к `game:GetService("ScriptContext").Error`.
3. При возникновении исключения в runtime (например, обращение к `nil` или ошибка в логике) стек трейс Luau автоматически перехватывается.
4. `ErrorHandler.lua` анализирует имена модулей и номера строк, переводя их в пути к C# файлам:

```
================================================================================
🔥 [ROBLOX C# RUNTIME EXCEPTION CAUGHT BY SOURCEMAP HANDLER]
📌 Error Message: ServerScriptService.RobloxCSharp.SuperJumperHero:51: attempt to index nil with 'AssemblyLinearVelocity'
📍 Exact C# Origin:  --> [C# SOURCE] csharp/Entities/SuperJumperHero.cs:line 51 (Lua line 51)
📜 Remapped C# Stack Trace:
  --> [C# SOURCE] csharp/Entities/SuperJumperHero.cs:line 51 (Lua line 51)
  --> [C# SOURCE] csharp/Entities/SuperJumperHero.cs:line 39 (Lua line 39)
  --> [C# SOURCE] csharp/Server/MainServer.cs:line 79 (Lua line 79)
================================================================================
```

---

## 🎮 5. Примеры написания Игрового Кода на C#

### 1. Создание базового класса Героя (`csharp/Entities/Hero.cs`)

```csharp
namespace RobloxCSharp.Entities
{
    using System;
    using Roblox;

    public class Hero
    {
        public Player Player;
        public Model Character;
        public Humanoid Humanoid;
        public string Name;
        public double Health = 100;
        public double MaxHealth = 100;

        public Hero(Player player, Model character, Humanoid humanoid)
        {
            this.Player = player;
            this.Character = character;
            this.Humanoid = humanoid;
            this.Name = player != null ? player.Name : "Hero";
        }

        public virtual void Initialize()
        {
            if (this.Humanoid != null)
            {
                this.Humanoid.MaxHealth = this.MaxHealth;
                this.Humanoid.Health = this.Health;
                this.Humanoid.WalkSpeed = 16;
            }

            CreateOverheadGui();
            Console.WriteLine("[Герой] Инициализирован базовый герой: " + this.Name);
        }

        protected void CreateOverheadGui()
        {
            var head = (BasePart)this.Character.FindFirstChild("Head");
            if (head == null) return;

            var billboard = Instance.New<BillboardGui>("BillboardGui");
            billboard.Name = "HeroOverhead";
            billboard.Adornee = head;
            billboard.Size = UDim2.New(0, 150, 0, 40);
            billboard.StudsOffset = Vector3.New(0, 2.5, 0);
            billboard.AlwaysOnTop = true;
            billboard.Parent = head;

            var nameLabel = Instance.New<TextLabel>("TextLabel");
            nameLabel.Size = UDim2.New(1, 0, 1, 0);
            nameLabel.BackgroundTransparency = 1;
            nameLabel.Text = this.Name + " [Герой]";
            nameLabel.TextColor3 = Color3.FromRGB(255, 255, 255);
            nameLabel.TextScaled = true;
            nameLabel.Font = Font.GothamBold;
            nameLabel.Parent = billboard;
        }

        public virtual void UseAbility()
        {
            Console.WriteLine("[Герой] Базовая способность активирована!");
        }
    }
}
```

---

### 2. Создание специализированного класса (`csharp/Entities/SuperJumperHero.cs`)

```csharp
namespace RobloxCSharp.Entities
{
    using System;
    using Roblox;

    public class SuperJumperHero : Hero
    {
        public double SuperJumpForce = 120;

        public SuperJumperHero(Player player, Model character, Humanoid humanoid)
            : base(player, character, humanoid)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            if (this.Humanoid != null)
            {
                this.Humanoid.UseJumpPower = true;
                this.Humanoid.JumpPower = this.SuperJumpForce;
            }

            // Добавляем золотую ауру и супер-прыжок
            CreateJumpAura();
            SuperJump();
        }

        public void SuperJump()
        {
            var rootPart = (BasePart)this.Character.FindFirstChild("HumanoidRootPart");
            if (rootPart == null || this.Humanoid == null) return;

            Console.WriteLine("🚀 [Способность] Супер Прыжок активирован для " + this.Name);
            rootPart.AssemblyLinearVelocity = Vector3.New(0, this.SuperJumpForce, 0);
            this.Humanoid.Jump = true;
        }

        private void CreateJumpAura()
        {
            var aura = Instance.New<Highlight>("Highlight");
            aura.Name = "JumpAura";
            aura.FillColor = Color3.FromRGB(255, 215, 0);
            aura.FillTransparency = 0.5;
            aura.Parent = this.Character;
        }
    }
}
```

---

### 3. Главный Сервер (`csharp/Server/MainServer.cs`)

```csharp
namespace RobloxCSharp.Server
{
    using System;
    using System.Collections.Generic;
    using Roblox;
    using RobloxCSharp.Entities;

    public class MainServer
    {
        private static List<Hero> activeHeroes = new List<Hero>();

        public static void Init()
        {
            Console.WriteLine("[СЕРВЕР] Инициализация RobloxCSharp...");

            Game.Players.PlayerAdded.Connect((playerInstance) =>
            {
                var player = (Player)playerInstance;
                player.CharacterAdded.Connect((charInstance) =>
                {
                    var character = (Model)charInstance;
                    var humanoid = (Humanoid)character.FindFirstChild("Humanoid");
                    if (humanoid != null)
                    {
                        var hero = new SuperJumperHero(player, character, humanoid);
                        hero.Initialize();
                        activeHeroes.Add(hero);
                    }
                });
            });
        }
    }
}
```

---

## 🗂️ 6. Структура Каталогов (Project Structure)

```
C:\roblox-csharp-template/
├── .gitignore                      # Исключения Git (.NET, IDE, Roblox artifacts)
├── RobloxCSharp.sln               # Главное решение Visual Studio / Rider
├── RobloxCSharp.slnx              # Легковесное решение .slnx
├── default.project.json           # Конфигурация дерева проекта для Rojo 7.7
├── compile.bat                    # Быстрая сборка C# -> Luau (Release)
├── generate_types.bat             # Генерация/обновление RobloxAPI.cs
├── test.rbxlx                     # Готовый плейс Roblox Studio
│
├── RobloxAPI/                     # Сборка типизации движка Roblox
│   ├── RobloxAPI.csproj           # Проект библиотеки биндингов
│   ├── Attributes.cs              # CSharpLua мета-атрибуты
│   ├── api-dump.json              # Официальный дамп API движка Roblox
│   ├── RobloxAPI.cs               # Сгенерированные 26,000+ строк классов движка
│   └── Generator/                 # CLI-генератор C# биндингов
│       ├── RobloxAPI.Generator.csproj
│       ├── Program.cs
│       ├── Models.cs
│       └── CodeGenerator.cs
│
├── csharp/                        # Игровой код на C# (.NET 8)
│   ├── RobloxCSharpTemplate.csproj
│   ├── build.ps1                  # Скрипт транспиляции и генерации SourceMap
│   ├── Roblox.xml                 # XML-метаданные транслятора
│   ├── Server/                    # Серверная игровая логика
│   │   └── MainServer.cs
│   ├── Client/                    # Клиентская игровая логика
│   │   └── MainClient.cs
│   ├── Entities/                  # ООП сущности и игровые механики
│   │   ├── Hero.cs
│   │   └── SuperJumperHero.cs
│   └── Shared/                    # Общие структуры и утилиты
│       └── Utils.cs
│
├── src/                           # Luau исходники для синхронизации через Rojo
│   ├── ServerLoader.server.lua    # Точка входа ServerScriptService
│   ├── ClientLoader.client.lua    # Точка входа StarterPlayerScripts
│   ├── CoreSystem/                # Среда исполнения C# в Luau
│   │   ├── init.lua               # OOP runtime, списки, словари, Game API
│   │   ├── ErrorHandler.lua       # Перехватчик и SourceMap трассировщик
│   │   └── SourceMapData.lua      # Автогенерированная карта строк
│   └── Generated/                 # Сгенерированные Luau файлы C# кода
│       ├── manifest.lua
│       ├── Hero.lua
│       ├── SuperJumperHero.lua
│       ├── MainServer.lua
│       └── ...
│
└── docs/
    └── WIKI_API.md                # Данная вики-документация
```

---

## 🛠️ 7. Справочник Команд (Quick Commands)

| Задача | Команда |
|---|---|
| **Собрать C# и обновить Luau** | `.\compile.bat` или `dotnet build RobloxCSharp.sln` |
| **Запустить Rojo Live-Sync** | `rojo serve` |
| **Обновить типы Roblox API** | `.\generate_types.bat` |
| **Собрать только C# проект** | `dotnet build csharp/RobloxCSharpTemplate.csproj` |
