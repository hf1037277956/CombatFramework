using CombatCore.Core;
using Unity.Mathematics;

namespace CombatCore.Combat
{
    public class CombatEntity : Entity , IPosition
    {
        public CombatEntityType CombatEntityType { get; set; }

        public float3 Position { get; set; }
    }
}