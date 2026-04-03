using System;
using System.Collections.Generic;
//using Cysharp.Threading.Tasks;

namespace EGamePlay
{
    public class EntityObjectPool : IDisposable
    {
        private readonly Dictionary<Type, Queue<CombatCore.Core.Entity>> pool = new Dictionary<Type, Queue<CombatCore.Core.Entity>>();
        
        public static EntityObjectPool Instance = new EntityObjectPool();
        
        private EntityObjectPool()
        {
        }

        public CombatCore.Core.Entity Fetch(Type type)
        {
            Queue<CombatCore.Core.Entity> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                return Activator.CreateInstance(type) as CombatCore.Core.Entity;
            }

            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type) as CombatCore.Core.Entity;
            }
            return queue.Dequeue();
        }

        public void Recycle(CombatCore.Core.Entity obj)
        {
            Type type = obj.GetType();
            Queue<CombatCore.Core.Entity> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                queue = new Queue<CombatCore.Core.Entity>(128);
                pool.Add(type, queue);
            }
            queue.Enqueue(obj);
        }
        
        // /// <summary>
        // /// 异步分帧预加载 (泛型版本)
        // /// </summary>
        // /// <typeparam name="T">Entity类型</typeparam>
        // /// <param name="count">总数量</param>
        // /// <param name="itemsPerFrame">每帧创建的数量(建议10-20)</param>
        // public async UniTask PreloadPoolAsync<T>(int count, int itemsPerFrame = 10) where T : Entity
        // {
        //     await PreloadPoolAsync(typeof(T), count, itemsPerFrame);
        // }
        //
        // /// <summary>
        // /// 异步分帧预加载 (Type版本)
        // /// </summary>
        // public async UniTask PreloadPoolAsync(Type type, int count, int itemsPerFrame = 10)
        // {
        //     if (!pool.TryGetValue(type, out var queue))
        //     {
        //         queue = new Queue<Entity>(count);
        //         pool.Add(type, queue);
        //     }
        //
        //     for (int i = 0; i < count; i++)
        //     {
        //         var entity = Activator.CreateInstance(type) as Entity;
        //         if (entity != null)
        //         {
        //             queue.Enqueue(entity);
        //         }
        //         
        //         if ((i + 1) % itemsPerFrame == 0)
        //         {
        //             await UniTask.Yield();
        //         }
        //     }
        // }

        public void Dispose()
        {
            this.pool.Clear();
        }
    }
}