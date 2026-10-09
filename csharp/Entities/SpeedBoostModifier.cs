namespace RobloxCSharp.Entities
{
    using System;
    using Roblox;
    using RobloxCSharp.Framework;

    // Модификатор ускорения (ModifierBase)
    [RegisterModifierBase("SpeedBoostModifier")]
    public class SpeedBoostModifier : ModifierBase
    {
        public double BonusSpeed { get; set; } = 14;

        public override void OnCreated(object customData)
        {
            Console.WriteLine("💨 [SpeedBoostModifier] Персонаж " + this.Target?.Name + " получил бонус к скорости +" + this.BonusSpeed);
        }

        // Возвращает бонус к WalkSpeed
        public override double GetWalkSpeedBonus() => this.BonusSpeed;

        public override void OnDestroy()
        {
            Console.WriteLine("🛑 [SpeedBoostModifier] Действие ускорения завершено.");
        }
    }
}
