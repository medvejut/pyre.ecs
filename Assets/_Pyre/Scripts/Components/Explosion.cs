using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Components
{
    public struct Explosion : IComponentData
    {
        public float3 Position;
        public float Radius;

        public float3 Impulse;
        public float3 AngularImpulse;
    }
}