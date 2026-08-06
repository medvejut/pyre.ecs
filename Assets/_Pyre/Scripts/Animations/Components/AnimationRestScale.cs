using Unity.Entities;

namespace Pyre.Animations.Components
{
    // The authored scale an animated entity returns to once nothing is animating it.
    public struct AnimationRestScale : IComponentData
    {
        public float Value;
    }
}
