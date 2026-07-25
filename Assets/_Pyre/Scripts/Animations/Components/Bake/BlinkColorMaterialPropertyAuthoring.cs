using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Animations.Components.Bake
{
    public class BlinkColorMaterialPropertyAuthoring : MonoBehaviour
    {
        public Color InitialColor = new(1f, 1f, 1f, 0f);

        public class BlinkColorMaterialPropertyBaker : Baker<BlinkColorMaterialPropertyAuthoring>
        {
            public override void Bake(BlinkColorMaterialPropertyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                var color = authoring.InitialColor;
                AddComponent(entity, new BlinkColorMaterialProperty
                {
                    Value = new float4(color.r, color.g, color.b, color.a)
                });
            }
        }
    }
}