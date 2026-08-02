using UnityEngine;

namespace Pyre.Audio
{
    [CreateAssetMenu(fileName = "SoundClipSet", menuName = "Pyre/Sound Clip Set")]
    public class SoundClipSet : ScriptableObject
    {
        public AudioClip[] clips;

        [Range(0f, 1f)] public float spatialBlend;
    }
}
