using System;
using System.Collections.Generic;

namespace CombatCore.Core
{
    public class ComponentObjectPool : IDisposable
    {
        private readonly Dictionary<Type, Queue<Component>> _pool = new();
        
        public static readonly ComponentObjectPool Instance = new();
        
        private ComponentObjectPool() { }

        public T Fetch<T>() where T : Component, new()
        {
            Type type = typeof(T);
            
            if (_pool.TryGetValue(type, out var queue) && queue.Count > 0)
            {
                return queue.Dequeue() as T;
            }
            
            return new T();
        }

        public void Recycle(Component obj)
        {
            if (obj == null) return;
            
            Type type = obj.GetType();
            if (!_pool.TryGetValue(type, out var queue))
            {
                queue = new Queue<Component>(128);
                _pool.Add(type, queue);
            }
            queue.Enqueue(obj);
        }

        public void Dispose()
        {
            this._pool.Clear();
        }
    }
}