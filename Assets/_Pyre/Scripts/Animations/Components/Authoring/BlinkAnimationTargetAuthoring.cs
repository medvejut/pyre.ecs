using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Animations.Components
{
    public class BlinkAnimationTargetAuthoring : MonoBehaviour
    {
        public Color initialColor = new(1f, 1f, 1f, 0f);

        public class BlinkAnimationTargetBaker : Baker<BlinkAnimationTargetAuthoring>
        {
            public override void Bake(BlinkAnimationTargetAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                var initialColor = new float4(authoring.initialColor.r, authoring.initialColor.g, authoring.initialColor.b, authoring.initialColor.a);

                AddComponent(entity, new MaterialPropertyBlinkColor { Value = initialColor });
                AddComponent(entity, new AnimationRestColor { Value = initialColor });
            }
        }
    }
}