using Pyre.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace Pyre.Systems
{
    public partial struct ExplodeTimerViewSystem : ISystem
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

            foreach (var view in
                     SystemAPI.Query<RefRO<ExplodeTimerView>>()
                         .WithNone<ExplodeTimer>())
            {
                if (!SystemAPI.HasComponent<DisableRendering>(view.ValueRO.ProgressEntity))
                {
                    ecb.AddComponent<DisableRendering>(view.ValueRO.ProgressEntity);
                }
            }

            foreach (var (view, timer, explosive) in
                     SystemAPI.Query<RefRO<ExplodeTimerView>, RefRO<ExplodeTimer>, RefRO<Explosive>>())
            {
                if (SystemAPI.HasComponent<DisableRendering>(view.ValueRO.ProgressEntity))
                {
                    ecb.RemoveComponent<DisableRendering>(view.ValueRO.ProgressEntity);
                }

                if (SystemAPI.TryGetComponent<ProgressMaterialProperty>(view.ValueRO.ProgressEntity, out var progressMaterialProperty))
                {
                    progressMaterialProperty.Value = timer.ValueRO.TimeRemaining / explosive.ValueRO.Delay;
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