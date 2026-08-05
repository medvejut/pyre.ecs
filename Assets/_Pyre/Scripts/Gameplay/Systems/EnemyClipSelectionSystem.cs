using Pyre.Gameplay.Components;
using Pyre.Skeletons.Components;
using Pyre.Skeletons.Systems;
using Unity.Burst;
using Unity.Entities;

namespace Pyre.Gameplay.Systems
{
    // Uses the two SkeletonPose slots as from/to for a cross-fade between states, not as a blend tree.
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(SkeletonPoseSystem))]
    public partial struct EnemyClipSelectionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemySkeleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (link, animation) in SystemAPI
                         .Query<RefRO<EnemySkeleton>, RefRW<EnemyAnimationState>>())
            {
                var skeleton = link.ValueRO.Skeleton;

                if (!SystemAPI.HasComponent<SkeletonPose>(skeleton))
                {
                    continue;
                }

                var pose = SystemAPI.GetComponentRW<SkeletonPose>(skeleton);
                var target = link.ValueRO.ClipFor(animation.ValueRO.State);

                var shift = animation.ValueRO.AnimationOffset;

                if (pose.ValueRO.ClipB != target)
                {
                    pose.ValueRW.ClipB = target;
                    pose.ValueRW.TimeB = shift;
                    pose.ValueRW.Blend = 0f;

                    animation.ValueRW.AnimationOffset = 0f;
                }
                else if (shift != 0f)
                {
                    // Entering a state whose clip is already playing: no fade to ride in on, so the shift
                    // has to be applied to the live slot instead.
                    pose.ValueRW.TimeA = shift;
                    pose.ValueRW.TimeB = shift;

                    animation.ValueRW.AnimationOffset = 0f;
                }

                if (pose.ValueRO.ClipA == pose.ValueRO.ClipB)
                {
                    continue;
                }

                var fade = link.ValueRO.FadeDuration;
                var blend = fade > 0f ? pose.ValueRO.Blend + deltaTime / fade : 1f;

                if (blend < 1f)
                {
                    pose.ValueRW.Blend = blend;
                    continue;
                }

                pose.ValueRW.ClipA = pose.ValueRO.ClipB;
                pose.ValueRW.TimeA = pose.ValueRO.TimeB;
                pose.ValueRW.Blend = 0f;
            }
        }
    }
}
