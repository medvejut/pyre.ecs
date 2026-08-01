using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    public struct Ignitable : IComponentData
    {
        public float BurningRadius;
        public float IgnitionTime;
        public float CoolingRate;
    }
}