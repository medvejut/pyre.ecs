using Pyre.Animations.Components;
using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    // What an explosive shows and sounds like while its fuse burns.
    // Optional - an explosive that goes off silently simply has no warning.
    public struct ExplosiveWarning : IComponentData
    {
        public Entity TickAudioSourceEntity;

        public bool PlayPulse;
        public PulseAnimation Pulse;

        public bool PlayBlink;
        public BlinkAnimation Blink;
    }
}
