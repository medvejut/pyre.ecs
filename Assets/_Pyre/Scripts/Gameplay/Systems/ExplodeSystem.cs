using Pyre.Animations.Components;
using Pyre.Audio.Components;
using Pyre.Gameplay.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Pyre.Gameplay.Systems
{
    public partial struct ExplodeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
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

                if (explosive.ValueRO.TickAudioSourceEntity != Entity.Null)
                {
                    SystemAPI.GetSingletonBuffer<PlayAudioSourceEvent>()
                        .Add(new PlayAudioSourceEvent { AudioSourceEntity = explosive.ValueRO.TickAudioSourceEntity });
                }

                SystemAPI.GetSingletonBuffer<PlayAnimationEvent>()
                    .Add(new PlayAnimationEvent { Target = entity, Duration = delay });
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
                        Sound = explosive.ValueRO.ExplosionSound,
                        Vfx = explosive.ValueRO.ExplosionVfx
                    });

                    ecb.RemoveComponent<ExplodeTimer>(entity);
                    ecb.RemoveComponent<Explosive>(entity);

                    if (explosive.ValueRO.TickAudioSourceEntity != Entity.Null)
                    {
                        SystemAPI.GetSingletonBuffer<StopAudioSourceEvent>()
                            .Add(new StopAudioSourceEvent { AudioSourceEntity = explosive.ValueRO.TickAudioSourceEntity });
                    }
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