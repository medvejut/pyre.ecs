using Unity.Entities;
using UnityEngine;

namespace Pyre.Components
{
    public class ExplosiveAuthoring : MonoBehaviour
    {
        public bool ExplodeOnStartBurn = true;
        public float Delay = 3f;
        public float ExplosionRadius = 3f;

        public class ExplosiveBaker : Baker<ExplosiveAuthoring>
        {
            public override void Bake(ExplosiveAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Explosive { ExplodeOnStartBurn = authoring.ExplodeOnStartBurn, Delay = authoring.Delay, ExplosionRadius = authoring.ExplosionRadius });
            }
        }
    }
}