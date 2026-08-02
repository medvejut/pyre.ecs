using Pyre.Animations.Components;
using Unity.Burst;
using Unity.Entities;

namespace Pyre.Animations.Systems
{
    [UpdateInGroup(typeof(AnimationActivationGroup), OrderLast = true)]
    public partial struct ClearAnimationEventsSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<PlayAnimationEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            SystemAPI.GetSingletonBuffer<PlayAnimationEvent>().Clear();
        }
    }
}
