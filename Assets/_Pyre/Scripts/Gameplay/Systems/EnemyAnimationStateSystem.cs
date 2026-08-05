using Pyre.Gameplay.Components;
using Pyre.Gameplay.Utils;
using Pyre.Skeletons.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Pyre.Gameplay.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EnemyClipSelectionSystem))]
    public partial struct EnemyAnimationStateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemyAnimationState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (animation, entity) in SystemAPI
                         .Query<RefRW<EnemyAnimationState>>().WithEntityAccess())
            {
                animation.ValueRW.IsBurning = SystemAPI.HasComponent<Burning>(entity);
            }

            foreach (var (animation, physicsVelocity) in SystemAPI
                         .Query<RefRW<EnemyAnimationState>, RefRO<PhysicsVelocity>>())
            {
                animation.ValueRW.IsWarning = math.length(physicsVelocity.ValueRO.Linear) > 0.1f;
            }

            foreach (var (link, animation) in SystemAPI
                         .Query<RefRO<EnemySkeleton>, RefRW<EnemyAnimationState>>())
            {
                var currentState = animation.ValueRO.State;
                var targetState = currentState;

                switch (currentState)
                {
                    case EnemyAnimation.Reset:
                        targetState = EnemyAnimation.Idle;
                        break;

                    case EnemyAnimation.Idle:
                        OnIdleUpdate(ref state, animation);
                        targetState = GetIdleTransition(animation.ValueRO);
                        break;

                    case EnemyAnimation.Burning:
                        targetState = GetBurningTransition(animation.ValueRO);
                        break;

                    case EnemyAnimation.Warning:
                        targetState = GetWarningTransition(ref state, link.ValueRO, animation.ValueRO);
                        break;
                }

                if (currentState != targetState)
                {
                    animation.ValueRW.State = targetState;

                    if (targetState == EnemyAnimation.Idle)
                    {
                        OnIdleEnter(animation);
                    }

                    if (targetState == EnemyAnimation.Warning)
                    {
                        OnWarningEnter(animation);
                    }
                }
            }
        }

        private EnemyAnimation GetIdleTransition(in EnemyAnimationState animation)
        {
            if (animation.IsBurning)
            {
                return EnemyAnimation.Burning;
            }

            if (animation is { IsWarning: true, WarningDelay: <= 0 })
            {
                return EnemyAnimation.Warning;
            }

            return EnemyAnimation.Idle;
        }

        private EnemyAnimation GetBurningTransition(in EnemyAnimationState animation)
        {
            if (!animation.IsBurning)
            {
                return EnemyAnimation.Idle;
            }

            return EnemyAnimation.Burning;
        }

        private EnemyAnimation GetWarningTransition(ref SystemState state, in EnemySkeleton link, in EnemyAnimationState animation)
        {
            if (animation.IsBurning)
            {
                return EnemyAnimation.Burning;
            }

            if (SystemAPI.HasComponent<SkeletonPose>(link.Skeleton))
            {
                var skeleton = SystemAPI.GetComponent<SkeletonPose>(link.Skeleton);
                if (SkeletonUtils.HasCurrentAnimationFinished(skeleton))
                {
                    return EnemyAnimation.Idle;
                }
            }

            return EnemyAnimation.Warning;
        }

        private void OnIdleUpdate(ref SystemState state, RefRW<EnemyAnimationState> animation)
        {
            animation.ValueRW.WarningDelay -= SystemAPI.Time.DeltaTime;
        }

        private void OnIdleEnter(RefRW<EnemyAnimationState> animation)
        {
            // Offset the idle animation to avoid syncing with the player's idle animation
            animation.ValueRW.AnimationOffset = 0.5f;
        }

        private void OnWarningEnter(RefRW<EnemyAnimationState> animation)
        {
            animation.ValueRW.WarningDelay = 0.3f;
        }
    }
}