using Pyre.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace Pyre.Systems
{
    public partial struct FirePropagationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            var ignitableLookup = SystemAPI.GetComponentLookup<Ignitable>(true);
            var burningLookup = SystemAPI.GetComponentLookup<Burning>(true);

            foreach (var (burningLtw, burning, burningEntity) in SystemAPI
                         .Query<RefRO<LocalToWorld>, RefRO<Burning>>()
                         .WithEntityAccess())
            {
                var hits = new NativeList<DistanceHit>(Allocator.Temp);

                var input = new PointDistanceInput
                {
                    Position = burningLtw.ValueRO.Position,
                    MaxDistance = burning.ValueRO.HeatRadius,
                    Filter = CollisionFilter.Default
                };

                if (physicsWorld.CollisionWorld.CalculateDistance(input, ref hits))
                {
                    foreach (var hit in hits)
                    {
                        if (hit.Entity == burningEntity)
                        {
                            continue;
                        }

                        if (ignitableLookup.TryGetComponent(hit.Entity, out var ignitable) && !burningLookup.HasComponent(hit.Entity))
                        {
                            ecb.AddComponent(hit.Entity, new Burning { HeatRadius = ignitable.BurningRadius });
                        }
                    }
                }

                hits.Dispose();
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}