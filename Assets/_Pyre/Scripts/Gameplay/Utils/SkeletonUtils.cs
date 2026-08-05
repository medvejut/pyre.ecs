using Pyre.Skeletons.Components;
using Pyre.Skeletons.Settings;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Gameplay.Utils
{
    public static class SkeletonUtils
    {
        public static int ResolveClipIndex(SkeletonClipSet set, string clipName, Object context)
        {
            var index = set.IndexOf(clipName);
            if (index < 0)
            {
                Debug.LogError($"[{context.name}] {set.name} has no clip named '{clipName}'. Falling back to clip 0.", context);
                return 0;
            }

            return index;
        }

        public static bool HasCurrentAnimationFinished(SkeletonPose pose)
        {
            if (pose.ClipA != pose.ClipB)
            {
                return false;
            }

            if (!pose.Library.IsCreated)
            {
                return false;
            }

            ref var clips = ref pose.Library.Value.Clips;

            if (clips.Length == 0)
            {
                return false;
            }

            ref var clip = ref clips[math.clamp(pose.ClipA, 0, clips.Length - 1)];

            return !clip.Looping && pose.TimeA >= clip.Length;
        }
    }
}