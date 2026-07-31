using Unity.Entities;
using UnityEngine;

namespace Pyre.Transforms.Components
{
    public class FreezeWorldRotationAuthoring : MonoBehaviour
    {
        public class FreezeWorldRotationBaker : Baker<FreezeWorldRotationAuthoring>
        {
            public override void Bake(FreezeWorldRotationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FreezeWorldRotation { WorldRotation = authoring.transform.rotation });
            }
        }
    }
}