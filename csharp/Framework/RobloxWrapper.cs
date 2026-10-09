namespace RobloxCSharp.Framework
{
    using System;
    using System.Collections.Generic;
    using Roblox;

    // Универсальный класс-обёртка над любым Roblox Instance (Паттерн: Композиция + generic-параметризация)
    public abstract class RobloxWrapper<T> where T : Instance
    {
        // Прямой доступ к оригинальному Roblox C++ инстансу
        public readonly T RawInstance;

        // Список всех отслеживаемых сигналов для гарантированной отписки при уничтожении
        private List<object> _trackedConnections = new List<object>();

        // Флаг состояния жизни объекта
        public bool IsDestroyed { get; private set; } = false;

        public RobloxWrapper(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            this.RawInstance = instance;

            // Автоматическая очистка обёртки при удалении инстанса из DataModel Roblox
            this.RawInstance.Destroying.Connect(() =>
            {
                Dispose();
            });
        }

        // Инлайн-свойства базовых операций
        public string Name
        {
            get => this.RawInstance.Name;
            set => this.RawInstance.Name = value;
        }

        public Instance Parent
        {
            get => this.RawInstance.Parent;
            set => this.RawInstance.Parent = value;
        }

        // Поиск дочернего элемента с автоматическим приведением типов
        public TChild FindChild<TChild>(string childName) where TChild : Instance
        {
            var child = this.RawInstance.FindFirstChild(childName);
            return child as TChild;
        }

        // Регистрация соединения события с авто-отпиской при уничтожении
        public void TrackConnection(object connection)
        {
            if (connection != null)
            {
                this._trackedConnections.Add(connection);
            }
        }

        // Полное удаление инстанса и очистка всех связей
        public virtual void Destroy()
        {
            if (this.IsDestroyed) return;
            Dispose();
            this.RawInstance.Destroy();
        }

        // Внутреннее освобождение ресурсов обёртки
        protected virtual void Dispose()
        {
            if (this.IsDestroyed) return;
            this.IsDestroyed = true;

            // Отписка от всех событий
            this._trackedConnections.Clear();
            OnDestroyed();
        }

        // Хук для переопределения в дочерних классах
        protected virtual void OnDestroyed() { }
    }
}
