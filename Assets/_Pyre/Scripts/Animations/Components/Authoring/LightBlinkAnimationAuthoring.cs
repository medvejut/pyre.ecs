using Pyre.Animations.Settings;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Pyre.Animations.Components.Authoring
{
    [RequireComponent(typeof(Light))]
    public class LightBlinkAnimationAuthoring : MonoBehaviour
    {
        public LightBlinkAnimationConfig Config;

        public class LightBlinkAnimationBaker : Baker<LightBlinkAnimationAuthoring>
        {
            public override void Bake(LightBlinkAnimationAuthoring authoring)
            {
                if (authoring.Config == null)
                    return;

                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var light = GetComponent<Light>();
                var baseIntensity = light.intensity;

                AddComponent(entity, new LightBlinkAnimation
                {
                    MinIntensity = baseIntensity * authoring.Config.minIntensity,
                    MaxIntensity = baseIntensity * authoring.Config.maxIntensity,
                    Frequency = authoring.Config.frequency,
                    Irregularity = authoring.Config.irregularity,
                    PhaseOffset = authoring.Config.randomizePhase ? PhaseFromPosition() : 0f,
                });
            }

            private float PhaseFromPosition()
            {
                var position = (float3)GetComponent<Transform>().position;
                var random = Random.CreateFromIndex(math.hash(position));
                return random.NextFloat(0f, 1000f);
            }
        }
    }
}