using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public struct AudioDefaults : IComponentData
    {
        public UnityObjectRef<AudioClip> ExtinguishClip;
    }
}
