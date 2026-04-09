namespace CombatCore.Core.Object
{
    public abstract class DisposeObject
    {
        public virtual bool IsDisposed { get; set; }
        
        public virtual void Dispose()
        {
        }
    }
}