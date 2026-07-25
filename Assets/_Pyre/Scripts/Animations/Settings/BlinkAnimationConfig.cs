using UnityEngine;

namespace Pyre.Animations.Settings
{
    [CreateAssetMenu(fileName = "BlinkAnimationConfig", menuName = "Pyre/Animations/Blink Animation Config")]
    public class BlinkAnimationConfig : ScriptableObject
    {
        public Color startColor = Color.white;
        public Color endColor = Color.red;

        [Range(0f, 1f)] public float minOpacity = 0.2f;
        [Range(0f, 1f)] public float maxOpacity = 1f;

        public float baseFrequency = 1f;
        public float maxFrequency = 6f;
    }
}