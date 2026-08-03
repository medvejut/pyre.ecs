using System;
using Pyre.Audio;
using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    [Serializable]
    public struct AudioDefaults : IComponentData
    {
        public UnityObjectRef<SoundClipSet> ExtinguishSound;
    }
}
