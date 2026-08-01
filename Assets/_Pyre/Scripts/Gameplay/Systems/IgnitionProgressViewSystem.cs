using Pyre.Audio;
using Pyre.Audio.Components;
using Pyre.Gameplay.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Pyre.Gameplay.Systems
{
    public partial struct IgnitionProgressViewSystem : ISystem
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

            SystemAPI.TryGetSingletonBuffer<DefaultSoundClip>(out var soundDefaults, isReadOnly: true);

            foreach (var (view, ignitable, ignitionProgress, entity) in SystemAPI
                         .Query<RefRO<IgnitionProgressView>, RefRO<Ignitable>, RefRO<IgnitionProgress>>()
                         .WithEntityAccess())
            {
                var shouldRender = ignitionProgress.ValueRO.Elapsed > 0 && !SystemAPI.HasComponent<Burning>(entity);
                var isRenderEnabled = !SystemAPI.HasComponent<DisableRendering>(view.ValueRO.ProgressEntity);

                if (shouldRender != isRenderEnabled)
                {
                    if (shouldRender)
                    {
                        ecb.RemoveComponent<DisableRendering>(view.ValueRO.ProgressEntity);
                    }
                    else
                    {
                        ecb.AddComponent<DisableRendering>(view.ValueRO.ProgressEntity);
                    }

                    if (SystemAPI.ManagedAPI.TryGetComponent(view.ValueRO.ProgressEntity, out AudioSource audioSource))
                    {
                        var loopClip = SoundClipUtility.Resolve(SoundKind.BurningLoop, entity, _soundClipOverrideLookup, soundDefaults);

                        if (shouldRender && loopClip && !SoundClipUtility.IsMuted(SoundKind.BurningLoop, entity, _mutedSoundLookup))
                        {
                            audioSource.clip = loopClip;
                            audioSource.loop = true;
                            audioSource.Play();
                        }
                        else
                        {
                            audioSource.Stop();
                        }
                    }
                }

                if (SystemAPI.TryGetComponent<ProgressMaterialProperty>(view.ValueRO.ProgressEntity, out var progressMaterialProperty))
                {
                    progressMaterialProperty.Value = ignitionProgress.ValueRO.Elapsed / ignitable.ValueRO.IgnitionTime;
                    SystemAPI.SetComponent(view.ValueRO.ProgressEntity, progressMaterialProperty);
                }
            }
        }

        private void UpdateLookups(ref SystemState state)
        {
            _soundClipOverrideLookup.Update(ref state);
            _mutedSoundLookup.Update(ref state);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}