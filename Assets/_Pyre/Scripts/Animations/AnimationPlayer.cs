using Pyre.Animations.Components;
using Unity.Entities;

namespace Pyre.Animations
{
    // Call this wherever an animation should start. Every call creates its own
    // instance entity, so triggering the same animation twice stacks instead of
    // overwriting.
    public static class AnimationPlayer
    {
        public static Entity Play(EntityCommandBuffer ecb, Entity target, float duration)
        {
            var instance = ecb.CreateEntity();
            ecb.AddComponent(instance, new AnimationInstance
            {
                Target = target,
                Duration = duration,
                Elapsed = 0f,
            });

            return instance;
        }

        public static void Play(EntityCommandBuffer ecb, Entity target, float duration, in PulseAnimation pulse)
        {
            ecb.AddComponent(Play(ecb, target, duration), pulse);
        }

        public static void Play(EntityCommandBuffer ecb, Entity target, float duration, in BlinkAnimation blink)
        {
            ecb.AddComponent(Play(ecb, target, duration), blink);
        }
    }
}
