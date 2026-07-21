using Pyre.Components;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Authoring
{
    public class IgnitableAuthoring : MonoBehaviour
    {
        [SerializeField] private float burningRadius = 1f;
        [SerializeField] private float ignitionTime = 1f;
        [SerializeField] private float coolingRate = 0.5f;

        public class IgnitableBaker : Baker<IgnitableAuthoring>
        {
            public override void Bake(IgnitableAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Ignitable
                {
                    BurningRadius = authoring.burningRadius,
                    IgnitionTime = authoring.ignitionTime,
                    CoolingRate = authoring.coolingRate
                });
                AddComponent(entity, new IgnitionProgress { Elapsed = 0f });
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, burningRadius);
        }
    }
}