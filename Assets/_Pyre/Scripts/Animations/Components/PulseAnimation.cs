using Unity.Entities;

namespace Pyre.Animations.Components
{
    public struct PulseAnimation : IComponentData
    {
        public float MinScale;
        public float MaxScale;
        public float BaseFrequency;
        public float MaxFrequency;
        public float TotalDuration;
        public float ElapsedTime;

        // Captured when the animation starts, restored when it finishes.
        public float ResetScale;
    }
}
