using Pyre.Components;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Authoring
{
    public class BillboardAuthoring : MonoBehaviour
    {
        public class BillboardBaker : Baker<BillboardAuthoring>
        {
            public override void Bake(BillboardAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Billboard>(entity);
            }
        }
    }
}