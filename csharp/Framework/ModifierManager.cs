namespace RobloxCSharp.Framework
{
    using System;
    using System.Collections.Generic;
    using Roblox;

    // Менеджер модификаторов: обновляет состояние модификаторов на каждом тике Heartbeat
    public static class ModifierManager
    {
        private static readonly List<ModifierBase> activeModifiers = new List<ModifierBase>();
        private static bool isInitialized = false;

        public static void Init()
        {
            if (isInitialized) return;
            isInitialized = true;

            // Подключаем физический цикл обновления через RunService.Heartbeat
            Game.RunService.Heartbeat.Connect((step) =>
            {
                double dt = 1.0 / 60.0;
                Update(dt);
            });
        }

        public static void AddModifier(CharacterBase target, ModifierBase modifier)
        {
            Init();
            if (target == null || modifier == null) return;

            if (!target.ActiveModifiers.Contains(modifier))
            {
                target.ActiveModifiers.Add(modifier);
            }

            if (!activeModifiers.Contains(modifier))
            {
                activeModifiers.Add(modifier);
            }

            RecalculateCharacterStats(target);
        }

        public static void RemoveModifier(CharacterBase target, ModifierBase modifier)
        {
            if (target != null)
            {
                target.ActiveModifiers.Remove(modifier);
                RecalculateCharacterStats(target);
            }
            activeModifiers.Remove(modifier);
        }

        // Обновление всех активных модификаторов на сервере
        public static void Update(double deltaTime)
        {
            if (activeModifiers.Count == 0) return;

            var toRemove = new List<ModifierBase>();

            for (int i = 0; i < activeModifiers.Count; i++)
            {
                var mod = activeModifiers[i];
                if (!mod.IsActive || mod.Target == null || mod.Target.Model == null || mod.Target.Model.Parent == null)
                {
                    toRemove.Add(mod);
                    continue;
                }

                mod.ElapsedTime += deltaTime;

                // 1. Обновление физических контроллеров движения (MotionBase)
                if (mod is MotionBase motion)
                {
                    motion.UpdateMotion(deltaTime);
                }

                // 2. Периодический тик (OnIntervalThink)
                if (mod.ThinkInterval > 0)
                {
                    if (mod.ElapsedTime - mod.LastThinkTime >= mod.ThinkInterval)
                    {
                        mod.LastThinkTime = mod.ElapsedTime;
                        mod.OnIntervalThink();
                    }
                }

                // 3. Проверка истечения времени действия (Duration)
                if (mod.Duration > 0 && mod.ElapsedTime >= mod.Duration)
                {
                    toRemove.Add(mod);
                }
            }

            // Очищаем завершившиеся модификаторы
            for (int i = 0; i < toRemove.Count; i++)
            {
                toRemove[i].Destroy();
            }
        }

        // Пересчет характеристик персонажа (скорость бега, сила прыжка и т.д.)
        public static void RecalculateCharacterStats(CharacterBase character)
        {
            if (character == null || character.Humanoid == null) return;

            double speedBonus = 0;
            double jumpBonus = 0;
            bool isStunned = false;

            for (int i = 0; i < character.ActiveModifiers.Count; i++)
            {
                var mod = character.ActiveModifiers[i];
                if (mod.IsActive)
                {
                    speedBonus += mod.GetWalkSpeedBonus();
                    jumpBonus += mod.GetJumpPowerBonus();
                    if (mod.IsStunned()) isStunned = true;
                }
            }

            if (isStunned)
            {
                character.Humanoid.WalkSpeed = 0;
                character.Humanoid.JumpPower = 0;
            }
            else
            {
                character.Humanoid.WalkSpeed = character.BaseWalkSpeed + speedBonus;
                character.Humanoid.JumpPower = character.BaseJumpPower + jumpBonus;
            }
        }
    }
}
