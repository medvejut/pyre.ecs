using UnityEngine;

namespace Pyre.Audio
{
    /// <summary>
    /// Один звук игры: клипы и то, как их проигрывать. Все клипы набора звучат вместе.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundClipSet", menuName = "Pyre/Sound Clip Set")]
    public class SoundClipSet : ScriptableObject
    {
        public AudioClip[] clips;

        [Range(0f, 1f)] public float spatialBlend;
    }
}
