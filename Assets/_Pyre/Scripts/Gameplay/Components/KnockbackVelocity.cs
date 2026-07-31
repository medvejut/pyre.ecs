using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Gameplay.Components
{
    public struct KnockbackVelocity : IComponentData
    {
        public float3 Linear;
        public float3 Angular;
    }
}