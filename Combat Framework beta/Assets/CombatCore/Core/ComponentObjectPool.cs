using System;
using System.Collections.Generic;

namespace EGamePlay
{
    public class ComponentObjectPool : IDisposable
    {
        private readonly Dictionary<Type, Queue<Component>> pool = new Dictionary<Type, Queue<Component>>();
        
        public static ComponentObjectPool Instance = new ComponentObjectPool();
        
        private ComponentObjectPool()
        {
        }

        public Component Fetch(Type type)
        {
            Queue<Component> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                return Activator.CreateInstance(type) as Component;
            }

            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type) as Component;
            }
            return queue.Dequeue();
        }

        public void Recycle(Component obj)
        {
            Type type = obj.GetType();
            Queue<Component> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                queue = new Queue<Component>(128);
                pool.Add(type, queue);
            }
            queue.Enqueue(obj);
        }

        public void Dispose()
        {
            this.pool.Clear();
        }
    }
}