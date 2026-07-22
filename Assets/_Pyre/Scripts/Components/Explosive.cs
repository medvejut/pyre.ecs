using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Components
{
    public struct Explosive : IComponentData
    {
        public bool ExplodeOnStartBurn;
        public float Delay;

        public float ExplosionRadius;

        public float3 CustomExplosionImpulse;
        public float CustomExplosionAngularImpulseMultiplier;
        public uint CustomExplosionAngularImpulseRandomSeed;
    }
}