namespace Pyre.Audio.Components
{
    /// <summary>
    /// Values double as dense indices into the <see cref="DefaultSoundClip"/> singleton buffer.
    /// Only ever append new members; never reorder or renumber.
    /// </summary>
    public enum SoundKind : byte
    {
        Burn = 0,
        BurningLoop = 1,
        Extinguish = 2,
        Explode = 3
    }
}
