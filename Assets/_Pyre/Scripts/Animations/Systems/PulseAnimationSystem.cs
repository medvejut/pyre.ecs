using Pyre.Animations.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pyre.Animations.Systems
{
    public partial struct PulseAnimationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (pulse, transform, entity) in
                     SystemAPI.Query<RefRW<PulseAnimation>, RefRW<LocalTransform>>().WithEntityAccess())
            {
                if (pulse.ValueRO.ResetOnFinish && pulse.ValueRO.ElapsedTime <= 0f)
                {
                    pulse.ValueRW.ResetScale = transform.ValueRO.Scale;
                }

                pulse.ValueRW.ElapsedTime += deltaTime;

                var elapsed = pulse.ValueRO.ElapsedTime;
                var totalDuration = pulse.ValueRO.TotalDuration;

                if (elapsed >= totalDuration)
                {
                    if (pulse.ValueRO.ResetOnFinish)
                    {
                        transform.ValueRW.Scale = pulse.ValueRO.ResetScale;
                    }

                    ecb.RemoveComponent<PulseAnimation>(entity);
                    continue;
                }

                var f0 = pulse.ValueRO.BaseFrequency;
                var f1 = pulse.ValueRO.MaxFrequency;
                var phase = f0 * elapsed + 0.5f * (f1 - f0) * elapsed * elapsed / totalDuration;

                var pulseT = (math.sin(phase * 2f * math.PI) + 1f) * 0.5f;
                transform.ValueRW.Scale = math.lerp(pulse.ValueRO.MinScale, pulse.ValueRO.MaxScale, pulseT);
            }
        }
    }
}