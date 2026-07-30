using Pyre.Animations.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Animations.Systems
{
    public partial struct LightBlinkAnimationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LightBlinkAnimation>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (blink, light) in SystemAPI
                         .Query<RefRW<LightBlinkAnimation>, SystemAPI.ManagedAPI.UnityEngineComponent<Light>>())
            {
                blink.ValueRW.ElapsedTime += deltaTime;

                var blinkT = Evaluate(blink.ValueRO);
                light.Value.intensity = math.lerp(blink.ValueRO.MinIntensity, blink.ValueRO.MaxIntensity, blinkT);
            }
        }

        private static float Evaluate(in LightBlinkAnimation blink)
        {
            var time = blink.ElapsedTime * blink.Frequency + blink.PhaseOffset;

            // Steady part: a plain sine, keeps a readable rhythm.
            var wave = math.sin(time * 2f * math.PI);

            // Irregular part: two octaves of gradient noise, gives an organic flicker.
            var flicker = noise.snoise(new float2(time, blink.PhaseOffset)) +
                          noise.snoise(new float2(time * 2.7f, blink.PhaseOffset + 17.3f)) * 0.5f;
            flicker /= 1.5f;

            return math.saturate((math.lerp(wave, flicker, blink.Irregularity) + 1f) * 0.5f);
        }
    }
}