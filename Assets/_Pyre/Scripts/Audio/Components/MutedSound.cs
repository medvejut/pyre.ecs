using Unity.Entities;

namespace Pyre.Audio.Components
{
    /// <summary>
    /// Presence of an entry silences that <see cref="SoundKind"/> on this entity.
    /// </summary>
    [InternalBufferCapacity(2)]
    public struct MutedSound : IBufferElementData
    {
        public SoundKind Kind;
    }
}
