using System;

namespace CombatCore.Core.Singleton
{
    public interface ISingleton: IDisposable
    {
        void Register();
        void Destroy();
        bool IsDisposed();
    }
    
    public abstract class Singleton<T>: ISingleton where T: Singleton<T>, new()
    {
        private bool _isDisposed;
        
        private static T _instance;

        public static T Instance
        {
            get
            {
                return _instance;
            }
        }

        void ISingleton.Register()
        {
            if (_instance != null)
            {
                throw new Exception($"singleton register twice! {typeof (T).Name}");
            }
            _instance = (T)this;
        }

        void ISingleton.Destroy()
        {
            if (this._isDisposed)
            {
                return;
            }
            this._isDisposed = true;
            
            _instance.Dispose();
            _instance = null;
        }

        bool ISingleton.IsDisposed()
        {
            return this._isDisposed;
        }

        public virtual void Dispose()
        {
        }
    }
}