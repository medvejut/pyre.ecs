using Pyre.Audio;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public struct Ignitable : IComponentData
    {
        public float BurningRadius;
        public float IgnitionTime;
        public float CoolingRate;

        public UnityObjectRef<SoundClipSet> StartBurningSound;
        public UnityObjectRef<AudioClip> LoopSound;
        public Entity LoopAudioSourceEntity;
    }
}
