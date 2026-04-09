using System.Collections.Generic;
using CombatCore.Core.Singleton;

namespace CombatCore.Core
{
    public class UpdateManager : Singleton<UpdateManager>
    {
        private readonly List<IUpdate> _updateEntityList = new(100);
        private readonly List<IUpdate> _updateComponentList = new(100);
        
        public void RegisterEntityUpdate(IUpdate update)
        {
            if (!_updateEntityList.Contains(update))
            {
                _updateEntityList.Add(update);
            }
        }
        
        public void RegisterComponentUpdate(IUpdate update)
        {
            if (!_updateComponentList.Contains(update))
            {
                _updateComponentList.Add(update);
            }
        }
        
        public void UnRegister(IUpdate update)
        {
            _updateEntityList.Remove(update);
        }
        
        public void UpdateEntity(float deltaTime)
        {
            for (int i = 0; i < _updateEntityList.Count; i++)
            {
                var item = _updateEntityList[i];
                
                if (((Entity)item).IsDisposed) 
                {
                    int lastIndex = _updateEntityList.Count - 1;
                    _updateEntityList[i] = _updateEntityList[lastIndex];
                    _updateEntityList.RemoveAt(lastIndex);
                    i--;
                    continue;
                }

                item.Update(deltaTime);
            }
        }
        
        public void UpdateComponent(float deltaTime)
        {
            for (int i = 0; i < _updateEntityList.Count; i++)
            {
                var item = _updateEntityList[i];
                
                if (((Component)item).IsDisposed) 
                {
                    int lastIndex = _updateEntityList.Count - 1;
                    _updateEntityList[i] = _updateEntityList[lastIndex];
                    _updateEntityList.RemoveAt(lastIndex);
                    i--;
                    continue;
                }

                if (!((Component)item).Enable) continue;

                item.Update(deltaTime);
            }
        }
    }
}