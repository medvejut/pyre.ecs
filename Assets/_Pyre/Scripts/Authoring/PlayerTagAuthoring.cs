using Pyre.Components;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Authoring
{
    public class PlayerTagAuthoring : MonoBehaviour
    {
        public class PlayerTagBaker : Baker<PlayerTagAuthoring>
        {
            public override void Bake(PlayerTagAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(entity);
            }
        }
    }
}