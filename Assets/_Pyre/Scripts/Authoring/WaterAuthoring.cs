using Pyre.Components;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Authoring
{
    public class WaterAuthoring : MonoBehaviour
    {
        public class WaterBaker : Baker<WaterAuthoring>
        {
            public override void Bake(WaterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Water>(entity);
            }
        }
    }
}