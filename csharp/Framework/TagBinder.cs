namespace RobloxCSharp.Framework
{
    using System;
    using System.Collections.Generic;
    using Roblox;

    // Автоматическая привязка C# классов-обёрток к Roblox Instance по тегам CollectionService
    public static class TagBinder
    {
        private static Dictionary<Instance, object> _boundWrappers = new Dictionary<Instance, object>();

        // Привязывает фабрику класса-обёртки к тегу CollectionService
        public static void BindTag<TWrapper, TInstance>(string tagName, Func<TInstance, TWrapper> factory)
            where TWrapper : RobloxWrapper<TInstance>
            where TInstance : Instance
        {
            var collectionService = (CollectionService)Game.GetService("CollectionService");
            if (collectionService == null) return;

            // 1. Оборачиваем уже существующие инстансы в мире с этим тегом
            var existing = (object[])collectionService.GetTagged(tagName);
            if (existing != null)
            {
                foreach (var obj in existing)
                {
                    if (obj is TInstance inst)
                    {
                        AttachWrapper(inst, factory);
                    }
                }
            }

            // 2. Слушаем появление новых инстансов с тегом (созданных скриптами или загруженных)
            var addedSignal = (RBXScriptSignal)collectionService.GetInstanceAddedSignal(tagName);
            if (addedSignal != null)
            {
                addedSignal.Connect((addedInstance) =>
                {
                    if (addedInstance is TInstance inst)
                    {
                        AttachWrapper(inst, factory);
                    }
                });
            }

            // 3. Слушаем снятие тега с инстанса
            var removedSignal = (RBXScriptSignal)collectionService.GetInstanceRemovedSignal(tagName);
            if (removedSignal != null)
            {
                removedSignal.Connect((removedInstance) =>
                {
                    if (removedInstance is TInstance inst)
                    {
                        DetachWrapper(inst);
                    }
                });
            }
        }

        private static void AttachWrapper<TWrapper, TInstance>(TInstance instance, Func<TInstance, TWrapper> factory)
            where TWrapper : RobloxWrapper<TInstance>
            where TInstance : Instance
        {
            if (_boundWrappers.ContainsKey(instance)) return;

            var wrapper = factory(instance);
            _boundWrappers[instance] = wrapper;

            // Автоматическое удаление из словаря при вызове Destroy
            instance.Destroying.Connect(() =>
            {
                _boundWrappers.Remove(instance);
            });
        }

        private static void DetachWrapper(Instance instance)
        {
            if (_boundWrappers.TryGetValue(instance, out var wrapperObj))
            {
                _boundWrappers.Remove(instance);
                if (wrapperObj is IDisposable disp)
                {
                    disp.Dispose();
                }
            }
        }

        // Получение C# обёртки по сырому Roblox Instance
        public static TWrapper GetWrapper<TWrapper>(Instance instance) where TWrapper : class
        {
            if (_boundWrappers.TryGetValue(instance, out var wrapper))
            {
                return wrapper as TWrapper;
            }
            return null;
        }
    }
}
