using Unity.Entities;
using UnityEngine;

namespace Pyre.Audio.Components
{
    [InternalBufferCapacity(4)]
    public struct SoundClipOverride : IBufferElementData
    {
        public SoundKind Kind;
        public UnityObjectRef<AudioClip> Clip;
    }
}
