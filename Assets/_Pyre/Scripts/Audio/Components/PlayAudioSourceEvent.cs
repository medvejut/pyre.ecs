using Unity.Entities;

namespace Pyre.Audio.Components
{
    public struct PlayAudioSourceEvent : IBufferElementData
    {
        public Entity AudioSourceEntity;
    }
}