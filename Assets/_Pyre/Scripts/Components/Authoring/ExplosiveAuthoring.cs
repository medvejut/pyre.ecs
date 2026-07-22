using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Components
{
    public class ExplosiveAuthoring : MonoBehaviour
    {
        public bool ExplodeOnStartBurn = true;
        public float Delay = 3f;
        public float ExplosionRadius = 3f;
        public float3 CustomExplosionImpulse = new(10f, 20f, 10f);
        public float CustomExplosionAngularImpulseMultiplier = 5f;
        public uint CustomExplosionAngularImpulseRandomSeed = 2;

        public class ExplosiveBaker : Baker<ExplosiveAuthoring>
        {
            public override void Bake(ExplosiveAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Explosive
                {
                    ExplodeOnStartBurn = authoring.ExplodeOnStartBurn,
                    Delay = authoring.Delay,
                    ExplosionRadius = authoring.ExplosionRadius,
                    CustomExplosionImpulse = authoring.CustomExplosionImpulse,
                    CustomExplosionAngularImpulseMultiplier = authoring.CustomExplosionAngularImpulseMultiplier,
                    CustomExplosionAngularImpulseRandomSeed = authoring.CustomExplosionAngularImpulseRandomSeed
                });
            }
        }
    }
}