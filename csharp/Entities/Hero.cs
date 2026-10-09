namespace RobloxCSharp.Entities
{
    using System;
    using Roblox;
    using RobloxCSharp.Framework;

    // Класс Героя: расширяет базовый CharacterBase
    public class Hero : CharacterBase
    {
        public Hero(Player player, Model character, Humanoid humanoid)
            : base(player, character, humanoid)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            Console.WriteLine("[Hero] Инициализирован игровой герой: " + this.Name);
        }

        // Вызов основного действия героя
        public virtual void PerformMainAction()
        {
            Console.WriteLine("[Hero] Герой " + this.Name + " выполняет базовое действие.");
        }
    }
}
