using Pyre.Gameplay.Utils;
using Pyre.Skeletons.Components;
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
                    Idle = SkeletonUtils.ResolveClipIndex(skeleton.ClipSet, authoring.IdleClipName, authoring),
                    Walk = SkeletonUtils.ResolveClipIndex(skeleton.ClipSet, authoring.WalkClipName, authoring)
                });
            }
        }
    }
}