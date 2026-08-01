using Unity.Entities;
using UnityEngine;

namespace Pyre.Audio.Components
{
    public class MutedSoundAuthoring : MonoBehaviour
    {
        [SerializeField] private SoundKind[] mutedKinds;

        public class MutedSoundBaker : Baker<MutedSoundAuthoring>
        {
            public override void Bake(MutedSoundAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                foreach (var kind in authoring.mutedKinds)
                {
                    AppendToBuffer(entity, new MutedSound { Kind = kind });
                }
            }
        }
    }
}
