using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public class CustomBurnSoundAuthoring : MonoBehaviour
    {
        [SerializeField] private AudioClip[] clips;

        public class CustomBurnSoundBaker : Baker<CustomBurnSoundAuthoring>
        {
            public override void Bake(CustomBurnSoundAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var buffer = AddBuffer<BurnSoundClip>(entity);
                foreach (var clip in authoring.clips)
                {
                    buffer.Add(new BurnSoundClip { Clip = clip });
                }
            }
        }
    }
}
