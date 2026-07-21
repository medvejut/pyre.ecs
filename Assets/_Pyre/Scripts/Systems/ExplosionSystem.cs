using Pyre.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace Pyre.Systems
{
    public partial struct ExplosionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            var destructibleLookup = SystemAPI.GetComponentLookup<Destructible>();
            var destroyRequestedLookup = SystemAPI.GetComponentLookup<DestroyRequested>();

            foreach (var (explosion, entity) in
                     SystemAPI.Query<RefRO<Explosion>>()
                         .WithEntityAccess())
            {
                var hits = new NativeList<DistanceHit>(Allocator.Temp);
                if (physicsWorld.OverlapSphere(explosion.ValueRO.Position, explosion.ValueRO.Radius, ref hits, CollisionFilter.Default))
                {
                    foreach (var hit in hits)
                    {
                        if (destructibleLookup.HasComponent(hit.Entity) && !destroyRequestedLookup.HasComponent(hit.Entity))
                        {
                            ecb.AddComponent<DestroyRequested>(hit.Entity);
                        }
                    }
                }

                hits.Dispose();

                ecb.DestroyEntity(entity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}