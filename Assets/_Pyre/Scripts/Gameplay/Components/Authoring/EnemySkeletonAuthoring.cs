using Pyre.Gameplay.Utils;
using Pyre.Skeletons.Components;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public class EnemySkeletonAuthoring : MonoBehaviour
    {
        public string IdleClipName;
        public string FallClipName;
        public string WarningClipName;

        public class EnemySkeletonBaker : Baker<EnemySkeletonAuthoring>
        {
            public override void Bake(EnemySkeletonAuthoring authoring)
            {
                var skeleton = GetComponentInChildren<SkeletonAuthoring>();
                if (skeleton == null || skeleton.ClipSet == null)
                {
                    Debug.LogError($"[{authoring.name}] has no SkeletonAuthoring with a clip set among its children.", authoring);
                    return;
                }

                DependsOn(skeleton.ClipSet);

                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new EnemySkeleton
                {
                    Skeleton = GetEntity(skeleton, TransformUsageFlags.None),
                    Idle = SkeletonUtils.ResolveClipIndex(skeleton.ClipSet, authoring.IdleClipName, authoring),
                    Fall = SkeletonUtils.ResolveClipIndex(skeleton.ClipSet, authoring.FallClipName, authoring),
                    Warning = SkeletonUtils.ResolveClipIndex(skeleton.ClipSet, authoring.WarningClipName, authoring)
                });
            }
        }
    }
}