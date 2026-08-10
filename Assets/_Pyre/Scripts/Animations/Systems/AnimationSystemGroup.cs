using Unity.Entities;
using Unity.Transforms;

namespace Pyre.Animations.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial class AnimationSystemGroup : ComponentSystemGroup
    {
    }
}