using Unity.Entities;

namespace Pyre.Animations.Components
{
    // Lives on its own entity, one per playing animation, so a target can run
    // several animations of the same kind at once.
    public struct AnimationInstance : IComponentData
    {
        public Entity Target;
        public float Duration;
        public float Elapsed;
    }
}
