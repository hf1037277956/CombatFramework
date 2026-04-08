using System;
using System.Collections.Generic;
using CombatCore.Core.Object;
using EGamePlay;

namespace CombatCore.Core
{
    [Flags]
    public enum EntityStatus: byte
    {
        None = 0,
        IsFromPool = 1,
        IsCreated = 1 << 1,
        IsNew = 1 << 2,
    }
    
    // 实体类
    public partial class Entity : DisposeObject
    {
        public long InstanceId { get; protected set; }

        protected Entity()
        {
            
        }

        private EntityStatus _status = EntityStatus.None;

        private bool IsFromPool
        {
            get => (this._status & EntityStatus.IsFromPool) == EntityStatus.IsFromPool;
            set
            {
                if (value)
                {
                    this._status |= EntityStatus.IsFromPool;
                }
                else
                {
                    this._status &= ~EntityStatus.IsFromPool;
                }
            }
        }
        
        protected bool IsCreated
        {
            get => (this._status & EntityStatus.IsCreated) == EntityStatus.IsCreated;
            set
            {
                if (value)
                {
                    this._status |= EntityStatus.IsCreated;
                }
                else
                {
                    this._status &= ~EntityStatus.IsCreated;
                }
            }
        }
        
        protected bool IsNew
        {
            get => (this._status & EntityStatus.IsNew) == EntityStatus.IsNew;
            set
            {
                if (value)
                {
                    this._status |= EntityStatus.IsNew;
                }
                else
                {
                    this._status &= ~EntityStatus.IsNew;
                }
            }
        }

        public bool IsDisposed => InstanceId == 0;
        
        protected Entity _parent;

        public Entity Parent
        {
            get => this._parent;
            private set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {this.GetType().Name}");
                }

                if (value == this)
                {
                    throw new Exception($"cant set parent self: {this.GetType().Name}");
                }

                // 如果之前有parent
                if (this._parent != null) 
                {
                    // parent相同，不设置
                    if (this._parent == value)
                    {
                        //Log.Error($"重复设置了Parent: {this.GetType().Name} parent: {this.parent.GetType().Name}");
                        return;
                    }

                    this._parent.RemoveFromChildren(this);
                }

                this._parent = value;
                this._parent.AddToChildren(this);
            }
        }
        
        public T GetParent<T>() where T : Entity
        {
            return _parent as T;
        }
        
        // ========================== 子实体 ==========================
        
        private Dictionary<long, Entity> _children;
        
        public Dictionary<long, Entity> Children
        {
            get
            {
                return this._children ??= ObjectPool.Instance.Fetch<Dictionary<long, Entity>>();
            }
        }
        
        private void AddToChildren(Entity entity)
        {
            this.Children.Add(entity.InstanceId, entity);
        }
        
        private void RemoveFromChildren(Entity entity)
        {
            if (this._children == null)
            {
                return;
            }

            this._children.Remove(entity.InstanceId);

            if (this._children.Count == 0)
            {
                ObjectPool.Instance.Recycle(this._children);
                this._children = null;
            }
        }
        
        // ========================== 组件 ==========================
        
        private Dictionary<Type, Component> _components;
        
        public Dictionary<Type, Component> Components
        {
            get
            {
                return this._components ??= ObjectPool.Instance.Fetch<Dictionary<Type, Component>>();
            }
        }

        public virtual void Awake()
        {

        }
        
        public virtual void Awake<T>(T a) 
        {
            
        }
        //
        // public virtual void Start()
        // {
        //
        // }
        //
        // public virtual void Start(object initData)
        // {
        //
        // }
        //
        // public virtual void OnSetParent(Entity preParent, Entity nowParent)
        // {
        //
        // }
        //
        // public virtual void Update(float deltaTime)
        // {
        //
        // }
        //
        // public virtual void OnDestroy()
        // {
        //
        // }
        
        private void RegisterUpdate(Entity entity)
        {
            if (entity is IUpdate updateEntity)
            {
                UpdateManager.Instance.Register(updateEntity);
            }
        }
        
        private void RegisterUpdate(Component component)
        {
            if (component is IUpdate updateComp)
            {
                UpdateManager.Instance.Register(updateComp);
            }
        }
        
        public T GetComponent<T>() where T : Component
        {
            if (this._components == null)
            {
                return null;
            }

            if (!this._components.TryGetValue(typeof (T), out var component))
            {
                return null;
            }

            return (T) component;
        }
        
        public Component GetComponent(Type type)
        {
            if (this._components == null)
            {
                return null;
            }

            if (!this._components.TryGetValue(type, out var component))
            {
                return null;
            }

            return component;
        }
        
        // ========================== 创建实体 ==========================
        
        private static Entity Create(Type type, bool isFromPool)
        {
            Entity entity;
            if (isFromPool)
            {
                entity = EntityObjectPool.Instance.Fetch(type);
            }
            else
            {
                entity = Activator.CreateInstance(type) as Entity; 
            }
            
            entity.InstanceId = IdFactory.NewInstanceId();
            entity.IsFromPool = isFromPool;
            entity.IsCreated = true;
            entity.IsNew = true;
            
            return entity;
        }
        
        // ========================== 挂载新建组件 ==========================
        
        public Component AddComponent(Component component)
        {
            Type type = component.GetType();
            if (this._components != null && this._components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }
            
            component.Parent = this;
            component.IsDisposed = false;
            Components.Add(type, component);
            //EGamePlay.Entity.Master?.AllComponents.Add(component);
            
            component.Awake();

            RegisterUpdate(component);
            
            return component;
        }

        public Component AddComponent(Type type, bool isFromPool = false)
        {
            if (this._components != null && this._components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Component component;
            if (isFromPool)
            {
                // 从对象池获取
                component = ComponentObjectPool.Instance.Fetch(type);
                component.IsFromPool = true; // 标记来源
            }
            else
            {
                // 强制新建
                component = Activator.CreateInstance(type) as Component;
                // 默认不是从池里来的 (如果 Component 类也有 IsFromPool 属性的话，建议显式设为 false)
                component.IsFromPool = false; 
            }
            
            component.Parent = this;
            component.IsDisposed = false;
            Components.Add(type, component);
            //EGamePlay.Entity.Master?.AllComponents.Add(component);
            
            component.Awake();
            
            RegisterUpdate(component);
            
            return component;
        }

        public T AddComponent<T>(bool isFromPool = false) where T : Component
        {
            Type type = typeof (T);
            if (this._components != null && this._components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Component component;
            if (isFromPool)
            {
                // 从对象池获取
                component = ComponentObjectPool.Instance.Fetch(type);
                component.IsFromPool = true; // 标记来源
            }
            else
            {
                // 强制新建
                component = Activator.CreateInstance(type) as Component;
                // 默认不是从池里来的 (如果 Component 类也有 IsFromPool 属性的话，建议显式设为 false)
                component.IsFromPool = false; 
            }

            component.Parent = this;
            component.IsDisposed = false;
            Components.Add(typeof(T), component);
            //EGamePlay.Entity.Master?.AllComponents.Add(component);
            
            component.Awake();
            
            RegisterUpdate(component);
            
            return component as T;
        }

        public T AddComponent<T, TP1>(TP1 p1, bool isFromPool = false) where T : Component
        {
            Type type = typeof (T);
            if (this._components != null && this._components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Component component;
            if (isFromPool)
            {
                // 从对象池获取
                component = ComponentObjectPool.Instance.Fetch(type);
                component.IsFromPool = true; // 标记来源
            }
            else
            {
                // 强制新建
                component = Activator.CreateInstance(type) as Component;
                // 默认不是从池里来的 (如果 Component 类也有 IsFromPool 属性的话，建议显式设为 false)
                component.IsFromPool = false; 
            }

            component.Parent = this;
            component.IsDisposed = false;
            Components.Add(typeof(T), component);
            //EGamePlay.Entity.Master?.AllComponents.Add(component);
            
            component.Awake(p1);
            
            RegisterUpdate(component);
            
            return component as T;
        }

        // public K AddComponent<K, P1, P2>(P1 p1, P2 p2, bool isFromPool = false) where K : Entity, IAwake<P1, P2>, new()
        // {
        //     Type type = typeof (K);
        //     if (this.components != null && this.components.ContainsKey(type))
        //     {
        //         throw new Exception($"entity already has component: {type.FullName}");
        //     }
        //
        //     Entity component = Create(type, isFromPool);
        //     component.Id = this.Id;
        //     component.ComponentParent = this;
        //     EventSystem.Instance.Awake(component, p1, p2);
        //     
        //     if (this is IAddComponent)
        //     {
        //         EventSystem.Instance.AddComponent(this, component);
        //     }
        //     return component as K;
        // }
        //
        // public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3>, new()
        // {
        //     Type type = typeof (K);
        //     if (this.components != null && this.components.ContainsKey(type))
        //     {
        //         throw new Exception($"entity already has component: {type.FullName}");
        //     }
        //
        //     Entity component = Create(type, isFromPool);
        //     component.Id = this.Id;
        //     component.ComponentParent = this;
        //     EventSystem.Instance.Awake(component, p1, p2, p3);
        //     
        //     if (this is IAddComponent)
        //     {
        //         EventSystem.Instance.AddComponent(this, component);
        //     }
        //     return component as K;
        // }
        
        // -----------------------------------------------------------
        // 内部复用逻辑 
        // -----------------------------------------------------------

        public void RemoveComponent<T>() where T : Component
        {
            Components.Remove(typeof(T));
        }

        public bool HasComponent<T>() where T : Component
        {
            return Components.TryGetValue(typeof(T), out _);
        }
        
        // ========================== 挂载新建实体 ==========================
        
        public Entity AddChild(Entity entity)
        {
            entity.Parent = this;
            entity.Awake();
            
            RegisterUpdate(entity);
            
            return entity;
        }

        public T AddChild<T>(bool isFromPool = false) where T : Entity
        {
            Type type = typeof (T);
            var entity = Entity.Create(type, false);
            entity.Parent = this;

            //EventSystem.Instance.Awake(component);
            entity.Awake();

            RegisterUpdate(entity);
            
            return entity as T;
        }

        public T AddChild<T, TA>(TA a, bool isFromPool = false) where T : Entity
        {
            Type type = typeof (T);
            var entity = Entity.Create(type, false);
            entity.Parent = this;

            //EventSystem.Instance.Awake(component, a);
            entity.Awake(a);
            
            RegisterUpdate(entity);
            
            return entity as T;
        }
        
        public T AddChildWithId<T>(long id, bool isFromPool = false) where T : Entity
        {
            Type type = typeof (T);
            T entity = Entity.Create(type, isFromPool) as T;
            entity.InstanceId = id;
            entity.Parent = this;
            //EventSystem.Instance.Awake(component);
            entity.Awake();
            
            RegisterUpdate(entity);
            
            return entity;
        }
        
        public T GetChild<T>(long id) where T: Entity
        {
            if (this._children == null)
            {
                return null;
            }
            this._children.TryGetValue(id, out Entity child);
            return child as T;
        }
        
        public void RemoveChild(long id)
        {
            if (this._children == null)
            {
                return;
            }

            if (!this._children.TryGetValue(id, out Entity child))
            {
                return;
            }
            
            this._children.Remove(id);
            child.Dispose();
        }
        
        // ========================== 释放实体 ==========================
        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            InstanceId = 0;

            // 清理Children
            if (_children != null)
            {
                foreach (Entity child in this._children.Values)
                {
                    child.Dispose();
                }

                this._children.Clear();
                ObjectPool.Instance.Recycle(this._children);
                this._children = null;
            }
            
            // 清理Component
            if (this._components != null)
            {
                foreach (KeyValuePair<Type, Component> kv in this._components)
                {
                    kv.Value.Dispose();
                }

                this._components.Clear();
                ObjectPool.Instance.Recycle(this._components);
                this._components = null;
            }
            
            if (this._parent != null && !this._parent.IsDisposed)
            {
                this._parent.RemoveFromChildren(this);
            }

            this._parent = null;

            base.Dispose();
            
            if (this.IsFromPool)
            {
                ObjectPool.Instance.Recycle(this);
            }
            _status = EntityStatus.None;
            
            // // 触发Destroy事件
            // if (this is IDestroy)
            // {
            //     EventSystem.Instance.Destroy(this);
            // }

            //if (EnableLog) Log.Debug($"{GetType().Name}->Dispose");
            // 清理自身的子实体
            // if (Children.Count > 0)
            // {
            //     for (int i = Children.Count - 1; i >= 0; i--)
            //     {
            //         EGamePlay.Entity.Destroy(Children[i]);
            //     }
            //     Children.Clear();
            //     Type2Children.Clear();
            // }
            // // 从父实体中删掉自己
            // Parent?.RemoveChild(this);
            // // 清理自身组件
            // foreach (var component in Components.Values)
            // {
            //     component.Enable = false;
            //     Component.Destroy(component);
            // }
            // Components.Clear();
            //
            // // ID归零
            // InstanceId = 0;
            // parent = null; 
            //
            // if (IsFromPool)
            // {
            //     EntityObjectPool.Instance.Recycle(this);
            // }
            //
            // EGamePlay.Entity.Master.RemoveEntity(this);
        }
    }
}