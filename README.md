# 🚀 RobloxCSharp: Modern C# .NET 8 for Roblox Game Development

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Rojo 7.7](https://img.shields.io/badge/Rojo-7.7-E03C31?logo=roblox&logoColor=white)](https://rojo.space/)
[![Luau](https://img.shields.io/badge/Language-Luau-00A2FF?logo=lua&logoColor=white)](https://luau.org/)
[![Status](https://img.shields.io/badge/Status-Production%20Ready-brightgreen.svg)]()

> **RobloxCSharp** — мощный фреймворк и шаблон разработки игр для **Roblox** на современном **C# (.NET 8)** с транспиляцией в оптимизированный **Roblox Luau**, строгой типизацией Roblox Engine API, поддержкой ООП (наследование, конструкторы, полиморфизм), встроенными коллекциями .NET, системой **SourceMap** для перевода стека ошибок в строки C#, и мгновенной синхронизацией через **Rojo 7.7**.

---

## 📑 Содержание / Table of Contents

- [Особенности (Key Features)](#-особенности-key-features)
- [Архитектура (Architecture)](#-архитектура-architecture)
- [Быстрый старт (Quick Start)](#-быстрый-старт-quick-start)
- [Структура проекта (Project Structure)](#-структура-проекта-project-structure)
- [Как писать код на C# (How to Write Gameplay Code)](#-как-писать-код-на-c)
- [RobloxAPI Generator (Генератор API)](#-robloxapi-generator)
- [SourceMap: Отладка Runtime-Ошибок](#-sourcemap-отладка-runtime-ошибок)
- [Справочник API и Документация (WIKI API)](#-справочник-api-и-документация)
- [Лицензия (License)](#-лицензия)

---

## ✨ Особенности (Key Features)

- **Полный C# .NET 8**: Пишите привычный ООП-код в Visual Studio, JetBrains Rider или VS Code со всеми преимуществами сильного анализатора, автодополнения (IntelliSense) и рефакторинга.
- **Строгая типизация Roblox Engine**: Автоматически сгенерированные биндинги для **920+ классов** (`Instance`, `Player`, `Part`, `Humanoid`, `Model` и др.) и **636+ перечислений** (`Enums`).
- **Синхронизация Rojo 7.7 в реальном времени**: Автоматический хук в `.csproj` пересобирает Luau модули при каждой компиляции C# (`Ctrl+Shift+B` или `dotnet build`), и Rojo мгновенно отправляет изменения в открытый плейс Roblox Studio.
- **Полноценный ООП Runtime (`CoreSystem`)**:
  - Наследование классов (`public class SuperJumperHero : Hero`)
  - Вызов конструкторов базового класса (`: base(...)`)
  - Полиморфизм и переопределение методов (`virtual` / `override`)
  - Коллекции: `List<T>`, `Dictionary<TKey, TValue>`, `Array`, `System.each`
- **SourceMap Line Remapper**: Ошибки в Roblox Studio выводятся с точным указанием пути к C# файлу и номеру строки в исходном коде (`[C# SOURCE] csharp/Entities/Hero.cs:line 45`).

---

## 🏗 Архитектура (Architecture)

```
┌──────────────────────────────────────────────────────────┐
│                      IDE / Editor                        │
│            Visual Studio 2022 / Rider / VS Code          │
│            C# .NET 8 (csharp/Server, Client, Entities)   │
└────────────────────────────┬─────────────────────────────┘
                             │  dotnet build / compile.bat
                             ▼
┌──────────────────────────────────────────────────────────┐
│              CSharp.lua + RobloxAPI Generator            │
│       • Transpiles C# AST into Roblox Luau Modules       │
│       • Generates SourceMap line remappings              │
│       • Emits ModuleScripts into src/Generated/          │
└────────────────────────────┬─────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────┐
│                      Rojo 7.7 Server                     │
│         Syncs src/ -> Roblox Studio DataModel            │
└────────────────────────────┬─────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────┐
│                     Roblox Studio                        │
│  ├── ReplicatedStorage                                   │
│  │   ├── CoreSystem (OOP, Collections, ErrorHandler)     │
│  │   └── RobloxCSharp (Transpiled Modules & manifest)    │
│  ├── ServerScriptService/ServerLoader (Server Entry)    │
│  └── StarterPlayerScripts/ClientLoader (Client Entry)    │
└──────────────────────────────────────────────────────────┘
```

---

## ⚡ Быстрый старт (Quick Start)

### Требования
1. [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) или новее.
2. [Rojo 7.7+](https://rojo.space/) CLI и плагин для Roblox Studio.
3. [Roblox Studio](https://www.roblox.com/create).

### Шаги запуска

1. **Клонируйте репозиторий**:
   ```bash
   git clone https://github.com/dEN5-tech/RobloxCSharp.git
   cd RobloxCSharp
   ```

2. **Скомпилируйте проект**:
   Запустите файл `compile.bat` или выполните команду в терминале:
   ```powershell
   dotnet build RobloxCSharp.sln
   ```

3. **Запустите Rojo**:
   ```powershell
   rojo serve
   ```

4. **Подключитесь в Roblox Studio**:
   - Откройте файл `test.rbxlx` или новый плейс в Roblox Studio.
   - В панели плагинов нажмите **Rojo** -> **Connect** (порт по умолчанию `34872`).
   - Нажмите **Play / Run** в Roblox Studio!

---

## 📂 Структура проекта (Project Structure)

```
├── .gitignore                      # Исключения Git (.NET, IDE, Roblox artifacts)
├── RobloxCSharp.sln               # Главное решение Visual Studio
├── RobloxCSharp.slnx              # Легковесное .slnx решение
├── default.project.json           # Дерево синхронизации Rojo 7.7
├── compile.bat                    # Скрипт сборки C# и обновления Luau
├── generate_types.bat             # Скрипт генерации типов Roblox API
├── test.rbxlx                     # Готовый плейс Roblox Studio
│
├── RobloxAPI/                     # Сборка типизации движка Roblox
│   ├── RobloxAPI.csproj           # Проект библиотеки биндингов
│   ├── Attributes.cs              # Мета-атрибуты компилятора CSharpLua
│   ├── api-dump.json              # Официальный дамп API движка Roblox
│   ├── RobloxAPI.cs               # 26,000+ строк классов движка Roblox
│   └── Generator/                 # CLI-генератор C# биндингов из api-dump.json
│
├── csharp/                        # Игровой код на C# (.NET 8)
│   ├── RobloxCSharpTemplate.csproj
│   ├── build.ps1                  # Скрипт транспиляции и сборки SourceMap
│   ├── Roblox.xml                 # XML-метаданные транслятора
│   ├── Server/                    # Серверный код (MainServer.cs)
│   ├── Client/                    # Клиентский код (MainClient.cs)
│   ├── Entities/                  # ООП сущности (Hero.cs, SuperJumperHero.cs)
│   └── Shared/                    # Общий клиент-серверный код (Utils.cs)
│
├── src/                           # Luau исходники для синхронизации в Roblox
│   ├── ServerLoader.server.lua    # Серверная точка входа
│   ├── ClientLoader.client.lua    # Клиентская точка входа
│   ├── CoreSystem/                # Среда выполнения (ООП, коллекции, ошибки)
│   │   ├── init.lua               # OOP runtime, System.List, System.Dictionary
│   │   ├── ErrorHandler.lua       # Перехватчик ошибок ScriptContext.Error
│   │   └── SourceMapData.lua      # Автогенерированная карта строк C# <-> Luau
│   └── Generated/                 # Транспилированные Luau модули
│
└── docs/
    └── WIKI_API.md                # Полная документация и справочник API
```

---

## 🕹 Как писать код на C#

### 1. Серверная логика (`csharp/Server/MainServer.cs`)
```csharp
namespace RobloxCSharp.Server
{
    using System;
    using System.Collections.Generic;
    using Roblox;
    using RobloxCSharp.Entities;

    public class MainServer
    {
        private static List<Hero> heroList = new List<Hero>();

        public static void Init()
        {
            Console.WriteLine("[СЕРВЕР C#] Запуск игрового сервера!");

            // Слушаем подключение игроков
            Game.Players.PlayerAdded.Connect((playerInstance) =>
            {
                var player = (Player)playerInstance;
                player.CharacterAdded.Connect((characterInstance) =>
                {
                    var character = (Model)characterInstance;
                    var humanoid = (Humanoid)character.FindFirstChild("Humanoid");

                    if (humanoid != null)
                    {
                        var hero = new SuperJumperHero(player, character, humanoid);
                        hero.Initialize();
                        heroList.Add(hero);
                    }
                });
            });
        }
    }
}
```

### 2. ООП Сущности с наследованием (`csharp/Entities/SuperJumperHero.cs`)
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

            // Создаем золотую ауру и делаем супер прыжок
            CreateJumpAura();
            SuperJump();
        }

        public void SuperJump()
        {
            var rootPart = (BasePart)this.Character.FindFirstChild("HumanoidRootPart");
            if (rootPart == null || this.Humanoid == null) return;

            rootPart.AssemblyLinearVelocity = Vector3.New(0, this.SuperJumpForce, 0);
            this.Humanoid.Jump = true;
        }

        private void CreateJumpAura()
        {
            var aura = Instance.New<Highlight>("Highlight");
            aura.Name = "JumpAura";
            aura.FillColor = Color3.FromRGB(255, 215, 0);
            aura.Parent = this.Character;
        }
    }
}
```

---

## 🛠 RobloxAPI Generator

Шаблон содержит собственный CLI-генератор `RobloxAPI.Generator`, который создает биндинги всех сервисов, классов и енумов Roblox.

Для генерации или обновления биндингов:
```powershell
# Обновление из локального дампа
.\generate_types.bat

# Принудительное скачивание последней версии API с GitHub
.\RobloxAPI\generate.ps1 -Fetch
```

---

## 🔍 SourceMap: Отладка Runtime-Ошибок

При возникновении любого исключения или ошибки в Roblox Studio, глобальный перехватчик `ErrorHandler.lua` с помощью `SourceMapData.lua` декодирует стек вызовов обратно в исходный C# код:

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

## 📖 Справочник API и Документация

Подробное описание архитектуры, структур данных `CoreSystem`, коллекций, вызовов API и внутренностей трансляции доступно в файле [docs/WIKI_API.md](docs/WIKI_API.md).

---

## 📄 Лицензия

Проект распространяется под свободной лицензией MIT. Вы можете свободно использовать его в коммерческих и некоммерческих играх для Roblox.
