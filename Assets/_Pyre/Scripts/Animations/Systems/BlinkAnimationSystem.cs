using Pyre.Animations.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Animations.Systems
{
    [UpdateInGroup(typeof(AnimationSystemGroup))]
    public partial struct BlinkAnimationSystem : ISystem
    {
        private ComponentLookup<MaterialPropertyBlinkColor> _blinkColorLookup;
        private ComponentLookup<AnimationRestColor> _restColorLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BlinkAnimation>();

            _blinkColorLookup = state.GetComponentLookup<MaterialPropertyBlinkColor>();
            _restColorLookup = state.GetComponentLookup<AnimationRestColor>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _blinkColorLookup.Update(ref state);
            _restColorLookup.Update(ref state);

            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (instance, entity) in
                     SystemAPI.Query<RefRO<AnimationInstance>>()
                         .WithAll<BlinkAnimation>()
                         .WithEntityAccess())
            {
                if (instance.ValueRO.Elapsed < instance.ValueRO.Duration)
                    continue;

                RestoreRestColor(instance.ValueRO.Target);
                ecb.DestroyEntity(entity);
            }

            foreach (var (instance, blink) in
                     SystemAPI.Query<RefRW<AnimationInstance>, RefRO<BlinkAnimation>>())
            {
                if (instance.ValueRO.Elapsed >= instance.ValueRO.Duration)
                    continue;

                instance.ValueRW.Elapsed += deltaTime;

                if (!_blinkColorLookup.HasComponent(instance.ValueRO.Target))
                    continue;

                var blinkColor = _blinkColorLookup[instance.ValueRO.Target];
                blinkColor.Value = Evaluate(blink.ValueRO, instance.ValueRO);
                _blinkColorLookup[instance.ValueRO.Target] = blinkColor;
            }
        }

        private void RestoreRestColor(Entity target)
        {
            if (!_restColorLookup.TryGetComponent(target, out var restColor))
                return;

            if (!_blinkColorLookup.HasComponent(target))
                return;

            var blinkColor = _blinkColorLookup[target];
            blinkColor.Value = restColor.Value;
            _blinkColorLookup[target] = blinkColor;
        }

        private static float4 Evaluate(in BlinkAnimation blink, in AnimationInstance instance)
        {
            var elapsed = instance.Elapsed;
            var normalizedProgress = elapsed / instance.Duration;

            var f0 = blink.BaseFrequency;
            var f1 = blink.MaxFrequency;
            var phase = f0 * elapsed + 0.5f * (f1 - f0) * elapsed * elapsed / instance.Duration;

            var blinkT = (math.sin(phase * 2f * math.PI) + 1f) * 0.5f;
            var opacity = math.lerp(blink.MinOpacity, blink.MaxOpacity, blinkT);

            var rgb = math.lerp(blink.StartColor.xyz, blink.EndColor.xyz, normalizedProgress);

            return new float4(rgb, opacity);
        }
    }
}