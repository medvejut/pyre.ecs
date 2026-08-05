using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    public struct EnemySkeleton : IComponentData
    {
        public Entity Skeleton;
        public int Idle, Fall, Warning;
    }
}