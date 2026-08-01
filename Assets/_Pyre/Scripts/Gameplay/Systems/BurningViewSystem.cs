using Pyre.Audio;
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
        private BufferLookup<SoundClipOverride> _soundClipOverrideLookup;
        private BufferLookup<MutedSound> _mutedSoundLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();

            _soundClipOverrideLookup = state.GetBufferLookup<SoundClipOverride>(isReadOnly: true);
            _mutedSoundLookup = state.GetBufferLookup<MutedSound>(isReadOnly: true);
        }

        public void OnUpdate(ref SystemState state)
        {
            UpdateLookups(ref state);

            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var soundEventBuffer = SystemAPI.GetSingletonBuffer<SoundEvent>(isReadOnly: false);
            SystemAPI.TryGetSingletonBuffer<DefaultSoundClip>(out var soundDefaults, isReadOnly: true);

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

                    SoundClipUtility.Queue(SoundKind.Burn, entity, ltw.ValueRO.Position, 0f, _soundClipOverrideLookup, _mutedSoundLookup, soundDefaults, soundEventBuffer);
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

        private void UpdateLookups(ref SystemState state)
        {
            _soundClipOverrideLookup.Update(ref state);
            _mutedSoundLookup.Update(ref state);
        }
    }
}