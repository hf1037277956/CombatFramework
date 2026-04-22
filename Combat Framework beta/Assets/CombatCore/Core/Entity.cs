using System;
using System.Collections.Generic;
using CombatCore.Core.Object;


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

        public override bool IsDisposed => InstanceId == 0;
        
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
        
        // public Component GetComponent(Type type) 
        // {
        //     if (this._components == null)
        //     {
        //         return null;
        //     }
        //
        //     if (!this._components.TryGetValue(type, out var component))
        //     {
        //         return null;
        //     }
        //
        //     return component;
        // }
        
        // ========================== 创建实体 ==========================
        
        private static T Create<T>(bool isFromPool) where T : Entity, new()
        {
            T entity;
            if (isFromPool)
            {
                entity = EntityObjectPool.Instance.Fetch<T>();
            }
            else
            {
                entity = new T(); 
            }
            
            entity.InstanceId = IdFactory.NewInstanceId();
            entity.IsFromPool = isFromPool;
            entity.IsCreated = true;
            entity.IsNew = true;
            
            return entity;
        }
        
        // private static Entity Create(Type type, bool isFromPool)
        // {
        //     Entity entity;
        //     if (isFromPool)
        //     {
        //         entity = EntityObjectPool.Instance.Fetch(type);
        //     }
        //     else
        //     {
        //         entity = Activator.CreateInstance(type) as Entity; 
        //     }
        //     
        //     entity.InstanceId = IdFactory.NewInstanceId();
        //     entity.IsFromPool = isFromPool;
        //     entity.IsCreated = true;
        //     entity.IsNew = true;
        //     
        //     return entity;
        // }
        
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
            
            component.Awake();

            EntityCoreNode.Instance.RegisterUpdate(component);
            
            return component;
        }
        
        public T AddComponent<T>(bool isFromPool = true) where T : Component, new()
        {
            Type type = typeof(T);
            if (this._components != null && this._components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            T component; 
            if (isFromPool)
            {
                component = ComponentObjectPool.Instance.Fetch<T>();
                component.IsFromPool = true;
            }
            else
            {
                component = new T
                {
                    IsFromPool = false
                };
            }

            component.Parent = this;
            component.IsDisposed = false;
            Components.Add(type, component); 
            
            component.Awake();

            EntityCoreNode.Instance.RegisterUpdate(component);
            
            return component;
        }
        
        public T AddComponent<T, TP1>(TP1 p1, bool isFromPool = true) where T : Component, new()
        {
            Type type = typeof(T);
            if (this._components != null && this._components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            T component;
            if (isFromPool)
            {
                component = ComponentObjectPool.Instance.Fetch<T>();
                component.IsFromPool = true;
            }
            else
            {
                component = new T();
                component.IsFromPool = false; 
            }

            component.Parent = this;
            component.IsDisposed = false;
            Components.Add(type, component);
            
            component.Awake(p1);
            
            EntityCoreNode.Instance.RegisterUpdate(component);
            
            return component;
        }

        // public K AddComponent<K, P1, P2>(P1 p1, P2 p2, bool isFromPool = true) where K : Entity, IAwake<P1, P2>, new()
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
        // public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = true) where K : Entity, IAwake<P1, P2, P3>, new()
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

        public bool RemoveComponent<T>() where T : Component
        {
            if (this._components == null) return false;
            if (!this._components.TryGetValue(typeof(T), out Component component)) return false;

            this._components.Remove(typeof(T));
            
            component.Dispose();

            if (this._components.Count == 0)
            {
                ObjectPool.Instance.Recycle(this._components);
                this._components = null;
            }

            return true;
        }

        public bool HasComponent<T>() where T : Component
        {
            return this._components != null && this._components.TryGetValue(typeof(T), out _);
        }
        
        // ========================== 挂载新建实体 ==========================

        public T AddChild<T>(bool isFromPool = true) where T : Entity, new()
        {
            // 路由到泛型的 Create<T> 高速通道，并正确传递 isFromPool
            var entity = Entity.Create<T>(isFromPool);
            entity.Parent = this;

            entity.Awake();

            EntityCoreNode.Instance.AddEntity(entity);
            
            return entity;
        }

        public T AddChild<T, TA>(TA a, bool isFromPool = true) where T : Entity, new()
        {
            var entity = Entity.Create<T>(isFromPool);
            entity.Parent = this;

            entity.Awake(a);

            EntityCoreNode.Instance.AddEntity(entity);
            
            return entity;
        }
        
        public T AddChildWithId<T>(long id, bool isFromPool = true) where T : Entity, new()
        {
            var entity = Entity.Create<T>(isFromPool);
            entity.InstanceId = id;
            entity.Parent = this;

            entity.Awake();

            EntityCoreNode.Instance.AddEntity(entity);
            
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

            EntityCoreNode.Instance.RemoveEntity(this);

            InstanceId = 0;

            // 清理Children
            if (_children != null)
            {
                foreach (Entity child in this._children.Values)
                {
                    child.Dispose();
                }

                this._children.Clear();
                // 回收子实体字典对象
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
                // 回收组件字典对象
                ObjectPool.Instance.Recycle(this._components);
                this._components = null;
            }
            
            if (this._parent != null && !this._parent.IsDisposed)
            {
                this._parent.RemoveFromChildren(this);
            }

            this._parent = null;

            if (this.IsFromPool)
            {
                EntityObjectPool.Instance.Recycle(this);
            }

            _status = EntityStatus.None;
        }
    }
}