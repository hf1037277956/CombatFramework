using System;
using System.Collections.Generic;
using CombatCore.Core.Singleton;
using CombatCore.Core.Object; // 假设 DisposeObject 在此命名空间

namespace CombatCore.Core
{
    public sealed class EntityCoreNode : Singleton<EntityCoreNode>
    {
        // ================== 数据存储（生命周期与索引） ==================

        // 用于通过 InstanceId 极速查找 (O(1))
        public Dictionary<long, Entity> AllEntities { get; private set; } = new Dictionary<long, Entity>(1000);
        
        // 用于通过 Type 批量查找 (例如未来做空间划分时，查出所有的 ColliderEntity)
        public Dictionary<Type, List<Entity>> EntitiesByType { get; private set; } = new Dictionary<Type, List<Entity>>(50);
        
        // 跟踪所有创建出来的 Component，主要用于在 CoreNode 销毁时进行兜底清理
        public List<Component> AllComponents { get; private set; } = new List<Component>(1000);

        // ================== 轮询驱动队列 (缓存命中友好) ==================
        
        // 仅存储实现了 IUpdate 的对象 (Entity 或 Component)
        private readonly List<IUpdate> _updatables = new List<IUpdate>(500);
        
        // 仅存储实现了 IFixedUpdate 的对象
        private readonly List<IFixedUpdate> _fixedUpdatables = new List<IFixedUpdate>(500);

        // ================== 实体管理 API ==================

        public void AddEntity(Entity entity)
        {
            if (entity == null || entity.IsDisposed) return;

            var entityType = entity.GetType();
            
            // 1. 加入 ID 索引
            AllEntities[entity.InstanceId] = entity;

            // 2. 加入 Type 索引
            if (!EntitiesByType.TryGetValue(entityType, out var list))
            {
                list = new List<Entity>();
                EntitiesByType[entityType] = list;
            }
            list.Add(entity);
            
            // 3. 注册可能的 Update 特征
            RegisterUpdate(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            if (entity == null) return;

            AllEntities.Remove(entity.InstanceId);
            
            if (EntitiesByType.TryGetValue(entity.GetType(), out var list))
            {
                list.Remove(entity);
            }
            
            // 注意：这里不需要手动从 _updatables 中 Remove
            // 因为我们在 Update 循环中做了 IsDisposed 的自动 O(1) 剔除
        }

        // ================== 生命周期轮询注册 ==================

        /// <summary>
        /// 核心注册：利用 C# 的模式匹配，自动捕获对象身上的轮询特征
        /// </summary>
        public void RegisterUpdate<T>(T obj) where T : DisposeObject
        {
            // 如果继承了 IUpdate，装入渲染层更新队列
            if (obj is IUpdate updatable)
            {
                _updatables.Add(updatable);
            }
            
            // 如果继承了 IFixedUpdate，装入逻辑层推演队列
            if (obj is IFixedUpdate fixedUpdatable)
            {
                _fixedUpdatables.Add(fixedUpdatable);
            }
        }

        // ================== 引擎轮询泵 (Engine Pumps) ==================

        public void Update(float deltaTime)
        {
            // 倒序遍历，配合无序 O(1) 移除，将 List 元素移除的开销降到最低
            for (int i = _updatables.Count - 1; i >= 0; i--)
            {
                var updatable = _updatables[i];
                
                // 实体或组件被销毁时，将其从轮询队列中剔除
                if (((DisposeObject)updatable).IsDisposed)
                {
                    int lastIndex = _updatables.Count - 1;
                    _updatables[i] = _updatables[lastIndex]; // 将末尾元素覆盖到当前位置
                    _updatables.RemoveAt(lastIndex);         // 删掉末尾元素
                    continue;
                }

                updatable.Update(deltaTime);
            }
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            // 为未来的固定步长推演预留的高性能循环
            for (int i = _fixedUpdatables.Count - 1; i >= 0; i--)
            {
                var fixedUpdatable = _fixedUpdatables[i];
                
                if (((DisposeObject)fixedUpdatable).IsDisposed)
                {
                    int lastIndex = _fixedUpdatables.Count - 1;
                    _fixedUpdatables[i] = _fixedUpdatables[lastIndex];
                    _fixedUpdatables.RemoveAt(lastIndex);
                    continue;
                }

                fixedUpdatable.FixedUpdate(fixedDeltaTime);
            }
        }

        // ================== 释放与清理 ==================

        public override void Dispose()
        {
            Clear();
            base.Dispose();
        }

        public void Clear()
        {
            // 倒序销毁所有实体
            foreach (var entity in AllEntities.Values)
            {
                if (!entity.IsDisposed)
                {
                    entity.Dispose();
                }
            }
            
            AllEntities.Clear();
            EntitiesByType.Clear();
            AllComponents.Clear();
            _updatables.Clear();
            _fixedUpdatables.Clear();
        }
    }
}