using Pyre.Audio;
using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    public struct AudioDefaults : IComponentData
    {
        public UnityObjectRef<SoundClipSet> ExtinguishSound;
    }
}
