using System;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Audio.Components
{
    /// <summary>
    /// Fallback clip per <see cref="SoundKind"/>, authored on the GameConfig asset and
    /// baked into the <see cref="DefaultSoundClip"/> singleton buffer.
    /// Add a field here whenever a kind is added to <see cref="SoundKind"/>.
    /// </summary>
    [Serializable]
    public struct SoundDefaults
    {
        public AudioClip BurnClip;
        public AudioClip BurningLoopClip;
        public AudioClip ExtinguishClip;
        public AudioClip ExplodeClip;

        /// <summary>
        /// Fills the buffer in <see cref="SoundKind"/> order — it is indexed by (int)kind.
        /// </summary>
        public void Populate(DynamicBuffer<DefaultSoundClip> buffer)
        {
            buffer.Add(new DefaultSoundClip { Clip = BurnClip });
            buffer.Add(new DefaultSoundClip { Clip = BurningLoopClip });
            buffer.Add(new DefaultSoundClip { Clip = ExtinguishClip });
            buffer.Add(new DefaultSoundClip { Clip = ExplodeClip });
        }
    }
}
