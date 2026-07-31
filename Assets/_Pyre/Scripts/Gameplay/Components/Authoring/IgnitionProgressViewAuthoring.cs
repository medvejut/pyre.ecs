using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public class IgnitionProgressViewAuthoring : MonoBehaviour
    {
        public GameObject View;

        public class IgnitionProgressViewBaker : Baker<IgnitionProgressViewAuthoring>
        {
            public override void Bake(IgnitionProgressViewAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var progressEntity = GetEntity(authoring.View, TransformUsageFlags.Renderable);

                AddComponent(entity, new IgnitionProgressView { ProgressEntity = progressEntity });
            }
        }
    }
}