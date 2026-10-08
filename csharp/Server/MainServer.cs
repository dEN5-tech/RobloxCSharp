namespace RobloxCSharp.Server
{
    using System;
    using System.Collections.Generic;
    using Roblox;
    using RobloxCSharp.Entities;

    // Главный сервер: регистрирует игроков и создает для них героев
    public class MainServer
    {
        // Список всех активных героев на сервере
        private static List<Hero> heroList = new List<Hero>();

        public static void Init()
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("[СЕРВЕР C#] Запуск игрового сервера с C# Героями!");
            Console.WriteLine("=================================================");

            // 1. Слушаем подключение новых игроков
            Game.Players.PlayerAdded.Connect((playerInstance) =>
            {
                var player = (Player)playerInstance;
                OnPlayerConnected(player);
            });

            // 2. Обрабатываем игроков, которые уже есть на сервере
            var currentPlayers = (object[])Game.Players.GetPlayers();
            if (currentPlayers != null)
            {
                foreach (var p in currentPlayers)
                {
                    if (p != null)
                    {
                        OnPlayerConnected((Player)p);
                    }
                }
            }

            Console.WriteLine("[СЕРВЕР C#] Сервер готов к игре!");
        }

        // Обработка подключения игрока
        private static void OnPlayerConnected(Player player)
        {
            if (player == null) return;
            Console.WriteLine("[СЕРВЕР C#] Подключился игрок: " + player.Name);

            // Слушаем появление персонажа в игре
            player.CharacterAdded.Connect((characterInstance) =>
            {
                var character = (Model)characterInstance;
                SpawnHeroForPlayer(player, character);
            });

            // Если персонаж уже загрузился
            if (player.Character != null)
            {
                SpawnHeroForPlayer(player, player.Character);
            }
        }

        // Создание экземпляра Героя для игрока
        private static void SpawnHeroForPlayer(Player player, Model character)
        {
            var humanoidInstance = character.FindFirstChild("Humanoid");
            var humanoid = (Humanoid)humanoidInstance;

            if (humanoid == null)
            {
                Console.WriteLine("[СЕРВЕР C#] Предупреждение: Humanoid не найден у " + player.Name);
                return;
            }

            // Создаем экземпляр Героя-Прыгуна (SuperJumperHero)
            Hero newHero = new SuperJumperHero(player, character, humanoid);

            // Инициализируем героя (свойства, интерфейс, аура, прыжок)
            newHero.Initialize();

            // Сохраняем в список
            heroList.Add(newHero);
        }
    }
}
