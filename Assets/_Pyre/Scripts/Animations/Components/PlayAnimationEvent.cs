using Unity.Entities;

namespace Pyre.Animations.Components
{
    public struct PlayAnimationEvent : IBufferElementData
    {
        public Entity Target;
        public float Duration;
    }
}
