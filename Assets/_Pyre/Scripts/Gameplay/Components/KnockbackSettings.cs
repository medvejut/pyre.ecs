using System;
using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    [Serializable]
    public struct KnockbackSettings : IComponentData
    {
        public float LinearDamping;
        public float AngularDamping;
    }
}