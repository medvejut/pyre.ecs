using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public class MuteBurnSoundAuthoring : MonoBehaviour
    {
        public class MuteBurnSoundBaker : Baker<MuteBurnSoundAuthoring>
        {
            public override void Bake(MuteBurnSoundAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<MuteBurnSound>(entity);
            }
        }
    }
}
