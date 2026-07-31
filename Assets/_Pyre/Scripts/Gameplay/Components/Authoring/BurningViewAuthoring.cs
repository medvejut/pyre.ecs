using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public class BurningViewAuthoring : MonoBehaviour
    {
        public GameObject View;

        public class BurningViewBaker : Baker<BurningViewAuthoring>
        {
            public override void Bake(BurningViewAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var fireEntity = GetEntity(authoring.View, TransformUsageFlags.Renderable);

                AddComponent(entity, new BurningView { FireEntity = fireEntity });
            }
        }
    }
}