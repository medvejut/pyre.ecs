using Pyre.Animations.Components;
using Unity.Burst;
using Unity.Entities;

namespace Pyre.Animations.Systems
{
    // Instances outlive their target when it is destroyed mid-animation, so reap them
    // before the channel systems try to write through a dangling Target.
    [UpdateInGroup(typeof(AnimationSystemGroup), OrderFirst = true)]
    public partial struct AnimationInstanceCleanupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<AnimationInstance>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (instance, entity) in
                     SystemAPI.Query<RefRO<AnimationInstance>>().WithEntityAccess())
            {
                if (!SystemAPI.Exists(instance.ValueRO.Target))
                {
                    ecb.DestroyEntity(entity);
                }
            }
        }
    }
}
