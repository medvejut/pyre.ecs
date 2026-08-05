using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    public enum EnemyAnimation : byte
    {
        Reset = 0,
        Idle = 1,
        Warning = 2,
        Burning = 3
    }

    public struct EnemyAnimationState : IComponentData
    {
        public EnemyAnimation State;

        public bool IsBurning;
        public bool IsWarning;

        public float WarningDelay;
        public float AnimationOffset;
    }
}