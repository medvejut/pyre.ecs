using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Audio.Components
{
    public struct SoundEvent : IBufferElementData
    {
        public float3 Position;
        public UnityObjectRef<AudioClip> Clip;
        public float SpatialBlend;
    }
}