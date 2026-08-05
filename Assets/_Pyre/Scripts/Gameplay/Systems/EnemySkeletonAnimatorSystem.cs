using Pyre.Gameplay.Components;
using Pyre.Skeletons.Components;
using Pyre.Skeletons.Systems;
using Unity.Burst;
using Unity.Entities;

namespace Pyre.Gameplay.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(SkeletonPoseSystem))]
    public partial struct EnemySkeletonAnimatorSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (skeleton, entity)in SystemAPI
                         .Query<RefRO<EnemySkeleton>>().WithEntityAccess())
            {
                if (!SystemAPI.HasComponent<SkeletonPose>(skeleton.ValueRO.Skeleton))
                    continue;

                var isBurning = SystemAPI.HasComponent<Burning>(entity);

                var skeletonPose = SystemAPI.GetComponentRW<SkeletonPose>(skeleton.ValueRO.Skeleton);

                skeletonPose.ValueRW.ClipA = skeleton.ValueRO.Idle;
                skeletonPose.ValueRW.ClipB = skeleton.ValueRO.Fall;

                skeletonPose.ValueRW.Blend = isBurning ? 1 : 0;
            }
        }
    }
}