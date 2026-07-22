using Pyre.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;

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

            var destructibleLookup = SystemAPI.GetComponentLookup<Destructible>(true);
            var destroyRequestedLookup = SystemAPI.GetComponentLookup<DestroyRequested>(true);
            var velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>();
            var massLookup = SystemAPI.GetComponentLookup<PhysicsMass>(true);
            var ltwLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);

            foreach (var (explosion, entity) in SystemAPI
                         .Query<RefRO<Explosion>>()
                         .WithEntityAccess())
            {
                var hits = new NativeList<DistanceHit>(Allocator.Temp);
                if (physicsWorld.OverlapSphere(explosion.ValueRO.Position, explosion.ValueRO.Radius, ref hits, CollisionFilter.Default))
                {
                    foreach (var hit in hits)
                    {
                        DestroyHitEntity(hit.Entity, destructibleLookup, destroyRequestedLookup, ecb);
                        TryKickBody(hit.Entity, explosion.ValueRO, velocityLookup, massLookup, ltwLookup);
                    }
                }

                hits.Dispose();

                ecb.DestroyEntity(entity);
            }
        }

        private static void DestroyHitEntity(Entity hitEntity, ComponentLookup<Destructible> destructibleLookup, ComponentLookup<DestroyRequested> destroyRequestedLookup, EntityCommandBuffer ecb)
        {
            if (destructibleLookup.HasComponent(hitEntity) && !destroyRequestedLookup.HasComponent(hitEntity))
            {
                ecb.AddComponent<DestroyRequested>(hitEntity);
            }
        }

        private static void TryKickBody(Entity hitEntity, Explosion explosion, ComponentLookup<PhysicsVelocity> velocityLookup, ComponentLookup<PhysicsMass> massLookup, ComponentLookup<LocalToWorld> ltwLookup)
        {
            if (velocityLookup.HasComponent(hitEntity) && massLookup.HasComponent(hitEntity) && ltwLookup.HasComponent(hitEntity))
            {
                var velocity = velocityLookup.GetRefRW(hitEntity);
                var mass = massLookup[hitEntity];
                var ltw = ltwLookup[hitEntity];

                var position = ltw.Position;
                var direction = math.normalizesafe(position - explosion.Position);

                var distance = math.distance(position, explosion.Position);
                var t = math.saturate(1f - distance / explosion.Radius);
                var impulse = explosion.Impulse * t;

                velocity.ValueRW.ApplyLinearImpulse(mass, direction * impulse);
                velocity.ValueRW.ApplyAngularImpulse(mass, explosion.AngularImpulse);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}