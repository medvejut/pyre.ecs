using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public struct Explosion : IComponentData
    {
        public float3 Position;
        public float Radius;

        public float Impulse;
        public float3 AngularImpulse;

        public UnityObjectRef<AudioClip> Clip;
        public UnityObjectRef<ParticleSystem> Vfx;
    }
}