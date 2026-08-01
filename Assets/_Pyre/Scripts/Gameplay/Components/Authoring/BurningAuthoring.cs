using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public class BurningAuthoring : MonoBehaviour
    {
        [SerializeField] public float heatRadius = 1f;

        public class BurningBaker : Baker<BurningAuthoring>
        {
            public override void Bake(BurningAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Burning { HeatRadius = authoring.heatRadius });
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Origins sit at the model's feet, so draw from the collider center to match the runtime query.
            var bodyCollider = GetComponentInChildren<Collider>();
            var center = bodyCollider ? bodyCollider.bounds.center : transform.position;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, heatRadius);
        }
    }
}