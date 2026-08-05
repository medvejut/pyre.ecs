using Pyre.Audio.Components;
using Pyre.Cameras.Components;
using Pyre.Gameplay.Components;
using Pyre.Gameplay.Utils;
using Pyre.Effects.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;

namespace Pyre.Gameplay.Systems
{
    public partial struct ExplosionSystem : ISystem
    {
        private ComponentLookup<Destructible> _destructibleLookup;
        private ComponentLookup<DestroyRequested> _destroyRequestedLookup;
        private ComponentLookup<PhysicsVelocity> _velocityLookup;
        private ComponentLookup<PhysicsMass> _massLookup;
        private ComponentLookup<LocalToWorld> _ltwLookup;
        private ComponentLookup<KnockbackVelocity> _knockbackVelocityLookup;
        private ComponentLookup<Ignitable> _ignitableLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Explosion>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            _destructibleLookup = state.GetComponentLookup<Destructible>(isReadOnly: true);
            _destroyRequestedLookup = state.GetComponentLookup<DestroyRequested>(isReadOnly: true);
            _velocityLookup = state.GetComponentLookup<PhysicsVelocity>(isReadOnly: false);
            _massLookup = state.GetComponentLookup<PhysicsMass>(isReadOnly: true);
            _ltwLookup = state.GetComponentLookup<LocalToWorld>(isReadOnly: true);
            _knockbackVelocityLookup = state.GetComponentLookup<KnockbackVelocity>(isReadOnly: false);
            _ignitableLookup = state.GetComponentLookup<Ignitable>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.CompleteDependency();

            UpdateLookups(ref state);

            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var cameraShakeBuffer = SystemAPI.GetSingletonBuffer<CameraShakeEvent>(isReadOnly: false);
            var soundEventBuffer = SystemAPI.GetSingletonBuffer<SoundEvent>(isReadOnly: false);
            var playParticleBuffer = SystemAPI.GetSingletonBuffer<PlayParticlesEvent>(isReadOnly: false);

            foreach (var (explosion, entity) in SystemAPI
                         .Query<RefRO<Explosion>>()
                         .WithEntityAccess())
            {
                var hits = new NativeList<DistanceHit>(Allocator.Temp);
                if (physicsWorld.OverlapSphere(explosion.ValueRO.Position, explosion.ValueRO.Radius, ref hits, CollisionFilter.Default))
                {
                    foreach (var hit in hits)
                    {
                        DestroyHitEntity(hit.Entity, ecb);
                        TryKickBody(hit.Entity, explosion.ValueRO, physicsWorld);
                        TryBurnEntity(hit.Entity, ecb);
                        cameraShakeBuffer.Add(new CameraShakeEvent());
                    }
                }

                hits.Dispose();

                soundEventBuffer.Add(new SoundEvent { Position = explosion.ValueRO.Position, Sound = explosion.ValueRO.Sound });

                if (explosion.ValueRO.Vfx)
                {
                    playParticleBuffer.Add(new PlayParticlesEvent { ParticleSystem = explosion.ValueRO.Vfx, Position = explosion.ValueRO.Position });
                }

                ecb.DestroyEntity(entity);
            }
        }

        private void UpdateLookups(ref SystemState state)
        {
            _destructibleLookup.Update(ref state);
            _destroyRequestedLookup.Update(ref state);
            _velocityLookup.Update(ref state);
            _massLookup.Update(ref state);
            _ltwLookup.Update(ref state);
            _knockbackVelocityLookup.Update(ref state);
            _ignitableLookup.Update(ref state);
        }

        private void DestroyHitEntity(Entity hitEntity, EntityCommandBuffer ecb)
        {
            if (_destructibleLookup.HasComponent(hitEntity) && !_destroyRequestedLookup.HasComponent(hitEntity))
            {
                ecb.AddComponent<DestroyRequested>(hitEntity);
            }
        }

        private void TryKickBody(Entity hitEntity, in Explosion explosion, in PhysicsWorldSingleton physicsWorld)
        {
            if (!_velocityLookup.HasComponent(hitEntity) || !_massLookup.HasComponent(hitEntity) || !_ltwLookup.HasComponent(hitEntity))
                return;

            var velocity = _velocityLookup.GetRefRW(hitEntity);
            var mass = _massLookup[hitEntity];
            var ltw = _ltwLookup[hitEntity];

            var position = BodyCenter.GetWorldPosition(physicsWorld, hitEntity, ltw);
            var direction = math.normalizesafe(position - explosion.Position);

            var distance = math.distance(position, explosion.Position);
            var t = math.saturate(1f - distance / explosion.Radius);
            var impulse = explosion.Impulse * t;

            if (_knockbackVelocityLookup.HasComponent(hitEntity))
            {
                var knockbackVelocity = _knockbackVelocityLookup.GetRefRW(hitEntity);
                knockbackVelocity.ValueRW.Linear += direction * impulse;
                knockbackVelocity.ValueRW.Angular += explosion.AngularImpulse;
            }
            else
            {
                velocity.ValueRW.ApplyLinearImpulse(mass, direction * impulse);
                velocity.ValueRW.ApplyAngularImpulse(mass, explosion.AngularImpulse);
            }
        }

        private void TryBurnEntity(Entity hitEntity, EntityCommandBuffer ecb)
        {
            if (_ignitableLookup.TryGetRefRO(hitEntity, out var ignitable))
            {
                ecb.AddComponent(hitEntity, new Burning { HeatRadius = ignitable.ValueRO.BurningRadius });
            }
        }
    }
}