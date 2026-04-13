using CombatCore.Core.Object;

namespace CombatCore.Core
{
    /// <summary>
    /// 组件：代表某个实体的扩展功能或数据承载。
    /// 遵循极简原则：不持有多余引用，不默认参与 Update 循环。
    /// </summary>
    public class Component : DisposeObject
    {
        /// <summary>
        /// 组件的父实体 (挂载该组件的容器)
        /// </summary>
        public Entity Parent { get; set; }
        
        public override bool IsDisposed { get; set; }

        public bool IsFromPool { get; set; }
        
        private bool _enable = false;
        
        public bool Enable
        {
            set
            {
                if (_enable == value) return;
                _enable = value;
                if (_enable) OnEnable();
                else OnDisable();
            }
            get => _enable;
        }
        
        public bool Disable => _enable == false;

        public T GetParent<T>() where T : Entity
        {
            return Parent as T;
        }

        // ==========================================
        // 生命周期方法 (Lifecycle Methods)
        // ==========================================
        
        public virtual void Awake() { }
        
        public virtual void Awake<T>(T a) { }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        public virtual void OnDestroy() { }

        // ==========================================
        // 内存回收逻辑
        // ==========================================
        
        public override void Dispose()
        {
            if (IsDisposed) return;
            
            Enable = false;
            OnDestroy();
            
            Parent = null;
            
            IsDisposed = true;
            
            if (IsFromPool)
            {
                ComponentObjectPool.Instance.Recycle(this);
            }
        }
    }
}