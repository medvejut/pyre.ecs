using System;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    [Serializable]
    public struct AudioDefaults : IComponentData
    {
        public UnityObjectRef<AudioClip> ExtinguishClip;
    }
}
