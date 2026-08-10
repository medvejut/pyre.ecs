using Pyre.Animations.Components;
using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    public struct ExplosiveWarning : IComponentData
    {
        public Entity TickAudioSourceEntity;

        public bool PlayPulse;
        public PulseAnimation Pulse;

        public bool PlayBlink;
        public BlinkAnimation Blink;
    }
}