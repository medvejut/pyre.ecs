using Unity.Entities;

namespace Pyre.Components
{
    public struct Burning : IComponentData
    {
        public float HeatRadius;
        public bool CanSpreadHeat => HeatRadius > 0;
    }
}