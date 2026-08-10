using Unity.Entities;

namespace Pyre.Animations.Components
{
    public struct AnimationInstance : IComponentData
    {
        public Entity Target;
        public float Duration;
        public float Elapsed;
    }
}