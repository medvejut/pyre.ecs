using Unity.Entities;

namespace Pyre.Animations.Components
{
    public struct PulseAnimationSource : IComponentData
    {
        public float MinScale;
        public float MaxScale;
        public float BaseFrequency;
        public float MaxFrequency;
    }
}
