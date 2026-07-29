using Unity.Entities;

namespace Pyre.Audio.Components
{
    public struct StopAudioSourceEvent : IBufferElementData
    {
        public Entity AudioSourceEntity;
    }
}