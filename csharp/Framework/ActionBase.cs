namespace RobloxCSharp.Framework
{
    using System;
    using Roblox;

    // Базовый класс для игровых действий / способностей персонажа (ActionBase)
    public abstract class ActionBase
    {
        public string Name { get; set; }
        public CharacterBase Owner { get; set; }
        public double Cooldown { get; set; } = 0;
        public double LastExecuteTime { get; set; } = 0;
        public bool IsPassive { get; set; } = false;

        public ActionBase(CharacterBase owner, string name)
        {
            this.Owner = owner;
            this.Name = name;
        }

        // Проверка возможности выполнения действия
        public virtual bool CanExecute()
        {
            if (this.Owner == null || this.Owner.Humanoid == null) return false;
            if (this.Owner.Humanoid.Health <= 0) return false;

            double currentTime = DateTime.UtcNow.Ticks / 10000000.0;
            return (currentTime - this.LastExecuteTime) >= this.Cooldown;
        }

        // Выполнение действия
        public virtual void Execute(CharacterBase target = null, Vector3 targetPosition = null)
        {
            if (!CanExecute())
            {
                Console.WriteLine("⏳ [ActionBase] Действие '" + this.Name + "' перезаряжается.");
                return;
            }

            this.LastExecuteTime = DateTime.UtcNow.Ticks / 10000000.0;
            OnActionStart(target, targetPosition);
        }

        // Обработчик логики действия
        public abstract void OnActionStart(CharacterBase target, Vector3 targetPosition);
    }
}
