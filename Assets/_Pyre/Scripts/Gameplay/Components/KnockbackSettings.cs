using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    public struct KnockbackSettings : IComponentData
    {
        public float LinearDamping;
        public float AngularDamping;
    }
}