using System.Collections.Generic;
using CombatCore.Core.Singleton;

namespace CombatCore.Core
{
    public class UpdateManager : Singleton<UpdateManager>
    {
        private readonly List<IUpdate> _updateList = new(100);
        
        public void Register(IUpdate update)
        {
            if (!_updateList.Contains(update))
            {
                _updateList.Add(update);
            }
        }
        
        public void UnRegister(IUpdate update)
        {
            _updateList.Remove(update);
        }
        
        public void Update(float deltaTime)
        {
            for (int i = 0; i < _updateList.Count; i++)
            {
                var item = _updateList[i];
                
                if (((Component)item).IsDisposed) 
                {
                    int lastIndex = _updateList.Count - 1;
                    _updateList[i] = _updateList[lastIndex];
                    _updateList.RemoveAt(lastIndex);
                    i--;
                    continue;
                }

                if (!((Component)item).Enable) continue;

                item.Update(deltaTime);
            }
        }
    }
}