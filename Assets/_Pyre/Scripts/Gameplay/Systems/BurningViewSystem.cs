using Pyre.Audio.Components;
using Pyre.Gameplay.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.VFX;

namespace Pyre.Gameplay.Systems
{
    public partial struct BurningViewSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var soundEventBuffer = SystemAPI.GetSingletonBuffer<SoundEvent>(isReadOnly: false);

            foreach (var (burningView, ltw, entity) in SystemAPI
                         .Query<RefRO<BurningView>, RefRO<LocalToWorld>>()
                         .WithEntityAccess())
            {
                var shouldRender = SystemAPI.HasComponent<Burning>(entity);
                var isRenderEnabled = !SystemAPI.HasComponent<DisableRendering>(burningView.ValueRO.FireEntity);

                if (shouldRender == isRenderEnabled)
                {
                    continue;
                }

                if (shouldRender)
                {
                    ecb.RemoveComponent<DisableRendering>(burningView.ValueRO.FireEntity);

                    PlaySound(ref state, entity, ltw, soundEventBuffer);
                }
                else
                {
                    ecb.AddComponent<DisableRendering>(burningView.ValueRO.FireEntity);
                }

                if (SystemAPI.ManagedAPI.TryGetComponent(burningView.ValueRO.FireEntity, out VisualEffect vfx))
                {
                    if (shouldRender)
                    {
                        vfx.Play();
                        vfx.playRate = 1f;
                    }
                    else
                    {
                        vfx.Stop();
                        vfx.playRate = 3f;
                    }
                }
            }
        }

        private void PlaySound(ref SystemState state, Entity entity, RefRO<LocalToWorld> ltw, DynamicBuffer<SoundEvent> soundEventBuffer)
        {
            if (SystemAPI.HasComponent<MuteBurnSound>(entity))
                return;

            if (SystemAPI.HasBuffer<BurnSoundClip>(entity))
            {
                var burnSoundClips = SystemAPI.GetBuffer<BurnSoundClip>(entity);
                foreach (var burnSoundClip in burnSoundClips)
                {
                    soundEventBuffer.Add(new SoundEvent { Position = ltw.ValueRO.Position, Clip = burnSoundClip.Clip, SpatialBlend = 0f });
                }
            }

            if (SystemAPI.TryGetComponent<Ignitable>(entity, out var ignitable) && ignitable.OnBurnClip)
            {
                soundEventBuffer.Add(new SoundEvent { Position = ltw.ValueRO.Position, Clip = ignitable.OnBurnClip, SpatialBlend = 0f });
            }
        }
    }
}