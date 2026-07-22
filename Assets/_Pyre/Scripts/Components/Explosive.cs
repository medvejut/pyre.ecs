using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Components
{
    public struct Explosive : IComponentData
    {
        public bool ExplodeOnStartBurn;
        public float Delay;

        public float ExplosionRadius;
        public float ExplosionImpulse;
        public float3 ExplosionOffset;

        public float CustomExplosionAngularImpulseMultiplier;
        public uint CustomExplosionAngularImpulseRandomSeed;
    }
}