using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Animations.Components
{
    // The authored blink color an animated entity returns to once nothing is animating it.
    public struct AnimationRestColor : IComponentData
    {
        public float4 Value;
    }
}
