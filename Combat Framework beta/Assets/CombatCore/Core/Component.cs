using System;
using System.Collections.Generic;
using CombatCore.Core.Object;

namespace CombatCore.Core
{
    /// <summary>
    /// 组件：代表某个实体的扩展功能，而不是某种规则
    /// 例如一个技能它有自己的伤害数值类型，但“移动”作为一个功能
    /// 有些技能可能不需要“移动”那么移动则可以抽象为一个组件为需要的技能添加AddComponent
    /// </summary>
    public class Component : DisposeObject
    {
        /// <summary>
        /// 组件的父实体
        /// </summary>
        public Entity Parent { get; set; }

        public override bool IsDisposed => base.IsDisposed;

        public bool IsFromPool { get; set; }
        
        // 实体的子实体列表
        public Dictionary<long, CombatCore.Core.Entity> Id2Children { get; private set; } = new Dictionary<long, CombatCore.Core.Entity>();
        
        private bool enable = false;
        
        public bool Enable
        {
            set
            {
                if (enable == value) return;
                enable = value;
                if (enable) OnEnable();
                else OnDisable();
            }
            get
            {
                return enable;
            }
        }
        
        public bool Disable => enable == false;

        public T GetParent<T>() where T : CombatCore.Core.Entity
        {
            return Parent as T;
        }

        public virtual void Awake()
        {

        }
        
        public virtual void Awake<T>(T a) 
        {
            
        }

        public virtual void OnEnable()
        {

        }

        public virtual void OnDisable()
        {

        }

        public virtual void Update(float deltaTime)
        {
        }

        public virtual void OnDestroy()
        {
            
        }

        public override void Dispose()
        {
            Enable = false;
            IsDisposed = true;

            if (IsFromPool)
            {
                ComponentObjectPool.Instance.Recycle(this);
            }
        }

        // public static void Destroy(Component entity)
        // {
        //     try
        //     {
        //         entity.OnDestroy();
        //     }
        //     catch (Exception e)
        //     {
        //         //Log.Error(e);
        //     }
        //     entity.Dispose();
        // }

        // public T Publish<T>(T TEvent) where T : class
        // {
        //     Parent.Publish(TEvent);
        //     return TEvent;
        // }
        //
        // public void Subscribe<T>(Action<T> action) where T : class
        // {
        //     Parent.Subscribe(action);
        // }
        //
        // public void UnSubscribe<T>(Action<T> action) where T : class
        // {
        //     Parent.UnSubscribe(action);
        // }
    }
}