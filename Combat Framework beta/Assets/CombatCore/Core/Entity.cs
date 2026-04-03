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
        IsComponent = 1 << 1,
        IsCreated = 1 << 2,
        IsNew = 1 << 3,
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

        private bool IsComponent
        {
            get => (this._status & EntityStatus.IsComponent) == EntityStatus.IsComponent;
            set
            {
                if (value)
                {
                    this._status |= EntityStatus.IsComponent;
                }
                else
                {
                    this._status &= ~EntityStatus.IsComponent;
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
                this.IsComponent = false;
                this._parent.AddToChildren(this);
            }
        }
        
        private Entity ComponentParent
        {
            set
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
                    this._parent.RemoveFromComponents(this);
                }

                this._parent = value;
                this.IsComponent = true;
                this._parent.AddToComponents(this);
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
        
        private Dictionary<Type, Entity> _components;
        
        public Dictionary<Type, Entity> Components
        {
            get
            {
                return this._components ??= ObjectPool.Instance.Fetch<Dictionary<Type, Entity>>();
            }
        }

        // public virtual void Awake()
        // {
        //
        // }
        //
        // public virtual void Awake(object initData)
        // {
        //
        // }
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
                foreach (KeyValuePair<Type, Entity> kv in this._components)
                {
                    kv.Value.Dispose();
                }

                this._components.Clear();
                ObjectPool.Instance.Recycle(this._components);
                this._components = null;
            }
            
            if (this._parent != null && !this._parent.IsDisposed)
            {
                if (this.IsComponent)
                {
                    //this._parent.RemoveComponent(this);
                }
                else
                {
                    this._parent.RemoveFromChildren(this);
                }
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
        
        private void AddToComponents(Entity component)
        {
            this.Components.Add(component.GetType(), component);
        }

        private void RemoveFromComponents(Entity component)
        {
            if (this._components == null)
            {
                return;
            }

            this._components.Remove(component.GetType());

            if (this._components.Count == 0)
            {
                ObjectPool.Instance.Recycle(this._components);
                this._components = null;
            }
        }

        //#region 组件
        
        public T AddComponent<T>() where T : Component
        {
            // 强制新建
            var component = Activator.CreateInstance<T>();
            // 默认不是从池里来的 (如果 Component 类也有 IsFromPool 属性的话，建议显式设为 false)
            component.IsFromPool = false; 
            
            return SetupNewComponent(component);
        }

        public T AddComponent<T>(object initData) where T : Component
        {
            // 强制新建
            var component = Activator.CreateInstance<T>();
            component.IsFromPool = false;
            
            return SetupNewComponent(component, initData);
        }
        
        // -----------------------------------------------------------
        // 对象池版本 AddComponentFromPool
        // -----------------------------------------------------------

        public T AddComponentFromPool<T>() where T : Component
        {
            // 从对象池获取
            var component = ComponentObjectPool.Instance.Fetch(typeof(T)) as T;
            component.IsFromPool = true; // 标记来源

            return SetupNewComponent(component);
        }

        public T AddComponentFromPool<T>(object initData) where T : Component
        {
            // 从对象池获取
            var component = ComponentObjectPool.Instance.Fetch(typeof(T)) as T;
            component.IsFromPool = true; // 标记来源

            return SetupNewComponent(component, initData);
        }
        
        // -----------------------------------------------------------
        // 内部复用逻辑 
        // -----------------------------------------------------------
        
        private T SetupNewComponent<T>(T component) where T : Component
        {
            component.Parent = this;
            component.IsDisposed = false;
            //Components.Add(typeof(T), component);
            //EGamePlay.Entity.Master?.AllComponents.Add(component);
            
            //if (EnableLog) Log.Debug($"{GetType().Name}->AddComponent, {typeof(T).Name} FromPool={component.IsFromPool}");
            
            component.Awake();
            component.Setup();
#if UNITY_EDITOR
            //GetComponent<GameObjectComponent>()?.OnAddComponent(component);
#endif
            component.Enable = component.DefaultEnable;
            return component;
        }

        private T SetupNewComponent<T>(T component, object initData) where T : Component
        {
            component.Parent = this;
            component.IsDisposed = false;
            //Components.Add(typeof(T), component);
            //EGamePlay.Entity.Master?.AllComponents.Add(component);
            
            //if (EnableLog) Log.Debug($"{GetType().Name}->AddComponent, {typeof(T).Name} FromPool={component.IsFromPool}, initData={initData}");
            
            component.Awake(initData);
            component.Setup(initData);
#if UNITY_EDITOR
            //GetComponent<GameObjectComponent>()?.OnAddComponent(component);
#endif
            component.Enable = component.DefaultEnable;
            return component;
        }

        public void RemoveComponent<T>() where T : Component
        {
            var component = Components[typeof(T)];
            //if (component.Enable) component.Enable = false;
            //Component.Destroy(component);
            Components.Remove(typeof(T));
#if UNITY_EDITOR
           // GetComponent<GameObjectComponent>().OnRemoveComponent(component);
#endif
        }

//         public T GetComponent<T>() where T : Component
//         {
//             if (Components.TryGetValue(typeof(T), out var component))
//             {
//                 return component as T;
//             }
//             return null;
//         }
//
//         public bool HasComponent<T>() where T : Component
//         {
//             return Components.TryGetValue(typeof(T), out var component);
//         }
//
//         public Component GetComponent(Type componentType)
//         {
//             if (this.Components.TryGetValue(componentType, out var component))
//             {
//                 return component;
//             }
//             return null;
//         }
//         
//         public bool TryGet<T>(out T component) where T : Component
//         {
//             if (Components.TryGetValue(typeof(T), out var c))
//             {
//                 component = c as T;
//                 return true;
//             }
//             component = null;
//             return false;
//         }
//
//         public bool TryGet<T, T1>(out T component, out T1 component1) where T : Component  where T1 : Component
//         {
//             component = null;
//             component1 = null;
//             if (Components.TryGetValue(typeof(T), out var c)) component = c as T;
//             if (Components.TryGetValue(typeof(T1), out var c1)) component1 = c1 as T1;
//             if (component != null && component1 != null) return true;
//             return false;
//         }
//
//         public bool TryGet<T, T1, T2>(out T component, out T1 component1, out T2 component2) where T : Component where T1 : Component where T2 : Component
//         {
//             component = null;
//             component1 = null;
//             component2 = null;
//             if (Components.TryGetValue(typeof(T), out var c)) component = c as T;
//             if (Components.TryGetValue(typeof(T1), out var c1)) component1 = c1 as T1;
//             if (Components.TryGetValue(typeof(T2), out var c2)) component2 = c2 as T2;
//             if (component != null && component1 != null && component2 != null) return true;
//             return false;
//         }
//         
//         #endregion
//
//         #region 子实体
//         
//         private void SetParent(Entity parent)
//         {
//             var preParent = Parent;
//             preParent?.RemoveChild(this);
//             this.parent = parent;
// #if UNITY_EDITOR
//             //parent.GetComponent<GameObjectComponent>()?.OnAddChild(this);
// #endif
//             OnSetParent(preParent, parent);
//         }
//
//         public void SetChild(Entity child)
//         {
//             Children.Add(child);
//             Id2Children.Add(child.Id, child);
//             if (!Type2Children.ContainsKey(child.GetType())) Type2Children.Add(child.GetType(), new List<Entity>());
//             Type2Children[child.GetType()].Add(child);
//             child.SetParent(this);
//         }
//
//         public void RemoveChild(Entity child)
//         {
//             Children.Remove(child);
//             Id2Children.Remove(child.Id);
//             if (Type2Children.ContainsKey(child.GetType())) Type2Children[child.GetType()].Remove(child);
//         }
//         
//         // -----------------------------------------------------------
//         // 非对象池版本：AddChild 
//         // -----------------------------------------------------------
//
//         public Entity AddChild(Type entityType)
//         {
//             var entity = EGamePlay.Entity.NewEntity(entityType, false);
//             //if (EnableLog) Log.Debug($"AddChild {this.GetType().Name}, {entityType.Name}={entity.Id}");
//             SetupEntity(entity, this);
//             return entity;
//         }
//
//         public Entity AddChild(Type entityType, object initData)
//         {
//             var entity = EGamePlay.Entity.NewEntity(entityType, false);
//             //if (EnableLog) Log.Debug($"AddChild {this.GetType().Name}, {entityType.Name}={entity.Id}");
//             SetupEntity(entity, this, initData);
//             return entity;
//         }
//
//         public T AddChild<T>() where T : Entity
//         {
//             return AddChild(typeof(T)) as T;
//         }
//
//         public T AddChild<T>(object initData) where T : Entity
//         {
//             return AddChild(typeof(T), initData) as T;
//         }
//         
//         // -----------------------------------------------------------
//         // 对象池版本：AddChildFromPool
//         // -----------------------------------------------------------
//
//         public Entity AddChildFromPool(Type entityType)
//         {
//             var entity = EGamePlay.Entity.NewEntity(entityType, true); 
//             //if (EnableLog) Log.Debug($"AddChildFromPool {this.GetType().Name}, {entityType.Name}={entity.Id}");
//             SetupEntity(entity, this);
//             return entity;
//         }
//
//         public Entity AddChildFromPool(Type entityType, object initData)
//         {
//             var entity = EGamePlay.Entity.NewEntity(entityType, true);
//             //if (EnableLog) Log.Debug($"AddChildFromPool {this.GetType().Name}, {entityType.Name}={entity.Id}, {initData}");
//             SetupEntity(entity, this, initData);
//             return entity;
//         }
//
//         public T AddChildFromPool<T>() where T : Entity
//         {
//             return AddChildFromPool(typeof(T)) as T;
//         }
//
//         public T AddChildFromPool<T>(object initData) where T : Entity
//         {
//             return AddChildFromPool(typeof(T), initData) as T;
//         }
//         
//         public T AddIdChild<T>(long id) where T : Entity
//         {
//             var entityType = typeof(T);
//             var entity = EGamePlay.Entity.NewEntity(entityType, false);
//             //if (EnableLog) Log.Debug($"AddChild {this.GetType().Name}, {entityType.Name}={entity.Id}");
//             SetupEntity(entity, this);
//             return entity as T;
//         }
//
//         public Entity GetIdChild(long id)
//         {
//             Id2Children.TryGetValue(id, out var entity);
//             return entity;
//         }
//
//         public T GetIdChild<T>(long id) where T : Entity
//         {
//             Id2Children.TryGetValue(id, out var entity);
//             return entity as T;
//         }
//
//         public T GetChild<T>(int index = 0) where T : Entity
//         {
//             if (Type2Children.ContainsKey(typeof(T)) == false)
//             {
//                 return null;
//             }
//             if (Type2Children[typeof(T)].Count <= index)
//             {
//                 return null;
//             }
//             return Type2Children[typeof(T)][index] as T;
//         }
//
//         public Entity[] GetChildren()
//         {
//             return Children.ToArray();
//         }
//
//         public T[] GetTypeChildren<T>() where T : Entity
//         {
//             return Type2Children[typeof(T)].ConvertAll(x => x.As<T>()).ToArray();
//         }
//
//         public Entity Find(string name)
//         {
//             foreach (var item in Children)
//             {
//                 if (item.name == name) return item;
//             }
//             return null;
//         }
//
//         public T Find<T>(string name) where T : Entity
//         {
//             if (Type2Children.TryGetValue(typeof(T), out var chidren))
//             {
//                 foreach (var item in chidren)
//                 {
//                     if (item.name == name) return item as T;
//                 }
//             }
//             return null;
//         }
//         
//         #endregion
//
//         #region 事件
//         public T Publish<T>(T TEvent) where T : class
//         {
//             var eventComponent = GetComponent<EventComponent>();
//             if (eventComponent == null)
//             {
//                 return TEvent;
//             }
//             eventComponent.Publish(TEvent);
//             return TEvent;
//         }
//         
//         public void Subscribe<T>(Action<T> action) where T : class
//         {
//             var eventComponent = GetComponent<EventComponent>();
//             if (eventComponent == null)
//             {
//                 eventComponent = AddComponentFromPool<EventComponent>();
//             }
//             eventComponent.Subscribe(action);
//             //return eventComponent.Subscribe(action);
//         }
//
//         // public SubscribeSubject Subscribe<T>(Action<T> action, Entity disposeWith) where T : class
//         // {
//         //     var eventComponent = GetComponent<EventComponent>();
//         //     if (eventComponent == null)
//         //     {
//         //         eventComponent = AddComponent<EventComponent>();
//         //     }
//         //     return eventComponent.Subscribe(action).DisposeWith(disposeWith);
//         // }
//
//         public void UnSubscribe<T>(Action<T> action) where T : class
//         {
//             var eventComponent = GetComponent<EventComponent>();
//             if (eventComponent != null)
//             {
//                 eventComponent.UnSubscribe(action);
//             }
//         }
//
//         public void FireEvent(string eventType)
//         {
//             FireEvent(eventType, this);
//         }
//
//         public void FireEvent(string eventType, Entity entity)
//         {
//             var eventComponent = GetComponent<EventComponent>();
//             if (eventComponent != null)
//             {
//                 eventComponent.FireEvent(eventType, entity);
//             }
//         }
//
//         public void OnEvent(string eventType, Action<Entity> action)
//         {
//             var eventComponent = GetComponent<EventComponent>();
//             if (eventComponent == null)
//             {
//                 eventComponent = AddComponentFromPool<EventComponent>();
//             }
//             eventComponent.OnEvent(eventType, action);
//         }
//
//         public void OffEvent(string eventType, Action<Entity> action)
//         {
//             var eventComponent = GetComponent<EventComponent>();
//             if (eventComponent != null)
//             {
//                 eventComponent.OffEvent(eventType, action);
//             }
//         }
//         #endregion
    }
}