using Unity.Entities;

namespace Pyre.Components
{
    public struct ExplodeTimer : IComponentData
    {
        public float TimeRemaining;
    }
}