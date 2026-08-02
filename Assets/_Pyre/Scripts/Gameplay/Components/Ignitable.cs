using Pyre.Audio;
using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    public struct Ignitable : IComponentData
    {
        public float BurningRadius;
        public float IgnitionTime;
        public float CoolingRate;

        public UnityObjectRef<SoundClipSet> IgniteSound;
        public UnityObjectRef<SoundClipSet> LoopSound;
        public UnityObjectRef<SoundClipSet> ExtinguishSound;
    }
}
