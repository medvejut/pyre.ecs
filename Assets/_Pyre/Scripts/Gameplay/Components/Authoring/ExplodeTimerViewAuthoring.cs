using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public class ExplodeTimerViewAuthoring : MonoBehaviour
    {
        public GameObject View;

        public class ExplodeTimerViewBaker : Baker<ExplodeTimerViewAuthoring>
        {
            public override void Bake(ExplodeTimerViewAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var progressEntity = GetEntity(authoring.View, TransformUsageFlags.Renderable);

                AddComponent(entity, new ExplodeTimerView { ProgressEntity = progressEntity });
            }
        }
    }
}