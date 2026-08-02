using System;
using Pyre.Audio;
using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    /// <summary>
    /// Звуки, которые незачем задавать на каждой сущности отдельно.
    /// Заводить сюда звук стоит только если он действительно общий для всех.
    /// </summary>
    [Serializable]
    public struct AudioDefaults : IComponentData
    {
        public UnityObjectRef<SoundClipSet> ExtinguishSound;
    }
}
