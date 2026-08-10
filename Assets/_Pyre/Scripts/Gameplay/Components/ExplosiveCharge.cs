using Pyre.Audio;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    // The blast an explosive produces. Fields mirror Explosion, which this is a
    // template for - only Position and AngularImpulse are resolved at detonation.
    public struct ExplosiveCharge : IComponentData
    {
        public float3 Offset;
        public float Radius;
        public float Impulse;

        public float AngularImpulseMultiplier;
        public uint AngularImpulseSeed;

        public UnityObjectRef<SoundClipSet> Sound;
        public UnityObjectRef<ParticleSystem> Vfx;
    }
}
