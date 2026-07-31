using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    public struct Burning : IComponentData
    {
        public float HeatRadius;
        public bool CanSpreadHeat => HeatRadius > 0;
    }
}