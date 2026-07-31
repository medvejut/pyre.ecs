using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public class MaterialPropertyProgressAuthoring : MonoBehaviour
    {
        public float Value;

        public class ProgressMaterialPropertyBaker : Baker<MaterialPropertyProgressAuthoring>
        {
            public override void Bake(MaterialPropertyProgressAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent(entity, new ProgressMaterialProperty { Value = authoring.Value });
            }
        }
    }
}