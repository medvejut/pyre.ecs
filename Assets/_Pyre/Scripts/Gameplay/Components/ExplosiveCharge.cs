using Pyre.Audio;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
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