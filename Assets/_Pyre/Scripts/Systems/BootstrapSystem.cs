using Pyre.Audio.Components;
using Pyre.Cameras.Components;
using Unity.Burst;
using Unity.Entities;

namespace Pyre.Systems
{
    public partial struct BootstrapSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<CameraShakeEvent>();
            state.EntityManager.CreateSingletonBuffer<SoundEvent>();
        }
    }
}