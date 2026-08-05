using Unity.Entities;

namespace Pyre.Player.Components
{
    public struct PlayerSkeleton : IComponentData
    {
        public Entity Skeleton;
        public int Idle, Walk;
    }
}