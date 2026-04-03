using System;
using System.Collections.Generic;
using CombatCore.Core.Singleton;

namespace CombatCore.Core
{
    public class ObjectPool : Singleton<ObjectPool>
    {
        private readonly Dictionary<Type, Queue<object>> _pool = new();
        
        public T Fetch<T>() where T: class
        {
            return this.Fetch(typeof (T)) as T;
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
        
        public void Recycle(object obj)
        {
            Type type = obj.GetType();
            
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