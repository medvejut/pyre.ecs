using Pyre.Animations.Settings;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Animations.Components
{
    public class PulseAnimationSourceAuthoring : MonoBehaviour
    {
        public PulseAnimationConfig Config;

        public class PulseAnimationSourceBaker : Baker<PulseAnimationSourceAuthoring>
        {
            public override void Bake(PulseAnimationSourceAuthoring authoring)
            {
                if (authoring.Config == null)
                    return;

                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new PulseAnimationSource
                {
                    MinScale = authoring.Config.minScale,
                    MaxScale = authoring.Config.maxScale,
                    BaseFrequency = authoring.Config.baseFrequency,
                    MaxFrequency = authoring.Config.maxFrequency,
                });
            }
        }
    }
}
