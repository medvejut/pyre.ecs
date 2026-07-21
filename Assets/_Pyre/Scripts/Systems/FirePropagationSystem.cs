using Pyre.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
            var heatedEntities = new NativeHashSet<Entity>(16, Allocator.Temp);

            CollectHeated(ref state, heatedEntities);
            BurnHeated(ref state, heatedEntities);

            heatedEntities.Dispose();
        }

        [BurstCompile]
        private void CollectHeated(ref SystemState state, NativeHashSet<Entity> heatedEntities)
        {
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            var ignitableLookup = SystemAPI.GetComponentLookup<Ignitable>(true);
            var burningLookup = SystemAPI.GetComponentLookup<Burning>(true);

            foreach (var (ltw, burning, entity) in SystemAPI
                         .Query<RefRO<LocalToWorld>, RefRO<Burning>>()
                         .WithEntityAccess())
            {
                var hits = new NativeList<DistanceHit>(Allocator.Temp);

                var input = new PointDistanceInput
                {
                    Position = ltw.ValueRO.Position,
                    MaxDistance = burning.ValueRO.HeatRadius,
                    Filter = CollisionFilter.Default
                };

                if (physicsWorld.CollisionWorld.CalculateDistance(input, ref hits))
                {
                    foreach (var hit in hits)
                    {
                        if (hit.Entity == entity)
                        {
                            continue;
                        }

                        if (ignitableLookup.HasComponent(hit.Entity) && !burningLookup.HasComponent(hit.Entity))
                        {
                            heatedEntities.Add(hit.Entity);
                        }
                    }
                }

                hits.Dispose();
            }
        }

        [BurstCompile]
        private void BurnHeated(ref SystemState state, NativeHashSet<Entity> heatedEntities)
        {
            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (ignitable, progress, entity) in SystemAPI
                         .Query<RefRO<Ignitable>, RefRW<IgnitionProgress>>()
                         .WithNone<Burning>()
                         .WithEntityAccess())
            {
                if (heatedEntities.Contains(entity))
                {
                    progress.ValueRW.Elapsed += deltaTime;
                }
                else
                {
                    progress.ValueRW.Elapsed = math.max(0f, progress.ValueRO.Elapsed - ignitable.ValueRO.CoolingRate * deltaTime);
                }

                if (progress.ValueRO.Elapsed >= ignitable.ValueRO.IgnitionTime)
                {
                    ecb.AddComponent(entity, new Burning { HeatRadius = ignitable.ValueRO.BurningRadius });
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}