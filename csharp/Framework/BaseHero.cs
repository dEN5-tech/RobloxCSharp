namespace RobloxCSharp.Framework
{
    using System;
    using System.Collections.Generic;
    using Roblox;

    // Базовый класс игрового персонажа / героя / NPC (обёртка над Model)
    public class BaseHero : RobloxWrapper<Model>
    {
        public Player Player { get; private set; }
        public Humanoid Humanoid { get; private set; }
        public BasePart RootPart { get; private set; }
        
        public double BaseWalkSpeed { get; set; } = 16;
        public double BaseJumpPower { get; set; } = 50;
        public double MaxHealth { get; set; } = 100;
        public double Health { get; set; } = 100;

        // Список способностей героя
        public List<BaseAbility> Abilities { get; private set; } = new List<BaseAbility>();

        // Список активных модификаторов героя
        public List<BaseModifier> Modifiers { get; private set; } = new List<BaseModifier>();

        public BaseHero(Model model, Player player = null) : base(model)
        {
            this.Player = player;
            this.Humanoid = FindChild<Humanoid>("Humanoid");
            this.RootPart = FindChild<BasePart>("HumanoidRootPart");

            if (this.Humanoid != null)
            {
                this.Humanoid.MaxHealth = this.MaxHealth;
                this.Humanoid.Health = this.Health;
                this.Humanoid.WalkSpeed = this.BaseWalkSpeed;
                this.Humanoid.JumpPower = this.BaseJumpPower;

                // Отслеживаем изменение здоровья
                this.Humanoid.HealthChanged.Connect((p) =>
                {
                    this.Health = this.Humanoid.Health;
                    if (this.Health <= 0)
                    {
                        OnDeath();
                    }
                });
            }
        }

        // Добавление способности герою
        public T AddAbility<T>() where T : BaseAbility
        {
            var ability = (T)Activator.CreateInstance(typeof(T), this);
            this.Abilities.Add(ability);
            ability.OnCreated();
            return ability;
        }

        // Поиск способности по типу
        public T GetAbility<T>() where T : BaseAbility
        {
            foreach (var ab in this.Abilities)
            {
                if (ab is T typed) return typed;
            }
            return null;
        }

        // Нанесение урона
        public virtual void TakeDamage(double amount, BaseHero attacker = null, BaseAbility sourceAbility = null)
        {
            if (this.IsDestroyed || this.Humanoid == null) return;
            
            // Проверяем модификаторы защиты
            double reduction = 0;
            foreach (var mod in this.Modifiers)
            {
                reduction += mod.CheckDamageReduction();
            }

            double finalDamage = Math.Max(0, amount * (1.0 - reduction));
            this.Humanoid.TakeDamage(finalDamage);
        }

        // Лечение
        public virtual void Heal(double amount)
        {
            if (this.IsDestroyed || this.Humanoid == null) return;
            this.Humanoid.Health = Math.Min(this.MaxHealth, this.Humanoid.Health + amount);
        }

        // Хук смерти
        protected virtual void OnDeath()
        {
            // Очищаем все модификаторы при смерти
            for (int i = this.Modifiers.Count - 1; i >= 0; i--)
            {
                this.Modifiers[i].Destroy();
            }
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();
            for (int i = this.Modifiers.Count - 1; i >= 0; i--)
            {
                this.Modifiers[i].Destroy();
            }
            this.Abilities.Clear();
        }
    }
}
