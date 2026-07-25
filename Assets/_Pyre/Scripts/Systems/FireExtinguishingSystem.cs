using Pyre.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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

            var waterLookup = SystemAPI.GetComponentLookup<Water>(true);
            var ignitionProgressLookup = SystemAPI.GetComponentLookup<IgnitionProgress>(isReadOnly: false);

            foreach (var (burningLtw, burning, entity) in SystemAPI
                         .Query<RefRO<LocalToWorld>, RefRO<Burning>>()
                         .WithEntityAccess())
            {
                var rigidBodyIndex = physicsWorld.GetRigidBodyIndex(entity);
                var position = burningLtw.ValueRO.Position;

                if (rigidBodyIndex != -1)
                {
                    var body = physicsWorld.Bodies[rigidBodyIndex];
                    if (CastRigidbody(body, position, physicsWorld, waterLookup))
                    {
                        Extinguish(entity, ecb, ignitionProgressLookup);
                    }
                }
                else
                {
                    if (CastPoint(entity, position, physicsWorld, waterLookup))
                    {
                        Extinguish(entity, ecb, ignitionProgressLookup);
                    }
                }
            }
        }

        private bool CastRigidbody(RigidBody body, float3 position, PhysicsWorldSingleton physicsWorld, ComponentLookup<Water> waterLookup)
        {
            var result = false;
            var hits = new NativeList<ColliderCastHit>(Allocator.Temp);

            var input = new ColliderCastInput(body.Collider, position, position);
            if (physicsWorld.CastCollider(input, ref hits))
            {
                foreach (var hit in hits)
                {
                    if (hit.Entity == body.Entity)
                        continue;

                    if (waterLookup.HasComponent(hit.Entity))
                    {
                        result = true;
                        break;
                    }
                }
            }

            hits.Dispose();
            return result;
        }

        private bool CastPoint(Entity entity, float3 position, PhysicsWorldSingleton physicsWorld, ComponentLookup<Water> waterLookup)
        {
            var result = false;
            var hits = new NativeList<DistanceHit>(Allocator.Temp);

            var input = new PointDistanceInput
            {
                Position = position,
                MaxDistance = 0f,
                Filter = CollisionFilter.Default
            };

            if (physicsWorld.CalculateDistance(input, ref hits))
            {
                foreach (var hit in hits)
                {
                    if (hit.Entity == entity)
                        continue;

                    if (waterLookup.HasComponent(hit.Entity))
                    {
                        result = true;
                        break;
                    }
                }
            }

            hits.Dispose();
            return result;
        }

        private void Extinguish(Entity entity, EntityCommandBuffer ecb, ComponentLookup<IgnitionProgress> ignitionProgressLookup)
        {
            ecb.RemoveComponent<Burning>(entity);

            if (ignitionProgressLookup.HasComponent(entity))
            {
                ecb.SetComponent(entity, new IgnitionProgress { Elapsed = 0f });
            }
        }
    }
}