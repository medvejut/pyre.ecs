using Unity.Entities;

namespace Pyre.Animations.Components
{
    // Parameters of a pulse, sitting on an AnimationInstance entity.
    public struct PulseAnimation : IComponentData
    {
        public float MinScale;
        public float MaxScale;
        public float BaseFrequency;
        public float MaxFrequency;
    }
}
