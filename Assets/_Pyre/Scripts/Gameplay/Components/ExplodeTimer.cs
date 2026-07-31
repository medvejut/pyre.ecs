using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    public struct ExplodeTimer : IComponentData
    {
        public float TimeRemaining;
    }
}