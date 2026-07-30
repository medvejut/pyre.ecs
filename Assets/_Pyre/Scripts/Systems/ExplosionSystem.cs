using Pyre.Audio.Components;
using Pyre.Cameras.Components;
using Pyre.Components;
using Pyre.Effects.Component;
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
            var cameraShakeBuffer = SystemAPI.GetSingletonBuffer<CameraShakeEvent>(isReadOnly: false);
            var soundEventBuffer = SystemAPI.GetSingletonBuffer<SoundEvent>(isReadOnly: false);
            var playParticleBuffer = SystemAPI.GetSingletonBuffer<PlayParticlesEvent>(isReadOnly: false);

            var destructibleLookup = SystemAPI.GetComponentLookup<Destructible>(true);
            var destroyRequestedLookup = SystemAPI.GetComponentLookup<DestroyRequested>(true);
            var velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>();
            var massLookup = SystemAPI.GetComponentLookup<PhysicsMass>(true);
            var ltwLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
            var knockbackVelocityLookup = SystemAPI.GetComponentLookup<KnockbackVelocity>();
            var ignitableLookup = SystemAPI.GetComponentLookup<Ignitable>(true);

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
                        TryKickBody(hit.Entity, explosion.ValueRO, velocityLookup, massLookup, ltwLookup, knockbackVelocityLookup);
                        TryBurnEntity(hit.Entity, ignitableLookup, ecb);
                        cameraShakeBuffer.Add(new CameraShakeEvent());
                    }
                }

                hits.Dispose();

                soundEventBuffer.Add(new SoundEvent { Position = explosion.ValueRO.Position, Clip = explosion.ValueRO.Clip, SpatialBlend = 0f });

                if (explosion.ValueRO.Vfx)
                {
                    playParticleBuffer.Add(new PlayParticlesEvent { ParticleSystem = explosion.ValueRO.Vfx, Position = explosion.ValueRO.Position });
                }

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

        private static void TryKickBody(Entity hitEntity, Explosion explosion, ComponentLookup<PhysicsVelocity> velocityLookup, ComponentLookup<PhysicsMass> massLookup, ComponentLookup<LocalToWorld> ltwLookup, ComponentLookup<KnockbackVelocity> knockbackVelocityLookup)
        {
            if (!velocityLookup.HasComponent(hitEntity) || !massLookup.HasComponent(hitEntity) || !ltwLookup.HasComponent(hitEntity))
                return;

            var velocity = velocityLookup.GetRefRW(hitEntity);
            var mass = massLookup[hitEntity];
            var ltw = ltwLookup[hitEntity];

            var position = ltw.Position;
            var direction = math.normalizesafe(position - explosion.Position);

            var distance = math.distance(position, explosion.Position);
            var t = math.saturate(1f - distance / explosion.Radius);
            var impulse = explosion.Impulse * t;

            if (knockbackVelocityLookup.HasComponent(hitEntity))
            {
                var knockbackVelocity = knockbackVelocityLookup.GetRefRW(hitEntity);
                knockbackVelocity.ValueRW.Linear += direction * impulse;
                knockbackVelocity.ValueRW.Angular += explosion.AngularImpulse;
            }
            else
            {
                velocity.ValueRW.ApplyLinearImpulse(mass, direction * impulse);
                velocity.ValueRW.ApplyAngularImpulse(mass, explosion.AngularImpulse);
            }
        }

        private void TryBurnEntity(Entity hitEntity, ComponentLookup<Ignitable> ignitableLookup, EntityCommandBuffer ecb)
        {
            if (ignitableLookup.TryGetRefRO(hitEntity, out var ignitable))
            {
                ecb.AddComponent(hitEntity, new Burning { HeatRadius = ignitable.ValueRO.BurningRadius });
            }
        }
    }
}