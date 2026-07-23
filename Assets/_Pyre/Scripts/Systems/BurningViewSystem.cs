using Pyre.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine.VFX;

namespace Pyre.Systems
{
    public partial struct BurningViewSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        // [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (burningView, entity) in SystemAPI
                         .Query<RefRO<BurningView>>()
                         .WithEntityAccess())
            {
                var shouldRender = SystemAPI.HasComponent<Burning>(entity);
                var isRenderEnabled = !SystemAPI.HasComponent<DisableRendering>(burningView.ValueRO.FireEntity);

                if (shouldRender == isRenderEnabled)
                {
                    continue;
                }

                if (shouldRender)
                {
                    ecb.RemoveComponent<DisableRendering>(burningView.ValueRO.FireEntity);
                }
                else
                {
                    ecb.AddComponent<DisableRendering>(burningView.ValueRO.FireEntity);
                }

                if (SystemAPI.ManagedAPI.TryGetComponent(burningView.ValueRO.FireEntity, out VisualEffect vfx))
                {
                    if (shouldRender)
                    {
                        vfx.Play();
                        vfx.playRate = 1f;
                    }
                    else
                    {
                        vfx.Stop();
                        vfx.playRate = 3f;
                    }
                }
            }
        }
    }
}