using System;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Audio.Components
{
    public class SoundClipOverrideAuthoring : MonoBehaviour
    {
        [Serializable]
        public struct Entry
        {
            public SoundKind Kind;
            public AudioClip Clip;
        }

        // Repeat a kind to stack several clips on one sound.
        [SerializeField] private Entry[] entries;

        public class SoundClipOverrideBaker : Baker<SoundClipOverrideAuthoring>
        {
            public override void Bake(SoundClipOverrideAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                foreach (var entry in authoring.entries)
                {
                    if (entry.Clip)
                    {
                        AppendToBuffer(entity, new SoundClipOverride { Kind = entry.Kind, Clip = entry.Clip });
                    }
                }
            }
        }
    }
}
