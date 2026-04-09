using System;
using System.Collections.Generic;

namespace CombatCore.Core
{
    public class EntityObjectPool : IDisposable
    {
        private readonly Dictionary<Type, Queue<Entity>> _pool = new();
        
        public static readonly EntityObjectPool Instance = new();
        
        private EntityObjectPool() { }

        public T Fetch<T>() where T : Entity, new()
        {
            Type type = typeof(T);
            
            if (_pool.TryGetValue(type, out var queue) && queue.Count > 0)
            {
                return queue.Dequeue() as T;
            }
            
            return new T();
        }

        public void Recycle(Entity obj)
        {
            if (obj == null) return;

            Type type = obj.GetType();
            if (!_pool.TryGetValue(type, out var queue))
            {
                queue = new Queue<Entity>(128);
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