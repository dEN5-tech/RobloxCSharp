namespace RobloxCSharp.Framework
{
    using System;
    using System.Collections.Generic;
    using Roblox;

    // Глобальный менеджер модификаторов: обновляет таймеры, тики и статы героев
    public static class ModifierManager
    {
        private static List<BaseModifier> _allActiveModifiers = new List<BaseModifier>();

        public static void RegisterModifier(BaseHero target, BaseModifier modifier)
        {
            if (target == null || modifier == null) return;

            target.Modifiers.Add(modifier);
            _allActiveModifiers.Add(modifier);

            UpdateStats(target);
        }

        public static void UnregisterModifier(BaseHero target, BaseModifier modifier)
        {
            if (target != null)
            {
                target.Modifiers.Remove(modifier);
                UpdateStats(target);
            }
            _allActiveModifiers.Remove(modifier);
        }

        // Обновление характеристик Roblox Humanoid на основе всех активных модификаторов
        public static void UpdateStats(BaseHero hero)
        {
            if (hero == null || hero.Humanoid == null || hero.IsDestroyed) return;

            double speedAdd = 0;
            double jumpAdd = 0;

            foreach (var mod in hero.Modifiers)
            {
                if (mod.IsActive)
                {
                    speedAdd += mod.CheckWalkSpeedModifier();
                    jumpAdd += mod.CheckJumpPowerModifier();
                }
            }

            hero.Humanoid.WalkSpeed = hero.BaseWalkSpeed + speedAdd;
            hero.Humanoid.JumpPower = hero.BaseJumpPower + jumpAdd;
        }

        // Вызывается на каждый игровой тик сервера для обновления длительности и интервалов
        public static void UpdateTick()
        {
            double now = DateTime.UtcNow.Ticks / 10000000.0;

            for (int i = _allActiveModifiers.Count - 1; i >= 0; i--)
            {
                var mod = _allActiveModifiers[i];
                if (!mod.IsActive || mod.Target.IsDestroyed)
                {
                    mod.Destroy();
                    continue;
                }

                // 1. Проверка истечения времени действия
                if (mod.Duration >= 0 && (now - mod.StartTime) >= mod.Duration)
                {
                    mod.Destroy();
                    continue;
                }

                // 2. Проверка тика интервала (OnIntervalThink)
                if (mod.Interval > 0 && (now - mod.LastThinkTime) >= mod.Interval)
                {
                    mod.LastThinkTime = now;
                    mod.OnIntervalThink();
                }
            }
        }
    }
}
