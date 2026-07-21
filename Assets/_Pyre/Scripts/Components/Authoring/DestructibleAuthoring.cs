using Unity.Entities;
using UnityEngine;

namespace Pyre.Components
{
    public class DestructibleAuthoring : MonoBehaviour
    {
        public class DestructibleBaker : Baker<DestructibleAuthoring>
        {
            public override void Bake(DestructibleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Destructible>(entity);
            }
        }
    }
}