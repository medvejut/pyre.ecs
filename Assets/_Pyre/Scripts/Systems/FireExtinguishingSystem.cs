using Pyre.Audio.Components;
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
            state.RequireForUpdate<AudioDefaults>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var audioDefaults = SystemAPI.GetSingleton<AudioDefaults>();
            var soundEventBuffer = SystemAPI.GetSingletonBuffer<SoundEvent>(isReadOnly: false);

            var waterLookup = SystemAPI.GetComponentLookup<Water>(true);
            var ignitionProgressLookup = SystemAPI.GetComponentLookup<IgnitionProgress>(isReadOnly: false);
            var ignitableLookup = SystemAPI.GetComponentLookup<Ignitable>(true);

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
                        Extinguish(entity, position, ecb, ignitionProgressLookup, ignitableLookup, audioDefaults, soundEventBuffer);
                    }
                }
                else
                {
                    if (CastPoint(entity, position, physicsWorld, waterLookup))
                    {
                        Extinguish(entity, position, ecb, ignitionProgressLookup, ignitableLookup, audioDefaults, soundEventBuffer);
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

        private void Extinguish(Entity entity, float3 position, EntityCommandBuffer ecb, ComponentLookup<IgnitionProgress> ignitionProgressLookup, ComponentLookup<Ignitable> ignitableLookup, AudioDefaults audioDefaults, DynamicBuffer<SoundEvent> soundEventBuffer)
        {
            ecb.RemoveComponent<Burning>(entity);

            if (ignitionProgressLookup.HasComponent(entity))
            {
                ecb.SetComponent(entity, new IgnitionProgress { Elapsed = 0f });
            }

            var clip = ignitableLookup.TryGetComponent(entity, out var ignitable) && ignitable.ExtinguishClip
                ? ignitable.ExtinguishClip
                : audioDefaults.ExtinguishClip;

            soundEventBuffer.Add(new SoundEvent { Position = position, Clip = clip, SpatialBlend = 0f });
        }
    }
}