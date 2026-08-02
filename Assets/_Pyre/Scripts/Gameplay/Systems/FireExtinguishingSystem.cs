using Pyre.Audio.Components;
using Pyre.Gameplay.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Pyre.Gameplay.Systems
{
    public partial struct FireExtinguishingSystem : ISystem
    {
        private ComponentLookup<Water> _waterLookup;
        private ComponentLookup<IgnitionProgress> _ignitionProgressLookup;
        private ComponentLookup<Ignitable> _ignitableLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();

            _waterLookup = state.GetComponentLookup<Water>(isReadOnly: true);
            _ignitionProgressLookup = state.GetComponentLookup<IgnitionProgress>(isReadOnly: false);
            _ignitableLookup = state.GetComponentLookup<Ignitable>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            UpdateLookups(ref state);

            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var soundEventBuffer = SystemAPI.GetSingletonBuffer<SoundEvent>(isReadOnly: false);
            SystemAPI.TryGetSingleton<AudioDefaults>(out var audioDefaults);

            foreach (var (burningLtw, burning, entity) in SystemAPI
                         .Query<RefRO<LocalToWorld>, RefRO<Burning>>()
                         .WithEntityAccess())
            {
                var rigidBodyIndex = physicsWorld.GetRigidBodyIndex(entity);
                var position = burningLtw.ValueRO.Position;

                if (rigidBodyIndex != -1)
                {
                    var body = physicsWorld.Bodies[rigidBodyIndex];
                    if (CastRigidbody(body, position, physicsWorld))
                    {
                        Extinguish(entity, position, ecb, audioDefaults, soundEventBuffer);
                    }
                }
                else
                {
                    if (CastPoint(entity, position, physicsWorld))
                    {
                        Extinguish(entity, position, ecb, audioDefaults, soundEventBuffer);
                    }
                }
            }
        }

        private void UpdateLookups(ref SystemState state)
        {
            _waterLookup.Update(ref state);
            _ignitionProgressLookup.Update(ref state);
            _ignitableLookup.Update(ref state);
        }

        private bool CastRigidbody(RigidBody body, float3 position, PhysicsWorldSingleton physicsWorld)
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

                    if (_waterLookup.HasComponent(hit.Entity))
                    {
                        result = true;
                        break;
                    }
                }
            }

            hits.Dispose();
            return result;
        }

        private bool CastPoint(Entity entity, float3 position, PhysicsWorldSingleton physicsWorld)
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

                    if (_waterLookup.HasComponent(hit.Entity))
                    {
                        result = true;
                        break;
                    }
                }
            }

            hits.Dispose();
            return result;
        }

        private void Extinguish(Entity entity, float3 position, EntityCommandBuffer ecb, AudioDefaults audioDefaults, DynamicBuffer<SoundEvent> soundEventBuffer)
        {
            ecb.RemoveComponent<Burning>(entity);

            if (_ignitionProgressLookup.HasComponent(entity))
            {
                ecb.SetComponent(entity, new IgnitionProgress { Elapsed = 0f });
            }

            var sound = _ignitableLookup.TryGetComponent(entity, out var ignitable) && ignitable.ExtinguishSound
                ? ignitable.ExtinguishSound
                : audioDefaults.ExtinguishSound;

            soundEventBuffer.Add(new SoundEvent { Position = position, Sound = sound });
        }
    }
}
