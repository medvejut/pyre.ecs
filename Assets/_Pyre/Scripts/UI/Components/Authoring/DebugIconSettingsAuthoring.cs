using Unity.Entities;
using UnityEngine;

namespace Pyre.UI.Components
{
    public class DebugIconSettingsAuthoring : MonoBehaviour
    {
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private Vector3 offset = new(0f, 2.25f, 0f);

        public class DebugIconSettingsBaker : Baker<DebugIconSettingsAuthoring>
        {
            public override void Bake(DebugIconSettingsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new DebugIconSettings { Enabled = authoring.isEnabled, Offset = authoring.offset });
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + offset, 0.15f);
        }
    }
}