using Pyre.Audio.Components;
using Pyre.Gameplay.Components;
using UnityEngine;

namespace Pyre.Settings
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Pyre/Game Config")]
    public class GameConfig : ScriptableObject
    {
        public MoveInputSettings moveInput = new() { Yaw = 45f };

        public KnockbackSettings knockback = new() { LinearDamping = 3f, AngularDamping = 5f };

        public SoundDefaults soundDefaults;
    }
}
