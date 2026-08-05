using Pyre.Gameplay.Utils;
using Pyre.Skeletons.Components;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public class EnemySkeletonAuthoring : MonoBehaviour
    {
        public string IdleClipName;
        public string BurnClipName;
        public string WarningClipName;

        public float FadeDuration = 0.15f;

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
                    Burn = SkeletonUtils.ResolveClipIndex(skeleton.ClipSet, authoring.BurnClipName, authoring),
                    Warning = SkeletonUtils.ResolveClipIndex(skeleton.ClipSet, authoring.WarningClipName, authoring),
                    FadeDuration = authoring.FadeDuration
                });

                AddComponent(entity, new EnemyAnimationState { State = EnemyAnimation.Reset });
            }
        }
    }
}