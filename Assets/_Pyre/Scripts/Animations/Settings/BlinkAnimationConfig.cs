using Pyre.Animations.Components;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Animations.Settings
{
    [CreateAssetMenu(fileName = "BlinkAnimationConfig", menuName = "Pyre/Animations/Blink Animation Config")]
    public class BlinkAnimationConfig : ScriptableObject
    {
        [Tooltip("The color the target rests at while nothing is blinking it.")]
        public Color initialColor = new(1f, 1f, 1f, 0f);

        public Color startColor = Color.white;
        public Color endColor = Color.red;

        [Range(0f, 1f)] public float minOpacity = 0.2f;
        [Range(0f, 1f)] public float maxOpacity = 1f;

        public float baseFrequency = 1f;
        public float maxFrequency = 6f;

        public float4 InitialColor => ToFloat4(initialColor);

        public BlinkAnimation ToAnimation() => new()
        {
            StartColor = ToFloat4(startColor),
            EndColor = ToFloat4(endColor),
            MinOpacity = minOpacity,
            MaxOpacity = maxOpacity,
            BaseFrequency = baseFrequency,
            MaxFrequency = maxFrequency,
        };

        private static float4 ToFloat4(Color color) => new(color.r, color.g, color.b, color.a);
    }
}
