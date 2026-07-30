using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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

        public UnityObjectRef<AudioClip> ExplosionClip;
        public Entity TickAudioSourceEntity;

        public UnityObjectRef<ParticleSystem> ExplosionVfx;
    }
}