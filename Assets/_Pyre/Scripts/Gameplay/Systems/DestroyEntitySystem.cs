using Pyre.Gameplay.Components;
using Unity.Burst;
using Unity.Entities;

namespace Pyre.Gameplay.Systems
{
    public partial struct DestroyEntitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var burningView in SystemAPI
                         .Query<RefRO<BurningView>>()
                         .WithAll<DestroyRequested>())
            {
                ecb.DestroyEntity(burningView.ValueRO.FireEntity);
            }

            foreach (var ignitionProgressView in SystemAPI
                         .Query<RefRO<IgnitionProgressView>>()
                         .WithAll<DestroyRequested>())
            {
                ecb.DestroyEntity(ignitionProgressView.ValueRO.ProgressEntity);
            }

            foreach (var explodeTimerView in SystemAPI
                         .Query<RefRO<ExplodeTimerView>>()
                         .WithAll<DestroyRequested>())
            {
                ecb.DestroyEntity(explodeTimerView.ValueRO.ProgressEntity);
            }

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<DestroyRequested>>()
                         .WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}