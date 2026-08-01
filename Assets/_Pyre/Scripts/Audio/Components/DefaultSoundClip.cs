using Unity.Entities;
using UnityEngine;

namespace Pyre.Audio.Components
{
    /// <summary>
    /// Singleton buffer holding the fallback clip for every <see cref="SoundKind"/>.
    /// Dense: exactly one entry per kind, in enum order, indexed by (int)kind.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct DefaultSoundClip : IBufferElementData
    {
        public UnityObjectRef<AudioClip> Clip;
    }
}
