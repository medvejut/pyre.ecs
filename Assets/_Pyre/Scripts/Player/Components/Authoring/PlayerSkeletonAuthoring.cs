using Pyre.Skeletons.Components;
using Pyre.Skeletons.Settings;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Player.Components
{
    public class PlayerSkeletonAuthoring : MonoBehaviour
    {
        public string IdleClipName = "Idle";
        public string WalkClipName = "Walk";

        public class PlayerSkeletonBaker : Baker<PlayerSkeletonAuthoring>
        {
            public override void Bake(PlayerSkeletonAuthoring authoring)
            {
                var skeleton = GetComponentInChildren<SkeletonAuthoring>();
                if (skeleton == null || skeleton.ClipSet == null)
                {
                    Debug.LogError($"[{authoring.name}] has no SkeletonAuthoring with a clip set among its children.", authoring);
                    return;
                }

                DependsOn(skeleton.ClipSet);

                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new PlayerSkeleton
                {
                    Skeleton = GetEntity(skeleton, TransformUsageFlags.None),
                    Idle = Resolve(skeleton.ClipSet, authoring.IdleClipName, authoring),
                    Walk = Resolve(skeleton.ClipSet, authoring.WalkClipName, authoring)
                });
            }

            private static int Resolve(SkeletonClipSet set, string clipName, Object context)
            {
                var index = set.IndexOf(clipName);
                if (index >= 0)
                {
                    return index;
                }

                Debug.LogError($"[{context.name}] {set.name} has no clip named '{clipName}'. Falling back to clip 0.", context);
                return 0;
            }
        }
    }
}