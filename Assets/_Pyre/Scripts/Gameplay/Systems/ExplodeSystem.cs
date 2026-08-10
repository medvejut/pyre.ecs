using Pyre.Animations;
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
        private ComponentLookup<ExplosiveWarning> _warningLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            _warningLookup = state.GetComponentLookup<ExplosiveWarning>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _warningLookup.Update(ref state);

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

                StartWarning(entity, delay, ecb);
            }
        }

        private void ExplodeAfterDelay(ref SystemState state, EntityCommandBuffer ecb)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (explodeTimer, charge, ltw, entity) in
                     SystemAPI.Query<RefRW<ExplodeTimer>, RefRO<ExplosiveCharge>, RefRO<LocalToWorld>>()
                         .WithEntityAccess())
            {
                explodeTimer.ValueRW.TimeRemaining -= deltaTime;

                if (explodeTimer.ValueRO.TimeRemaining > 0f)
                    continue;

                var explosionEntity = ecb.CreateEntity();

                ecb.AddComponent(explosionEntity, new Explosion
                {
                    Position = ltw.ValueRO.Position + charge.ValueRO.Offset,
                    Radius = charge.ValueRO.Radius,
                    Impulse = charge.ValueRO.Impulse,
                    AngularImpulse = CalculateAngularImpulse(charge.ValueRO),
                    Sound = charge.ValueRO.Sound,
                    Vfx = charge.ValueRO.Vfx
                });

                ecb.RemoveComponent<ExplodeTimer>(entity);
                ecb.RemoveComponent<Explosive>(entity);

                StopWarning(entity);
            }
        }

        private void StartWarning(Entity entity, float delay, EntityCommandBuffer ecb)
        {
            if (!_warningLookup.TryGetComponent(entity, out var warning))
                return;

            if (warning.TickAudioSourceEntity != Entity.Null)
            {
                SystemAPI.GetSingletonBuffer<PlayAudioSourceEvent>()
                    .Add(new PlayAudioSourceEvent { AudioSourceEntity = warning.TickAudioSourceEntity });
            }

            if (warning.PlayPulse)
            {
                AnimationPlayer.Play(ecb, entity, delay, warning.Pulse);
            }

            if (warning.PlayBlink)
            {
                AnimationPlayer.Play(ecb, entity, delay, warning.Blink);
            }
        }

        private void StopWarning(Entity entity)
        {
            if (!_warningLookup.TryGetComponent(entity, out var warning))
                return;

            if (warning.TickAudioSourceEntity == Entity.Null)
                return;

            SystemAPI.GetSingletonBuffer<StopAudioSourceEvent>()
                .Add(new StopAudioSourceEvent { AudioSourceEntity = warning.TickAudioSourceEntity });
        }

        private static float3 CalculateAngularImpulse(in ExplosiveCharge charge)
        {
            var random = Random.CreateFromIndex(charge.AngularImpulseSeed);
            return random.NextFloat3Direction() * charge.AngularImpulseMultiplier;
        }
    }
}