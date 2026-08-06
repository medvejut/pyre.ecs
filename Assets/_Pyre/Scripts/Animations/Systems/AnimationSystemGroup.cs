using Unity.Entities;
using Unity.Transforms;

namespace Pyre.Animations.Systems
{
    // Runs before transforms so scale written this frame is rendered this frame.
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial class AnimationSystemGroup : ComponentSystemGroup
    {
    }
}
