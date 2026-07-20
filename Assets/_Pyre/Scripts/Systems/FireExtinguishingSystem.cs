using Pyre.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace Pyre.Systems
{
    public partial struct FireExtinguishingSystem : ISystem
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

            foreach (var (burningTransform, burning, entity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<Burning>>()
                         .WithEntityAccess())
            {
                var hits = new NativeList<DistanceHit>(Allocator.Temp);

                var input = new PointDistanceInput
                {
                    Position = burningTransform.ValueRO.Position,
                    MaxDistance = 0f,
                    Filter = CollisionFilter.Default
                };

                if (physicsWorld.CollisionWorld.CalculateDistance(input, ref hits))
                {
                    foreach (var hit in hits)
                    {
                        if (SystemAPI.HasComponent<Water>(hit.Entity))
                        {
                            ecb.RemoveComponent<Burning>(entity);
                            break;
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