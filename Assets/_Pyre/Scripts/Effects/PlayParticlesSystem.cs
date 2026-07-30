using Pyre.Effects.Component;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Effects
{
    public partial struct PlayParticlesSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<PlayParticlesEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var buffer = SystemAPI.GetSingletonBuffer<PlayParticlesEvent>();

            foreach (var playParticlesEvent in buffer)
            {
                if (!playParticlesEvent.ParticleSystem)
                    continue;

                var particleSystem = Object.Instantiate<ParticleSystem>(playParticlesEvent.ParticleSystem, playParticlesEvent.Position, Quaternion.identity);
                particleSystem.Play();
            }

            buffer.Clear();
        }
    }
}