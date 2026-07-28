using Unity.Entities;
using UnityEngine;

namespace Pyre.Components
{
    public struct Ignitable : IComponentData
    {
        public float BurningRadius;
        public float IgnitionTime;
        public float CoolingRate;

        public UnityObjectRef<AudioClip> OnBurnClip;
        public UnityObjectRef<AudioClip> LoopClip;
        public UnityObjectRef<AudioClip> ExtinguishClip;
    }
}