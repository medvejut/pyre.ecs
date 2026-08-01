using Pyre.Animations.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Animations.Systems
{
    public partial struct BlinkAnimationSystem : ISystem
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

            foreach (var (blink, blinkColor, entity) in
                     SystemAPI.Query<RefRW<BlinkAnimation>, RefRW<BlinkColorMaterialProperty>>().WithEntityAccess())
            {
                if (blink.ValueRO.ElapsedTime <= 0f)
                    blink.ValueRW.ResetColor = blinkColor.ValueRO.Value;

                blink.ValueRW.ElapsedTime += deltaTime;

                var elapsed = blink.ValueRO.ElapsedTime;
                var totalDuration = blink.ValueRO.TotalDuration;

                if (elapsed >= totalDuration)
                {
                    blinkColor.ValueRW.Value = blink.ValueRO.ResetColor;
                    ecb.RemoveComponent<BlinkAnimation>(entity);
                    continue;
                }

                var normalizedProgress = elapsed / totalDuration;

                var f0 = blink.ValueRO.BaseFrequency;
                var f1 = blink.ValueRO.MaxFrequency;
                var phase = f0 * elapsed + 0.5f * (f1 - f0) * elapsed * elapsed / totalDuration;

                var blinkT = (math.sin(phase * 2f * math.PI) + 1f) * 0.5f;
                var opacity = math.lerp(blink.ValueRO.MinOpacity, blink.ValueRO.MaxOpacity, blinkT);

                var rgb = math.lerp(blink.ValueRO.StartColor.xyz, blink.ValueRO.EndColor.xyz, normalizedProgress);

                blinkColor.ValueRW.Value = new float4(rgb, opacity);
            }
        }
    }
}
