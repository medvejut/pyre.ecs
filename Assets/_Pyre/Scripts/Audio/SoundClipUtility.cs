using Pyre.Audio.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Audio
{
    /// <summary>
    /// Resolves which clip a given <see cref="SoundKind"/> plays on an entity:
    /// per-entity <see cref="SoundClipOverride"/> first, else the global
    /// <see cref="DefaultSoundClip"/>, and nothing at all if the entity has a
    /// matching <see cref="MutedSound"/> entry.
    /// </summary>
    public static class SoundClipUtility
    {
        public static bool IsMuted(SoundKind kind, Entity entity, BufferLookup<MutedSound> mutedLookup)
        {
            if (!mutedLookup.TryGetBuffer(entity, out var mutedSounds))
                return false;

            foreach (var mutedSound in mutedSounds)
            {
                if (mutedSound.Kind == kind)
                    return true;
            }

            return false;
        }

        public static UnityObjectRef<AudioClip> GetDefault(SoundKind kind, DynamicBuffer<DefaultSoundClip> defaults)
        {
            var index = (int)kind;
            return defaults.IsCreated && index < defaults.Length ? defaults[index].Clip : default;
        }

        /// <summary>
        /// First override for the kind, else the global default. For consumers that need exactly one clip.
        /// </summary>
        public static UnityObjectRef<AudioClip> Resolve(SoundKind kind, Entity entity, BufferLookup<SoundClipOverride> overrideLookup, DynamicBuffer<DefaultSoundClip> defaults)
        {
            if (overrideLookup.TryGetBuffer(entity, out var overrides))
            {
                foreach (var clipOverride in overrides)
                {
                    if (clipOverride.Kind == kind && clipOverride.Clip)
                        return clipOverride.Clip;
                }
            }

            return GetDefault(kind, defaults);
        }

        /// <summary>
        /// Queues every override for the kind; if there are none, queues the global default.
        /// Queues nothing if the entity mutes this kind.
        /// </summary>
        public static void Queue(SoundKind kind, Entity entity, float3 position, float spatialBlend, BufferLookup<SoundClipOverride> overrideLookup, BufferLookup<MutedSound> mutedLookup, DynamicBuffer<DefaultSoundClip> defaults, DynamicBuffer<SoundEvent> soundEvents)
        {
            if (IsMuted(kind, entity, mutedLookup))
                return;

            var queued = false;

            if (overrideLookup.TryGetBuffer(entity, out var overrides))
            {
                foreach (var clipOverride in overrides)
                {
                    if (clipOverride.Kind != kind || !clipOverride.Clip)
                        continue;

                    soundEvents.Add(new SoundEvent { Position = position, Clip = clipOverride.Clip, SpatialBlend = spatialBlend });
                    queued = true;
                }
            }

            if (queued)
                return;

            var fallback = GetDefault(kind, defaults);
            if (fallback)
            {
                soundEvents.Add(new SoundEvent { Position = position, Clip = fallback, SpatialBlend = spatialBlend });
            }
        }
    }
}
