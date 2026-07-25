using UnityEngine;

namespace Pyre.Animations.Settings
{
    [CreateAssetMenu(fileName = "PulseAnimationConfig", menuName = "Pyre/Animations/Pulse Animation Config")]
    public class PulseAnimationConfig : ScriptableObject
    {
        public float minScale = 0.9f;
        public float maxScale = 1.1f;
        public float baseFrequency = 1f;
        public float maxFrequency = 4f;
    }
}