using Unity.Entities;
using UnityEngine;

namespace Pyre.Animations.Components
{
    public class PulseAnimationTargetAuthoring : MonoBehaviour
    {
        public class PulseAnimationTargetBaker : Baker<PulseAnimationTargetAuthoring>
        {
            public override void Bake(PulseAnimationTargetAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var restScale = authoring.transform.localScale.x;
                AddComponent(entity, new AnimationRestScale { Value = restScale });
            }
        }
    }
}