using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    public struct EnemySkeleton : IComponentData
    {
        public Entity Skeleton;
        public int Idle, Burn, Warning;
        public float FadeDuration;

        public readonly int ClipFor(EnemyAnimation animation)
        {
            switch (animation)
            {
                case EnemyAnimation.Warning: return Warning;
                case EnemyAnimation.Burning: return Burn;
                default: return Idle;
            }
        }
    }
}