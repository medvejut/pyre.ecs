using Pyre.Components;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Authoring
{
    public class IgnitableAuthoring : MonoBehaviour
    {
        [SerializeField] private float burningRadius = 1f;

        public class IgnitableBaker : Baker<IgnitableAuthoring>
        {
            public override void Bake(IgnitableAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Ignitable { BurningRadius = authoring.burningRadius });
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, burningRadius);
        }
    }
}