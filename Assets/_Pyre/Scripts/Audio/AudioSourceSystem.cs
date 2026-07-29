using Pyre.Audio.Components;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Audio
{
    public partial struct AudioSourceSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<SoundEvent>();
            state.EntityManager.CreateSingletonBuffer<PlayAudioSourceEvent>();
            state.EntityManager.CreateSingletonBuffer<StopAudioSourceEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            Play(ref state);
            Stop(ref state);
        }

        private void Play(ref SystemState state)
        {
            var playAudioSourceEventBuffer = SystemAPI.GetSingletonBuffer<PlayAudioSourceEvent>();
            foreach (var audioSourceEvent in playAudioSourceEventBuffer)
            {
                if (audioSourceEvent.AudioSourceEntity == Entity.Null)
                    continue;

                var audioSource = SystemAPI.ManagedAPI.GetComponent<AudioSource>(audioSourceEvent.AudioSourceEntity);
                audioSource.Play();
            }

            playAudioSourceEventBuffer.Clear();
        }

        private void Stop(ref SystemState state)
        {
            var stopAudioSourceEventBuffer = SystemAPI.GetSingletonBuffer<StopAudioSourceEvent>();
            foreach (var audioSourceEvent in stopAudioSourceEventBuffer)
            {
                if (audioSourceEvent.AudioSourceEntity == Entity.Null)
                    continue;

                var audioSource = SystemAPI.ManagedAPI.GetComponent<AudioSource>(audioSourceEvent.AudioSourceEntity);
                audioSource.Stop();
            }

            stopAudioSourceEventBuffer.Clear();
        }
    }
}