using Pyre.Skeletons.Settings;
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
    }
}