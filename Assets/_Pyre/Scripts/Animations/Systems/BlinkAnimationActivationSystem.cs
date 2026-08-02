using Pyre.Animations.Components;
using Unity.Burst;
using Unity.Entities;

namespace Pyre.Animations.Systems
{
    [UpdateInGroup(typeof(AnimationActivationGroup))]
    public partial struct BlinkAnimationActivationSystem : ISystem
    {
        private ComponentLookup<BlinkAnimationSource> _blinkAnimationSourceLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PlayAnimationEvent>();
            _blinkAnimationSourceLookup = state.GetComponentLookup<BlinkAnimationSource>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _blinkAnimationSourceLookup.Update(ref state);

            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var playAnimationEvent in SystemAPI.GetSingletonBuffer<PlayAnimationEvent>(isReadOnly: true))
            {
                if (!_blinkAnimationSourceLookup.TryGetComponent(playAnimationEvent.Target, out var blinkAnimationSource))
                    continue;

                ecb.AddComponent(playAnimationEvent.Target, new BlinkAnimation
                {
                    StartColor = blinkAnimationSource.StartColor,
                    EndColor = blinkAnimationSource.EndColor,
                    MinOpacity = blinkAnimationSource.MinOpacity,
                    MaxOpacity = blinkAnimationSource.MaxOpacity,
                    BaseFrequency = blinkAnimationSource.BaseFrequency,
                    MaxFrequency = blinkAnimationSource.MaxFrequency,
                    ResetOnFinish = blinkAnimationSource.ResetOnFinish,

                    TotalDuration = playAnimationEvent.Duration,
                    ElapsedTime = 0f,
                });
            }
        }
    }
}
