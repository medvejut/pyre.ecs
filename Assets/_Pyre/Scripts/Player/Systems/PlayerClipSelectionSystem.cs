using Pyre.Player.Components;
using Pyre.Skeletons.Components;
using Pyre.Skeletons.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Pyre.Player.Systems
{
    // Uses the two SkeletonPose slots as a 1D blend tree along ground speed: idle in A, walk in B.
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(SkeletonPoseSystem))]
    public partial struct PlayerClipSelectionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerSkeleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (skeleton, velocity, movement)in SystemAPI
                         .Query<RefRO<PlayerSkeleton>, RefRO<PhysicsVelocity>, RefRO<PlayerMovement>>())
            {
                if (!SystemAPI.HasComponent<SkeletonPose>(skeleton.ValueRO.Skeleton))
                    continue;

                var skeletonPose = SystemAPI.GetComponentRW<SkeletonPose>(skeleton.ValueRO.Skeleton);

                skeletonPose.ValueRW.ClipA = skeleton.ValueRO.Idle;
                skeletonPose.ValueRW.ClipB = skeleton.ValueRO.Walk;

                var speed = math.saturate(math.length(velocity.ValueRO.Linear.xz) / movement.ValueRO.MoveSpeed);
                skeletonPose.ValueRW.Blend = speed;
            }
        }
    }
}