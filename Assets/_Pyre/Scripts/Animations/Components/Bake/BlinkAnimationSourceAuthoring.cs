using Pyre.Animations.Settings;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Animations.Components.Bake
{
    public class BlinkAnimationSourceAuthoring : MonoBehaviour
    {
        public BlinkAnimationConfig Config;

        public class BlinkAnimationSourceBaker : Baker<BlinkAnimationSourceAuthoring>
        {
            public override void Bake(BlinkAnimationSourceAuthoring authoring)
            {
                if (authoring.Config == null)
                    return;

                var entity = GetEntity(TransformUsageFlags.Renderable);

                var startColor = authoring.Config.startColor;
                var endColor = authoring.Config.endColor;

                AddComponent(entity, new BlinkAnimationSource
                {
                    StartColor = new float4(startColor.r, startColor.g, startColor.b, startColor.a),
                    EndColor = new float4(endColor.r, endColor.g, endColor.b, endColor.a),
                    MinOpacity = authoring.Config.minOpacity,
                    MaxOpacity = authoring.Config.maxOpacity,
                    BaseFrequency = authoring.Config.baseFrequency,
                    MaxFrequency = authoring.Config.maxFrequency,
                });
            }
        }
    }
}
