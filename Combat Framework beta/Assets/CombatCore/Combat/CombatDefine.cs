using Unity.Mathematics;

namespace CombatCore.Combat
{
    public interface IPosition
    {
        float3 Position { get; set; }
    }

    public interface IActionAbility
    {
        public CombatEntity OwnerEntity { get; set; }

        public bool Enable { get; set; }
    }

    public enum CombatEntityType
    {
        Hero,
        Monster,
        EliteMonster,
        Boss,
    }
}