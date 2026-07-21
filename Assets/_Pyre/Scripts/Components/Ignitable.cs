using Unity.Entities;

namespace Pyre.Components
{
    public struct Ignitable : IComponentData
    {
        public float BurningRadius;
        public float IgnitionTime;
        public float CoolingRate;
    }
}