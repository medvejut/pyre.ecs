using Unity.Entities;
using UnityEngine;

namespace Pyre.Audio.Components
{
    public struct AudioDefaults : IComponentData
    {
        public UnityObjectRef<AudioClip> ExtinguishClip;
    }
}
