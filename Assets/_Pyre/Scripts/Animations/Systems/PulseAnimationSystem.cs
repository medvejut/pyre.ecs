using Pyre.Animations.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pyre.Animations.Systems
{
    [UpdateInGroup(typeof(AnimationSystemGroup))]
    public partial struct PulseAnimationSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<AnimationRestScale> _restScaleLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PulseAnimation>();

            _transformLookup = state.GetComponentLookup<LocalTransform>();
            _restScaleLookup = state.GetComponentLookup<AnimationRestScale>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _transformLookup.Update(ref state);
            _restScaleLookup.Update(ref state);

            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var deltaTime = SystemAPI.Time.DeltaTime;

            // Finished instances first, so restoring the rest scale can never clobber
            // a sibling animation that is still running on the same target.
            foreach (var (instance, entity) in
                     SystemAPI.Query<RefRO<AnimationInstance>>()
                         .WithAll<PulseAnimation>()
                         .WithEntityAccess())
            {
                if (instance.ValueRO.Elapsed < instance.ValueRO.Duration)
                    continue;

                RestoreRestScale(instance.ValueRO.Target);
                ecb.DestroyEntity(entity);
            }

            foreach (var (instance, pulse) in
                     SystemAPI.Query<RefRW<AnimationInstance>, RefRO<PulseAnimation>>())
            {
                if (instance.ValueRO.Elapsed >= instance.ValueRO.Duration)
                    continue;

                instance.ValueRW.Elapsed += deltaTime;

                if (!_transformLookup.HasComponent(instance.ValueRO.Target))
                    continue;

                var transform = _transformLookup[instance.ValueRO.Target];
                transform.Scale = Evaluate(pulse.ValueRO, instance.ValueRO);
                _transformLookup[instance.ValueRO.Target] = transform;
            }
        }

        private void RestoreRestScale(Entity target)
        {
            if (!_restScaleLookup.TryGetComponent(target, out var restScale))
                return;

            if (!_transformLookup.HasComponent(target))
                return;

            var transform = _transformLookup[target];
            transform.Scale = restScale.Value;
            _transformLookup[target] = transform;
        }

        private static float Evaluate(in PulseAnimation pulse, in AnimationInstance instance)
        {
            var elapsed = instance.Elapsed;

            // Frequency chirp: sweeps BaseFrequency -> MaxFrequency across the duration.
            var f0 = pulse.BaseFrequency;
            var f1 = pulse.MaxFrequency;
            var phase = f0 * elapsed + 0.5f * (f1 - f0) * elapsed * elapsed / instance.Duration;

            var pulseT = (math.sin(phase * 2f * math.PI) + 1f) * 0.5f;
            return math.lerp(pulse.MinScale, pulse.MaxScale, pulseT);
        }
    }
}
