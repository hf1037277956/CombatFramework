using System;
using System.Collections.Generic;
using CombatCore.Core.Singleton;

namespace CombatCore.Core
{
    public class ObjectPool : Singleton<ObjectPool>
    {
        private readonly Dictionary<Type, Queue<object>> _pool = new();
        
        public T Fetch<T>() where T : class, new()
        {
            if (!_pool.TryGetValue(typeof(T), out var queue) || queue.Count == 0)
                return new T();

            return (T)queue.Dequeue();
        }

        public object Fetch(Type type)
        {
            if (!_pool.TryGetValue(type, out var queue))
            {
                return Activator.CreateInstance(type);
            }
            
            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type);
            }
            
            return queue.Dequeue();
        }
        
        public void Recycle<T>(T obj) where T : class
        {
            this.Recycle(typeof(T), obj);
        }
        
        public void Recycle(object obj)
        {
            Type type = obj.GetType();
            this.Recycle(type, obj);
        }
        
        private void Recycle(Type type, object obj)
        {
            if (!_pool.TryGetValue(type, out var queue))
            {
                queue = new Queue<object>();
                _pool.Add(type, queue);
            }
            
            // 一种对象最大为1000个
            if (queue.Count > 1000)
            {
                return;
            }
            
            queue.Enqueue(obj);
        }
    }
}