using Pyre.Audio;
using UnityEngine;

namespace Pyre.Settings
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Pyre/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Yaw that maps raw movement input onto world axes. Match the camera rig's Y rotation for screen-relative movement.")]
        [Range(-180f, 180f)] public float inputYaw = 45f;

        [Header("Knockback")]
        public float knockbackLinearDamping = 3f;
        public float knockbackAngularDamping = 5f;

        [Header("Audio")]
        public SoundClipSet extinguishSound;
    }
}