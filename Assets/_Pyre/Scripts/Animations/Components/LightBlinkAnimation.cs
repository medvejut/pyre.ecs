using Unity.Entities;

namespace Pyre.Animations.Components
{
    public struct LightBlinkAnimation : IComponentData
    {
        public float MinIntensity;
        public float MaxIntensity;
        public float Frequency;
        public float Irregularity;
        public float PhaseOffset;

        public float ElapsedTime;
    }
}