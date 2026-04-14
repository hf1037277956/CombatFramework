using System.Collections.Generic;
using CombatCore.Core.Singleton;


namespace CombatCore.Core
{
    public class UpdateManager : Singleton<UpdateManager>
    {
        private readonly List<IUpdate> _updateEntityList = new(100);

        private readonly HashSet<IUpdate> _entityUpdateSet = new HashSet<IUpdate>();



        private readonly List<IUpdate> _updateComponentList = new(100);

        private readonly HashSet<IUpdate> _componentUpdateSet = new HashSet<IUpdate>();


        public void Update(float deltaTime)
        {
            UpdateEntity(deltaTime);
            UpdateComponent(deltaTime);
        }

        public void RegisterEntityUpdate(Entity entity)
        {
            if (entity is not IUpdate update) return;

            if (!_entityUpdateSet.Add(update)) return;

            _updateEntityList.Add(update);
        }

        public void RegisterComponentUpdate(Component component)
        {
            if (component is not IUpdate update) return;

            if (!_componentUpdateSet.Add(update)) return;

            _updateComponentList.Add(update);
        }

        public void UnregisterUpdate(Entity entity)
        {
            if (entity is not IUpdate update) return;

            if (!_entityUpdateSet.Remove(update)) return;

            _updateEntityList.Remove(update);
        }

        public void UnregisterUpdate(Component component)
        {
            if (component is not IUpdate update) return;

            if (!_componentUpdateSet.Remove(update)) return;

            _updateComponentList.Remove(update);
        }

        private void UpdateEntity(float deltaTime)
        {
            for (int i = 0; i < _updateEntityList.Count; i++)
            {
                var item = _updateEntityList[i];

                if (((Entity)item).IsDisposed)
                {
                    _entityUpdateSet.Remove(item);

                    int lastIndex = _updateEntityList.Count - 1;

                    _updateEntityList[i] = _updateEntityList[lastIndex];

                    _updateEntityList.RemoveAt(lastIndex);

                    i--;

                    continue;
                }

                item.Update(deltaTime);
            }
        }

        private void UpdateComponent(float deltaTime)
        {
            for (int i = 0; i < _updateComponentList.Count; i++)
            {
                var item = _updateComponentList[i];

                if (((Component)item).IsDisposed)
                {
                    _componentUpdateSet.Remove(item);

                    int lastIndex = _updateComponentList.Count - 1;

                    _updateComponentList[i] = _updateComponentList[lastIndex];

                    _updateComponentList.RemoveAt(lastIndex);

                    i--;

                    continue;
                }

                if (!((Component)item).Enable) continue;

                item.Update(deltaTime);
            }
        }
    }
}


