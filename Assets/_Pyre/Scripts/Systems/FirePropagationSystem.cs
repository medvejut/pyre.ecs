using Pyre.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pyre.Systems
{
    public partial struct FirePropagationSystem : ISystem
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

            foreach (var (fireTransform, burning) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<Burning>>())
            {
                var burningPosition = fireTransform.ValueRO.Position;
                var burningRadius = burning.ValueRO.HeatRadius;

                foreach (var (ignitableTransform, ignitable, entity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<Ignitable>>()
                             .WithNone<Burning>()
                             .WithEntityAccess())
                {
                    var ignitablePosition = ignitableTransform.ValueRO.Position;
                    var distance = math.distance(burningPosition, ignitablePosition);

                    if (distance < burningRadius)
                    {
                        ecb.AddComponent(entity, new Burning { HeatRadius = ignitable.ValueRO.BurningRadius });
                    }
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}