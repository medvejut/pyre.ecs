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
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (fireTransform, burning) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<Burning>>())
            {
                var firePosition = fireTransform.ValueRO.Position;
                var radius = burning.ValueRO.HeatRadius;

                foreach (var (ignitableTransform, ignitable, entity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<Ignitable>>()
                             .WithNone<Burning>()
                             .WithEntityAccess())
                {
                    var distance = math.distance(firePosition, ignitableTransform.ValueRO.Position);

                    if (distance < radius)
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