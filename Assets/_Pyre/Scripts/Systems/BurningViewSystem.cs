using Pyre.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace Pyre.Systems
{
    public partial struct BurningViewSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (burningView, entity) in SystemAPI
                         .Query<RefRO<BurningView>>()
                         .WithEntityAccess())
            {
                var isBurning = SystemAPI.HasComponent<Burning>(entity);
                var isRenderEnabled = !SystemAPI.HasComponent<DisableRendering>(burningView.ValueRO.FireEntity);

                if (isBurning != isRenderEnabled)
                {
                    if (isBurning)
                    {
                        ecb.RemoveComponent<DisableRendering>(burningView.ValueRO.FireEntity);
                    }
                    else
                    {
                        ecb.AddComponent<DisableRendering>(burningView.ValueRO.FireEntity);
                    }
                }
            }
        }
    }
}