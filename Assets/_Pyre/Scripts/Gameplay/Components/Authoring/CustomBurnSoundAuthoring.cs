using Pyre.Audio.Components;
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

                foreach (var clip in authoring.clips)
                {
                    if (clip)
                    {
                        AppendToBuffer(entity, new SoundClipOverride { Kind = SoundKind.Burn, Clip = clip });
                    }
                }
            }
        }
    }
}
