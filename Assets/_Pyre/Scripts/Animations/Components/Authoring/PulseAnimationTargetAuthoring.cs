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

                // Through the baker, not authoring.transform: that is what records the dependency on the scale.
                var restScale = GetComponent<Transform>().localScale.x;
                AddComponent(entity, new AnimationRestScale { Value = restScale });
            }
        }
    }
}