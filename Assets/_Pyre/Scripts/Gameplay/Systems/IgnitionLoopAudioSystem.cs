using Pyre.Gameplay.Components;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Systems
{
    public partial struct IgnitionLoopAudioSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (ignitable, ignitionProgress, entity) in SystemAPI
                         .Query<RefRO<Ignitable>, RefRO<IgnitionProgress>>()
                         .WithEntityAccess())
            {
                var audioSourceEntity = ignitable.ValueRO.LoopAudioSourceEntity;
                if (audioSourceEntity == Entity.Null)
                    continue;

                if (!SystemAPI.ManagedAPI.TryGetComponent(audioSourceEntity, out AudioSource audioSource))
                    continue;

                var loopClip = ignitable.ValueRO.LoopSound;
                var shouldPlay = ignitionProgress.ValueRO.Elapsed > 0
                                 && !SystemAPI.HasComponent<Burning>(entity)
                                 && loopClip;

                if (shouldPlay == audioSource.isPlaying)
                    continue;

                if (shouldPlay)
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
    }
}
