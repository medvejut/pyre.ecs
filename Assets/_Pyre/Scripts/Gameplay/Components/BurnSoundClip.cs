using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    [InternalBufferCapacity(2)]
    public struct BurnSoundClip : IBufferElementData
    {
        public UnityObjectRef<AudioClip> Clip;
    }
}
