using Pyre.Animations.Settings;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Animations.Components
{
    // Marks an entity as blink-animatable by adding the driven material property and
    // recording the color it rests at. The blink parameters live on whatever triggers
    // the animation; only initialColor is read here.
    public class BlinkAnimationTargetAuthoring : MonoBehaviour
    {
        public BlinkAnimationConfig Config;

        public class BlinkAnimationTargetBaker : Baker<BlinkAnimationTargetAuthoring>
        {
            public override void Bake(BlinkAnimationTargetAuthoring authoring)
            {
                DependsOn(authoring.Config);

                if (authoring.Config == null)
                    return;

                var entity = GetEntity(TransformUsageFlags.Renderable);
                var initialColor = authoring.Config.InitialColor;

                AddComponent(entity, new MaterialPropertyBlinkColor { Value = initialColor });
                AddComponent(entity, new AnimationRestColor { Value = initialColor });
            }
        }
    }
}
