using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Animations.Components
{
    // Parameters of a blink, sitting on an AnimationInstance entity.
    public struct BlinkAnimation : IComponentData
    {
        public float4 StartColor;
        public float4 EndColor;
        public float MinOpacity;
        public float MaxOpacity;
        public float BaseFrequency;
        public float MaxFrequency;
    }
}
