using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Audio.Components
{
    public struct SoundEvent : IBufferElementData
    {
        public float3 Position;
        public UnityObjectRef<SoundClipSet> Sound;
    }
}
