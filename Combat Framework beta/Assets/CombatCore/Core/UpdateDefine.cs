namespace CombatCore.Core
{
    public interface IUpdate
    {
        void Update(float deltaTime);
    }

    public interface IFixedUpdate
    {
        void FixedUpdate(float fixedDeltaTime);
    }
}