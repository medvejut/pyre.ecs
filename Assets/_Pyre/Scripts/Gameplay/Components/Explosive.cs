using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    public struct Explosive : IComponentData
    {
        public bool ExplodeOnStartBurn;
        public float Delay;
    }
}