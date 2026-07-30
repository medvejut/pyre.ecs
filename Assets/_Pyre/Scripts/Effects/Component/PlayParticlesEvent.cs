using Unity.Entities;
using UnityEngine;

namespace Pyre.Effects.Component
{
    public struct PlayParticlesEvent : IBufferElementData
    {
        public UnityObjectRef<ParticleSystem> ParticleSystem;
        public Vector3 Position;
    }
}