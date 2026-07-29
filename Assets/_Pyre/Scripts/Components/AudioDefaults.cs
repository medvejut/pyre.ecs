using Unity.Entities;
using UnityEngine;

namespace Pyre.Components
{
    public struct AudioDefaults : IComponentData
    {
        public UnityObjectRef<AudioClip> ExtinguishClip;
    }
}
