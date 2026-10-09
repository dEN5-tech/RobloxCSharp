namespace RobloxCSharp.Framework
{
    using System;
    using Roblox;

    // Базовый примитив модификатора / статус-эффекта Roblox (ModifierBase)
    public class ModifierBase
    {
        public CharacterBase Target { get; set; }
        public CharacterBase Caster { get; set; }
        public double Duration { get; set; } = -1; // -1 = бесконечный
        public double StartTime { get; set; }
        public double ElapsedTime { get; set; } = 0;
        public double ThinkInterval { get; set; } = 0;
        public double LastThinkTime { get; set; } = 0;
        public bool IsActive { get; private set; } = true;
        public string Name { get; set; }

        public virtual MotionType MotionType => MotionType.None;

        // Статический фабричный метод наложения модификатора (Apply)
        public static T Apply<T>(CharacterBase target, CharacterBase caster, double duration = -1, object customData = null)
            where T : ModifierBase, new()
        {
            if (target == null) return null;

            var modifier = new T();
            modifier.Target = target;
            modifier.Caster = caster;
            modifier.Duration = duration;
            modifier.StartTime = DateTime.UtcNow.Ticks / 10000000.0;
            modifier.Name = typeof(T).Name;

            ModifierManager.AddModifier(target, modifier);
            modifier.OnCreated(customData);

            return modifier;
        }

        // Хук создания модификатора
        public virtual void OnCreated(object customData) { }

        // Хук периодического тика таймера
        public virtual void OnIntervalThink() { }

        // Хук уничтожения и очистки модификатора
        public virtual void OnDestroy() { }

        // Хук обновления модификатора при повторном наложении
        public virtual void OnRefresh(object customData)
        {
            this.ElapsedTime = 0;
            this.StartTime = DateTime.UtcNow.Ticks / 10000000.0;
        }

        // Запуск периодического интервала тиков (OnIntervalThink)
        public void StartIntervalThink(double interval)
        {
            this.ThinkInterval = interval;
            this.LastThinkTime = this.ElapsedTime;
        }

        // Завершение и удаление модификатора
        public void Destroy()
        {
            if (!this.IsActive) return;
            this.IsActive = false;
            OnDestroy();
            ModifierManager.RemoveModifier(this.Target, this);
        }

        // Модификаторы параметров персонажа в Roblox Engine
        public virtual double GetWalkSpeedBonus() => 0;
        public virtual double GetJumpPowerBonus() => 0;
        public virtual double GetMaxHealthBonus() => 0;
        public virtual bool IsStunned() => false;
        public virtual bool IsSilenced() => false;
    }
}
