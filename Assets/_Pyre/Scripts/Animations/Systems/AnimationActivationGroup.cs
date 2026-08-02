using Unity.Entities;

namespace Pyre.Animations.Systems
{
    // Runs last so every producer has already pushed its PlayAnimationEvent this frame.
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial class AnimationActivationGroup : ComponentSystemGroup
    {
    }
}
