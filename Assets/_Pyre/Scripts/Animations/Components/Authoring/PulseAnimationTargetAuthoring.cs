using Unity.Entities;
using UnityEngine;

namespace Pyre.Animations.Components
{
    // Marks an entity as pulse-animatable by recording the scale it rests at.
    // The pulse parameters live on whatever triggers the animation.
    public class PulseAnimationTargetAuthoring : MonoBehaviour
    {
        public class PulseAnimationTargetBaker : Baker<PulseAnimationTargetAuthoring>
        {
            public override void Bake(PulseAnimationTargetAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new AnimationRestScale
                {
                    Value = GetComponent<Transform>().localScale.x
                });
            }
        }
    }
}
