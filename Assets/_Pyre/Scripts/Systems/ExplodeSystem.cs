using Pyre.Components;
using Unity.Burst;
using Unity.Entities;

namespace Pyre.Systems
{
    public partial struct ExplodeSystem : ISystem
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

            ProcessExplodeOnStartBurn(ref state, ecb);
            ExplodeAfterDelay(ref state, ecb);
        }

        private void ProcessExplodeOnStartBurn(ref SystemState state, EntityCommandBuffer ecb)
        {
            foreach (var (explosive, entity) in
                     SystemAPI.Query<RefRO<Explosive>>()
                         .WithNone<ExplodeTimer>()
                         .WithAll<Burning>()
                         .WithEntityAccess())
            {
                if (explosive.ValueRO.ExplodeOnStartBurn)
                {
                    ecb.AddComponent(entity, new ExplodeTimer { TimeRemaining = explosive.ValueRO.Delay });
                }
            }
        }

        private void ExplodeAfterDelay(ref SystemState state, EntityCommandBuffer ecb)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (explodeTimer, entity) in
                     SystemAPI.Query<RefRW<ExplodeTimer>>()
                         .WithEntityAccess())
            {
                explodeTimer.ValueRW.TimeRemaining -= deltaTime;

                if (explodeTimer.ValueRO.TimeRemaining <= 0f)
                {
                    ecb.AddComponent<DestroyRequested>(entity);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}