namespace RobloxCSharp.Framework
{
    using System;
    using Roblox;

    // Базовый класс способности (Ability)
    public abstract class BaseAbility
    {
        public string Name { get; set; }
        public BaseHero Caster { get; set; }
        public double Cooldown { get; set; } = 0;
        public double LastCastTime { get; set; } = -999;
        public int Level { get; set; } = 1;
        public double ManaCost { get; set; } = 0;

        public BaseAbility(BaseHero caster, string name = null)
        {
            this.Caster = caster;
            this.Name = name ?? this.GetType().Name;
        }

        // Проверка готовности способности к касту
        public virtual bool CanCast()
        {
            if (this.Caster == null || this.Caster.IsDestroyed) return false;
            double now = DateTime.UtcNow.Ticks / 10000000.0;
            return (now - this.LastCastTime) >= this.Cooldown;
        }

        // Запуск способности
        public virtual bool Cast(BaseHero target = null, Vector3 targetPosition = null)
        {
            if (!CanCast()) return false;

            this.LastCastTime = DateTime.UtcNow.Ticks / 10000000.0;
            OnSpellStart(target, targetPosition);
            return true;
        }

        // Вызывается при создании способности
        public virtual void OnCreated() { }

        // Основной исполнительный хук логики способности
        public abstract void OnSpellStart(BaseHero target, Vector3 targetPosition);
    }

    // Базовый класс предмета инвентаря со способностью (Item)
    public abstract class BaseItem : BaseAbility
    {
        public Tool RawTool { get; private set; }
        public int Cost { get; set; } = 0;

        public BaseItem(BaseHero caster, Tool tool, string name = null) 
            : base(caster, name ?? (tool != null ? tool.Name : "Item"))
        {
            this.RawTool = tool;
            if (this.RawTool != null)
            {
                this.RawTool.Equipped.Connect(() => OnEquip());
                this.RawTool.Unequipped.Connect(() => OnUnequip());
                this.RawTool.Activated.Connect(() => Cast());
            }
        }

        public virtual void OnEquip() { }
        public virtual void OnUnequip() { }
    }
}
