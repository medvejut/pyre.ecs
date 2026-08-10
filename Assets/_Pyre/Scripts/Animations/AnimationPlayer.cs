using Pyre.Animations.Components;
using Unity.Entities;

namespace Pyre.Animations
{
    public static class AnimationPlayer
    {
        private static void PlayAnimation<T>(EntityCommandBuffer ecb, Entity target, float duration, in T component) where T : unmanaged, IComponentData
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new AnimationInstance { Target = target, Duration = duration, Elapsed = 0f });
            ecb.AddComponent(entity, component);
        }

        public static void Play(EntityCommandBuffer ecb, Entity target, float duration, in PulseAnimation pulse) =>
            PlayAnimation(ecb, target, duration, pulse);

        public static void Play(EntityCommandBuffer ecb, Entity target, float duration, in BlinkAnimation blink) =>
            PlayAnimation(ecb, target, duration, blink);
    }
}