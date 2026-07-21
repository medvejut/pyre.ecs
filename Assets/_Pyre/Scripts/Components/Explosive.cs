using Unity.Entities;

namespace Pyre.Components
{
    public struct Explosive : IComponentData
    {
        public bool ExplodeOnStartBurn;
        public float Delay;
    }
}