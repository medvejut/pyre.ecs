using Pyre.Animations.Components;
using Pyre.Audio;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public struct Explosive : IComponentData
    {
        public bool ExplodeOnStartBurn;
        public float Delay;

        public bool PlayWarningPulse;
        public PulseAnimation WarningPulse;
        public bool PlayWarningBlink;
        public BlinkAnimation WarningBlink;

        public float ExplosionRadius;
        public float ExplosionImpulse;
        public float3 ExplosionOffset;

        public float CustomExplosionAngularImpulseMultiplier;
        public uint CustomExplosionAngularImpulseRandomSeed;

        public UnityObjectRef<SoundClipSet> ExplosionSound;
        public Entity TickAudioSourceEntity;

        public UnityObjectRef<ParticleSystem> ExplosionVfx;
    }
}