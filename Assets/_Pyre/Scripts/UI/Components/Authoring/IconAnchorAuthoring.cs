using Unity.Entities;
using UnityEngine;

namespace Pyre.UI.Components
{
    public class IconAnchorAuthoring : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new(0f, 2.25f, 0f);

        public class IconAnchorBaker : Baker<IconAnchorAuthoring>
        {
            public override void Bake(IconAnchorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new IconAnchor { Offset = authoring.offset });
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + offset, 0.15f);
        }
    }
}