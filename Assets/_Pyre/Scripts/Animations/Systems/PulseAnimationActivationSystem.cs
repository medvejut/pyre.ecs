using Pyre.Animations.Components;
using Unity.Burst;
using Unity.Entities;

namespace Pyre.Animations.Systems
{
    [UpdateInGroup(typeof(AnimationActivationGroup))]
    public partial struct PulseAnimationActivationSystem : ISystem
    {
        private ComponentLookup<PulseAnimationSource> _pulseAnimationSourceLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PlayAnimationEvent>();
            _pulseAnimationSourceLookup = state.GetComponentLookup<PulseAnimationSource>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _pulseAnimationSourceLookup.Update(ref state);

            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var playAnimationEvent in SystemAPI.GetSingletonBuffer<PlayAnimationEvent>(isReadOnly: true))
            {
                if (!_pulseAnimationSourceLookup.TryGetComponent(playAnimationEvent.Target, out var pulseAnimationSource))
                    continue;

                ecb.AddComponent(playAnimationEvent.Target, new PulseAnimation
                {
                    MinScale = pulseAnimationSource.MinScale,
                    MaxScale = pulseAnimationSource.MaxScale,
                    BaseFrequency = pulseAnimationSource.BaseFrequency,
                    MaxFrequency = pulseAnimationSource.MaxFrequency,
                    ResetOnFinish = pulseAnimationSource.ResetOnFinish,

                    TotalDuration = playAnimationEvent.Duration,
                    ElapsedTime = 0f,
                });
            }
        }
    }
}
