namespace RobloxCSharp.Framework
{
    using System;
    using System.Collections.Generic;
    using Roblox;

    public enum MotionType
    {
        None,
        Horizontal,
        Vertical,
        Both
    }

    // Базовый класс модификатора (Buff / Debuff / Passive / Aura / Motion)
    public class BaseModifier
    {
        public BaseHero Target { get; set; }
        public BaseHero Caster { get; set; }
        public BaseAbility Ability { get; set; }
        public double Duration { get; set; } = -1; // -1 = бесконечно (пассивка/аура)
        public double StartTime { get; set; }
        public double Interval { get; set; } = 0;
        public double LastThinkTime { get; set; } = 0;
        public bool IsActive { get; private set; } = true;
        public string Name { get; set; }

        public virtual MotionType MotionType => MotionType.None;

        // Список порожденных модификатором визуальных эффектов (Highlight, Part, Light), удаляемых вместе с ним
        private List<Instance> _spawnedVisuals = new List<Instance>();

        // Универсальный фабричный метод наложения модификатора (Dota 2 / TypeScript pattern: BaseModifier.apply)
        public static T Apply<T>(BaseHero target, BaseHero caster, BaseAbility ability, double duration = -1, object customData = null) 
            where T : BaseModifier, new()
        {
            if (target == null || target.IsDestroyed) return null;

            var modifier = new T();
            modifier.Target = target;
            modifier.Caster = caster;
            modifier.Ability = ability;
            modifier.Duration = duration;
            modifier.StartTime = DateTime.UtcNow.Ticks / 10000000.0;
            modifier.LastThinkTime = modifier.StartTime;
            modifier.Name = typeof(T).Name;

            ModifierManager.RegisterModifier(target, modifier);
            modifier.OnCreated(customData);

            return modifier;
        }

        // Регистрация временного визуального объекта (Highlight, Light, Part) для авто-удаления
        public void AddTrackedVisual(Instance visual)
        {
            if (visual != null)
            {
                this._spawnedVisuals.Add(visual);
            }
        }

        // Жизненный цикл
        public virtual void OnCreated(object customData) { }
        public virtual void OnIntervalThink() { }
        public virtual void OnDestroy() { }

        // Модификаторы характеристик героя (переопределяются в потомках)
        public virtual double CheckWalkSpeedModifier() => 0;
        public virtual double CheckJumpPowerModifier() => 0;
        public virtual double CheckDamageReduction() => 0;
        public virtual double CheckHealthRegenModifier() => 0;

        // Уничтожение модификатора
        public void Destroy()
        {
            if (!this.IsActive) return;
            this.IsActive = false;

            // Очищаем прикрепленные визуальные эффекты
            foreach (var visual in this._spawnedVisuals)
            {
                if (visual != null)
                {
                    visual.Destroy();
                }
            }
            this._spawnedVisuals.Clear();

            OnDestroy();
            ModifierManager.UnregisterModifier(this.Target, this);
        }
    }
}
