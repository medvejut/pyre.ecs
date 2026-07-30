using UnityEngine;

namespace Pyre.Animations.Settings
{
    [CreateAssetMenu(fileName = "LightBlinkAnimationConfig", menuName = "Pyre/Animations/Light Blink Animation Config")]
    public class LightBlinkAnimationConfig : ScriptableObject
    {
        [Tooltip("Dimmest intensity, as a fraction of the intensity authored on the light.")]
        [Range(0f, 2f)] public float minIntensity = 0.75f;

        [Tooltip("Brightest intensity, as a fraction of the intensity authored on the light.")]
        [Range(0f, 2f)] public float maxIntensity = 1.15f;

        [Tooltip("Blinks per second.")]
        public float frequency = 2f;

        [Tooltip("0 - a steady sine pulse, 1 - a fully irregular flicker.")]
        [Range(0f, 1f)] public float irregularity = 0.8f;

        [Tooltip("Offsets the blink per instance, so lights sharing this config do not blink in sync.")]
        public bool randomizePhase = true;
    }
}
