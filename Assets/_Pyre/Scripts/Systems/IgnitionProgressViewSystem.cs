using Pyre.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace Pyre.Systems
{
    public partial struct IgnitionProgressViewSystem : ISystem
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

            foreach (var (view, ignitable, ignitionProgress, entity) in SystemAPI
                         .Query<RefRO<IgnitionProgressView>, RefRO<Ignitable>, RefRO<IgnitionProgress>>()
                         .WithEntityAccess())
            {
                var shouldRender = ignitionProgress.ValueRO.Elapsed > 0 && !SystemAPI.HasComponent<Burning>(entity);
                var isRenderEnabled = !SystemAPI.HasComponent<DisableRendering>(view.ValueRO.ProgressEntity);

                if (shouldRender != isRenderEnabled)
                {
                    if (shouldRender)
                    {
                        ecb.RemoveComponent<DisableRendering>(view.ValueRO.ProgressEntity);
                    }
                    else
                    {
                        ecb.AddComponent<DisableRendering>(view.ValueRO.ProgressEntity);
                    }
                }

                if (SystemAPI.TryGetComponent<ProgressMaterialProperty>(view.ValueRO.ProgressEntity, out var progressMaterialProperty))
                {
                    progressMaterialProperty.Value = ignitionProgress.ValueRO.Elapsed / ignitable.ValueRO.IgnitionTime;
                    SystemAPI.SetComponent(view.ValueRO.ProgressEntity, progressMaterialProperty);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}