using Pyre.Animations.Components;
using Pyre.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Pyre.Systems
{
    public partial struct ExplodeSystem : ISystem
    {
        private ComponentLookup<PulseAnimationSource> _pulseAnimationSourceLookup;
        private ComponentLookup<BlinkAnimationSource> _blinkAnimationSourceLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            _pulseAnimationSourceLookup = state.GetComponentLookup<PulseAnimationSource>(isReadOnly: true);
            _blinkAnimationSourceLookup = state.GetComponentLookup<BlinkAnimationSource>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _pulseAnimationSourceLookup.Update(ref state);
            _blinkAnimationSourceLookup.Update(ref state);

            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            ProcessExplodeOnStartBurn(ref state, ecb);
            ExplodeAfterDelay(ref state, ecb);
        }

        private void ProcessExplodeOnStartBurn(ref SystemState state, EntityCommandBuffer ecb)
        {
            foreach (var (explosive, entity) in
                     SystemAPI.Query<RefRO<Explosive>>()
                         .WithNone<ExplodeTimer>()
                         .WithAll<Burning>()
                         .WithEntityAccess())
            {
                if (!explosive.ValueRO.ExplodeOnStartBurn)
                    continue;

                var delay = explosive.ValueRO.Delay;
                ecb.AddComponent(entity, new ExplodeTimer { TimeRemaining = delay });

                if (_pulseAnimationSourceLookup.TryGetComponent(entity, out var pulseAnimationSource))
                {
                    ecb.AddComponent(entity, new PulseAnimation
                    {
                        MinScale = pulseAnimationSource.MinScale,
                        MaxScale = pulseAnimationSource.MaxScale,
                        BaseFrequency = pulseAnimationSource.BaseFrequency,
                        MaxFrequency = pulseAnimationSource.MaxFrequency,

                        TotalDuration = delay,
                        ElapsedTime = 0f,
                    });
                }

                if (_blinkAnimationSourceLookup.TryGetComponent(entity, out var blinkAnimationSource))
                {
                    ecb.AddComponent(entity, new BlinkAnimation
                    {
                        StartColor = blinkAnimationSource.StartColor,
                        EndColor = blinkAnimationSource.EndColor,
                        MinOpacity = blinkAnimationSource.MinOpacity,
                        MaxOpacity = blinkAnimationSource.MaxOpacity,
                        BaseFrequency = blinkAnimationSource.BaseFrequency,
                        MaxFrequency = blinkAnimationSource.MaxFrequency,

                        TotalDuration = delay,
                        ElapsedTime = 0f,
                    });
                }
            }
        }

        private void ExplodeAfterDelay(ref SystemState state, EntityCommandBuffer ecb)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (explodeTimer, explosive, ltw, entity) in
                     SystemAPI.Query<RefRW<ExplodeTimer>, RefRO<Explosive>, RefRO<LocalToWorld>>()
                         .WithEntityAccess())
            {
                explodeTimer.ValueRW.TimeRemaining -= deltaTime;

                if (explodeTimer.ValueRO.TimeRemaining <= 0f)
                {
                    var explosionEntity = ecb.CreateEntity();

                    ecb.AddComponent(explosionEntity, new Explosion
                    {
                        Position = ltw.ValueRO.Position + explosive.ValueRO.ExplosionOffset,
                        Radius = explosive.ValueRO.ExplosionRadius,
                        Impulse = explosive.ValueRO.ExplosionImpulse,
                        AngularImpulse = CalculateExplosionAngularImpulse(explosive),
                    });

                    ecb.RemoveComponent<ExplodeTimer>(entity);
                }
            }
        }

        private static float3 CalculateExplosionAngularImpulse(RefRO<Explosive> explosive)
        {
            var random = Random.CreateFromIndex(explosive.ValueRO.CustomExplosionAngularImpulseRandomSeed);
            return random.NextFloat3Direction() * explosive.ValueRO.CustomExplosionAngularImpulseMultiplier;
        }
    }
}