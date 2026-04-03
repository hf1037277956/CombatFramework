using System;
using System.Collections.Generic;

namespace EGamePlay
{
    /// <summary>
    /// 组件：代表某个实体的扩展功能，而不是某种规则
    /// 例如一个技能它有自己的伤害数值类型，但“移动”作为一个功能
    /// 有些技能可能不需要“移动”那么移动则可以抽象为一个组件为需要的技能添加AddComponent
    /// </summary>
    public class Component
    {
        // 和组件相关的实体
        /// <summary>
        /// 组件的父实体
        /// </summary>
        public CombatCore.Core.Entity Parent { get; set; }
        
        public bool IsDisposed { get; set; }

        public bool IsFromPool { get; set; }
        
        // 实体的子实体列表
        public Dictionary<long, CombatCore.Core.Entity> Id2Children { get; private set; } = new Dictionary<long, CombatCore.Core.Entity>();
        
        /// <summary>
        /// 定义组件初始化时是否默认启用 
        /// </summary>
        public virtual bool DefaultEnable { get; set; } = true;
        
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

        public virtual void Awake(object initData)
        {

        }

        public virtual void Setup()
        {

        }

        public virtual void Setup(object initData)
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

        private void Dispose()
        {
            //if (Entity.EnableLog) Log.Debug($"{GetType().Name}->Dispose");
            Enable = false;
            IsDisposed = true;

            if (IsFromPool)
            {
                ComponentObjectPool.Instance.Recycle(this);
            }
        }

        public static void Destroy(Component entity)
        {
            try
            {
                entity.OnDestroy();
            }
            catch (Exception e)
            {
                //Log.Error(e);
            }
            entity.Dispose();
        }

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