using Unity.Entities;
using UnityEngine;

namespace Pyre.Effects.Components
{
    public struct PlayParticlesEvent : IBufferElementData
    {
        public UnityObjectRef<ParticleSystem> ParticleSystem;
        public Vector3 Position;
    }
}